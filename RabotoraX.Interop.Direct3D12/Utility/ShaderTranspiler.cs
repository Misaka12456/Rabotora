using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Vortice.Dxc;
using Vortice.SpirvCross;
using IDxcCompiler3 = Vortice.Dxc.IDxcCompiler3;
using static Vortice.SpirvCross.SpirvCrossApi;

namespace RabotoraX.Interop.Direct3D12.Utility;

public static class ShaderTranspiler
{
	public static unsafe string CrossCompileHlslToSksl(string hlslSource, string entryPoint, string profile)
	{
		using var compiler = Dxc.CreateDxcCompiler<IDxcCompiler3>();
		string[] args = ["-E", entryPoint, "-T", profile, "-spirv", "-fspv-target-env=vulkan1.0", "-fvk-use-dx-layout"];
		using var result = compiler.Compile(hlslSource, args, null!);
		if (result.GetStatus().Failure)
		{
			// Fix: Convert the byte buffer to a readable string
			var errorBuffer = result.GetErrorBuffer();
			string errorMsg = System.Text.Encoding.UTF8.GetString(errorBuffer.AsBytes());
			throw new Exception($"DXC Error: {errorMsg}");
		}

		using var spirvBlob = result.GetOutput(DxcOutKind.Object);
		var spirvSpan = spirvBlob.AsSpan();

		spvc_context context = default;
		spvc_parsed_ir ir = default;
		spvc_compiler compiler_glsl = default;
		spvc_compiler_options options = default;

		try
		{
			spvc_context_create(&context).CheckResult();

			fixed (byte* pSpirv = spirvSpan)
			{
				spvc_context_parse_spirv(context, (uint*) pSpirv, (nuint) (spirvSpan.Length / 4), &ir).CheckResult();
			}

			spvc_context_create_compiler(context, Backend.GLSL, ir, CaptureMode.TakeOwnership, &compiler_glsl).CheckResult();
			spvc_compiler_create_compiler_options(compiler_glsl, &options).CheckResult();
			spvc_compiler_build_combined_image_samplers(compiler_glsl).CheckResult();
			spvc_compiler_options_set_uint(options, CompilerOption.GLSLVersion, 300).CheckResult();
			spvc_compiler_options_set_bool(options, CompilerOption.GLSLES, 1).CheckResult();
			spvc_compiler_options_set_bool(options, CompilerOption.GLSLEmitUniformBufferAsPlainUniforms, 1).CheckResult();
			spvc_compiler_build_combined_image_samplers(compiler_glsl).CheckResult();

			byte* resultPtr = null;
			spvc_compiler_compile(compiler_glsl, &resultPtr).CheckResult();

			string glsl = Marshal.PtrToStringAnsi((nint) resultPtr) ?? "";
			return PostProcessForSksl(glsl);
		}
		finally
		{
			if (context.Handle != nint.Zero) spvc_context_destroy(context);
		}
	}

	private static string PostProcessForSksl(string glsl)
	{
		// 1. Strip Headers & Qualifiers
		string cleaned = Regex.Replace(glsl, @"#version.*", "");
		cleaned = Regex.Replace(cleaned, @"precision\s+\w+\s+\w+;", "");
		cleaned = Regex.Replace(cleaned, @"layout\(location\s*=\s*\d+\)\s*", "");
		cleaned = Regex.Replace(cleaned, @"\bhighp\b|\bmediump\b|\blowp\b", "");

		// 2. Map types early
		cleaned = cleaned.Replace("vec4", "half4").Replace("vec3", "half3");

		// 3. Identify and STRIP global 'in' and 'out' declarations
		// Skia does not allow 'in' or 'out' variables at the top level.
		var outMatch = Regex.Match(cleaned, @"out\s+half4\s+(\w+);");
		string outName = outMatch.Success ? outMatch.Groups[1].Value : "out_var_SV_Target";
		cleaned = Regex.Replace(cleaned, @"out\s+half4\s+\w+;", ""); // DELETE the line

		var uvMatch = Regex.Match(cleaned, @"in\s+vec2\s+(\w+);");
		string uvName = uvMatch.Success ? uvMatch.Groups[1].Value : "in_var_TEXCOORD";
		cleaned = Regex.Replace(cleaned, @"in\s+vec2\s+\w+;", ""); // DELETE the line

		// 4. Handle Textures (Identify and replace sampler2D)
		var textureNames = new List<string>();
		cleaned = Regex.Replace(cleaned, @"uniform\s+sampler2D\s+(\w+);", m =>
		{
			textureNames.Add(m.Groups[1].Value);
			return $"uniform shader {m.Groups[1].Value};";
		});

		// 5. Transform Main function
		cleaned = cleaned.Replace("void main()", "half4 main(float2 fragCoord)");

		// 6. Map Logic inside the function
		foreach (var tex in textureNames)
		{
			// Replace texture(_45, in_var_TEXCOORD) -> _45.eval(fragCoord)
			// Note: Using fragCoord assumes you want 1:1 pixel mapping
			cleaned = cleaned.Replace($"texture({tex}, {uvName})", $"{tex}.eval(fragCoord)");
		}

		// 7. Convert the assignment to the former 'out' variable into a 'return'
		// This finds "out_var_SV_Target = ..." and turns it into "return ..."
		cleaned = Regex.Replace(cleaned, $@"\b{outName}\s*=\s*", "return ");

		return cleaned;
	}
}