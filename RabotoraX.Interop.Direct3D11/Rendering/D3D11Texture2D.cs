using System;
using RabotoraX.Core.Graphics;
using Vortice.Direct3D11;

namespace RabotoraX.Interop.Direct3D11.Rendering;

public class D3D11Texture2D : INativeTexture2D
{
	public ID3D11Texture2D Texture { get; }
	public ID3D11ShaderResourceView SRV { get; }
	public int Width { get; }
	public int Height { get; }

	public D3D11Texture2D(ID3D11Texture2D texture, ID3D11ShaderResourceView srv, int width, int height)
	{
		Texture = texture;
		SRV = srv;
		Width = width;
		Height = height;
	}

	public void Dispose()
	{
		SRV.Dispose();
		Texture.Dispose();
		GC.SuppressFinalize(this);
	}
}