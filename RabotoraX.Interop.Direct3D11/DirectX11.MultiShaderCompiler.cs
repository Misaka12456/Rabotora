using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RabotoraX.Core.Graphics;
using Vortice.D3DCompiler;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace RabotoraX.Interop.Direct3D11;

public partial class DirectX11 : IMultiPlatformShaderCompiler
{
	private readonly Dictionary<string, Guid> _registeredEffects = new();
	private readonly Dictionary<Guid, ID2D1Effect> _effectInstances = new();
	
	internal ID2D1Effect GetEffectInstance(Guid effectId)
	{
		if (_effectInstances.TryGetValue(effectId, out var effect))
		{
			return effect;
		}
		throw new Exception($"Effect instance {effectId} not initialized. D2D Custom Effects require registration.");
	}
	
	private Guid RegisterD2DEffect(string fragSource)
	{
		if (_registeredEffects.TryGetValue(fragSource, out var existingId))
			return existingId;

		Guid effectId = Guid.NewGuid();
       
		// 修复：补全参数 (sourceName: "none")
		// 注意：D2D 效果的编译通常需要 ps_5_0 或针对 D2D 的特定编译标志
		var shaderByteCode = Compiler.Compile(fragSource, "main", "none", "ps_5_0");
       
		// TODO: 使用 _d2dContext.RegisterEffectFromXml 或类似逻辑将 byteCode 绑定到 effectId
       
		_registeredEffects[fragSource] = effectId;
		return effectId;
	}
	
	public void CompileInto(INativeShader target, ShaderPlatform platform, string vert, string frag)
	{
		if (target is not DX11CombinedShader combined) return;
		if (platform == ShaderPlatform.HLSL2D11)
		{
			combined.D2DEffectId = RegisterD2DEffect(frag);
		}
		else if (platform == ShaderPlatform.HLSL11)
		{
			var vsByteCode = Compiler.Compile(vert, "VSMain", "none", "vs_5_0");
			combined.VertShader = _device!.CreateVertexShader(vsByteCode.Span);

			var layoutDesc = Vertex2D.GetLayout();
			var dxElements = layoutDesc.Select(e => new Vortice.Direct3D11.InputElementDescription(
				e.SemanticName, (uint)e.SemanticIndex, MapFormat(e.Format), (uint)e.AlignedByteOffset, (uint)e.InputSlot,
				InputClassification.PerVertexData, 0)).ToArray();
			combined.InputLayout = _device.CreateInputLayout(dxElements, vsByteCode.Span);

			var psByteCode = Compiler.Compile(frag, "PSMain", "none", "ps_5_0");
			combined.FragShader = _device!.CreatePixelShader(psByteCode.Span);
		}
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vortice.DXGI.Format MapFormat(GpuFormat format) => format switch
	{
		GpuFormat.R32G32B32_Float => Vortice.DXGI.Format.R32G32B32_Float,
		GpuFormat.R32G32_Float => Vortice.DXGI.Format.R32G32_Float,
		GpuFormat.R32G32B32A32_Float => Vortice.DXGI.Format.R32G32B32A32_Float,
		GpuFormat.R8G8B8A8_UNorm => Vortice.DXGI.Format.R8G8B8A8_UNorm,
		_ => Vortice.DXGI.Format.Unknown
	};
}