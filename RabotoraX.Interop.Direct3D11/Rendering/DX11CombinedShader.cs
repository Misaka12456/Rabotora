using System;
using RabotoraX.Core.Graphics;
using Vortice.Direct3D11;

namespace RabotoraX.Interop.Direct3D11;

internal class DX11CombinedShader : INativeShader
{
	public ID3D11VertexShader? VertShader { get; set; }
	public ID3D11PixelShader? FragShader { get; set; }
	public string? VertSource { get; }
	public string? FragSource { get; }
	public ID3D11InputLayout? InputLayout { get; set; }
	
	public Guid? D2DEffectId { get; set; } // 如果这个着色器是为Direct2D效果创建的，那么就存储对应的EffectId，供后续创建D2D效果时使用

	public void Dispose()
	{
		VertShader?.Dispose();
		FragShader?.Dispose();
		InputLayout?.Dispose();
	}
}