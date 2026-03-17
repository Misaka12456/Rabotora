using System;
using RabotoraX.Core.Graphics;
using Vortice.Direct2D1;
using Vortice.Direct3D11;

namespace RabotoraX.Interop.Direct3D11.Rendering;

public class D2DTexture : INativeTexture2D
{
	public ID2D1Bitmap Bitmap { get; }
	public int Width => (int)Bitmap.Size.Width;
	public int Height => (int)Bitmap.Size.Height;

	public D2DTexture(ID2D1Bitmap bitmap)
	{
		Bitmap = bitmap;
	}

	public void Dispose()
	{
		Bitmap.Dispose();
		GC.SuppressFinalize(this);
	}
}

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

public class D3D11RenderTexture : INativeRenderTexture
{
	public ID3D11Texture2D Texture { get; }
	public ID3D11RenderTargetView RTV { get; }
	public ID3D11ShaderResourceView SRV { get; }
	public int Width { get; }
	public int Height { get; }
	
	public D3D11RenderTexture(ID3D11Texture2D texture, ID3D11RenderTargetView rtv, ID3D11ShaderResourceView srv, int width, int height)
	{
		Texture = texture;
		RTV = rtv;
		SRV = srv;
		Width = width;
		Height = height;
	}

	public void Dispose()
	{
		RTV.Dispose();
		SRV.Dispose();
		Texture.Dispose();
		GC.SuppressFinalize(this);
	}
}