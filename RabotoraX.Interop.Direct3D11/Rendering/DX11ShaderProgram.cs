using System;
using RabotoraX.Core.Graphics;
using Vortice.Direct3D11;

namespace RabotoraX.Interop.Direct3D11;

public class DX11ShaderProgram : INativeShader
{
	public ID3D11VertexShader? VertexShader { get; }
	public ID3D11PixelShader? FragmentShader { get; }
	public ID3D11InputLayout? InputLayout { get; }
	public string? VertSource { get; init; }
	public string? FragSource { get; init; }

	public ShaderType Type => ShaderType.VertexFragment;
	
	public DX11ShaderProgram(ID3D11VertexShader? vs, ID3D11PixelShader? ps, ID3D11InputLayout? inputLayout)
	{
		VertexShader = vs;
		FragmentShader = ps;
		InputLayout = inputLayout;
	}

	public void Dispose()
	{
		VertexShader?.Dispose();
		FragmentShader?.Dispose();
		InputLayout?.Dispose();
		GC.SuppressFinalize(this);
	}
}