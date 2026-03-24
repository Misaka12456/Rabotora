using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace RabotoraX.Core.Graphics;

public interface INativeShader : IDisposable
{
	public string? VertSource { get; }
	public string? FragSource { get; }
}

public enum BlendFactor
{
	Zero, One,
	SrcColor, OneMinusSrcColor,
	DstColor, OneMinusDstColor,
	SrcAlpha, OneMinusSrcAlpha,
	DstAlpha, OneMinusDstAlpha,
	ConstantColor, OneMinusConstantColor,
	ConstantAlpha, OneMinusConstantAlpha,
	SrcAlphaSaturate
}

public enum BlendOperation
{
	Add, Subtract, ReverseSubtract, Min, Max
}

public partial record struct BlendState
{
	public bool EnableBlending;
	public BlendFactor SrcColor, DstColor;
	public BlendOperation ColorOp;
	
	public BlendFactor SrcAlpha, DstAlpha;
	public BlendOperation AlphaOp;
}

public partial record struct BlendState
{
	public readonly static BlendState AlphaBlend = new() // Straight Alpha Blending (SrcAlpha OneMinusSrcAlpha)
	{
		EnableBlending = true,
		SrcColor = BlendFactor.SrcAlpha, DstColor = BlendFactor.OneMinusSrcAlpha, ColorOp = BlendOperation.Add,
		SrcAlpha = BlendFactor.One, DstAlpha = BlendFactor.Zero, AlphaOp = BlendOperation.Add
	};
	public readonly static BlendState Additive = new() // Straight Alpha Blending with Additive Color
	{
		EnableBlending = true,
		SrcColor = BlendFactor.SrcAlpha, DstColor = BlendFactor.One, ColorOp = BlendOperation.Add,
		SrcAlpha = BlendFactor.One, DstAlpha = BlendFactor.Zero, AlphaOp = BlendOperation.Add
	};
	public readonly static BlendState Premultiplied = new() // Premultiplied Alpha Blending (One OneMinusSrcAlpha)
	{
		EnableBlending = true,
		SrcColor = BlendFactor.One, DstColor = BlendFactor.OneMinusSrcAlpha, ColorOp = BlendOperation.Add,
		SrcAlpha = BlendFactor.One, DstAlpha = BlendFactor.Zero, AlphaOp = BlendOperation.Add
	};
}

public interface IShader : IDisposable
{
	string Name { get; }
	BlendState BlendState { get; }
	bool ZWrite { get; }
	
	INativeShader Compile(INativeGraphicsAPI graphicsApi, bool noCache = false);
}

public enum ShaderPlatform
{
	Unavailable = 0,
	HLSL11 = 1, // Direct3D 11 HLSL
	HLSL2D11 = 2, // Direct2D 11 HLSL (for Direct2D effects -- Fragment Shader Only)
	HLSL12 = 3, // Direct3D 12 HLSL
	HLSLVulkan = 3, // Vulkan HLSL (via DXC)
	GLSL = 10, // OpenGL GLSL
	GLSLVulkan = 11, // Vulkan GLSL
	Metal = 20, // Apple Metal Shading Language
}

public class RShader : Object, IShader
{
	public string Name { get; init; }
	public BlendState BlendState { get; set; } = BlendState.AlphaBlend;
	public bool ZWrite { get; set; } = true;
	
	private readonly ConcurrentDictionary<ShaderPlatform, (string Vert, string Frag)> _platformSources = new();
	private INativeShader? _cachedNativeShader;

	public RShader(string name)
	{
		Name = name;
	}
	
	public void AddSource(ShaderPlatform platform, string vertSource, string fragSource)
	{
		_platformSources[platform] = (vertSource, fragSource);
	}

	public INativeShader Compile(INativeGraphicsAPI graphicsApi, bool noCache = false)
	{
		if (_cachedNativeShader != null && !noCache) return _cachedNativeShader;

		var supportedPlatforms = graphicsApi.GetSupportedShaderPlatforms().ToImmutableArray();
		ShaderPlatform? primaryPlatform = null;
		foreach (var p in supportedPlatforms)
		{
			if (_platformSources.ContainsKey(p))
			{
				primaryPlatform = p;
				break;
			}
		}

		if (primaryPlatform == null)
		{
			throw new NotSupportedException($"Shader '{Name}' does not have a compatible source for the current graphics API ({graphicsApi.ApiName}). Supported platforms: {string.Join(", ", supportedPlatforms)}");
		}
		
		var primarySource = _platformSources[primaryPlatform.Value];
		_cachedNativeShader = graphicsApi.CreateNativeShaderProgram(primarySource.Vert, primarySource.Frag, []);

		if (graphicsApi is IMultiPlatformShaderCompiler multiCompiler)
		{
			foreach (var platform in _platformSources.Keys)
			{
				if (platform == primaryPlatform) continue; // 已经编译过了
				if (supportedPlatforms.Contains(platform))
				{
					var s =_platformSources[platform];
					multiCompiler.CompileInto(_cachedNativeShader, platform, s.Vert, s.Frag);
				}
			}
		}
		
		return _cachedNativeShader;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_cachedNativeShader?.Dispose();
			_cachedNativeShader = null;
		}
		base.Dispose(disposing);
	}
}