using System;
using RabotoraX.Core.Graphics;
using Vortice.DCommon;
using Vortice.Direct2D1;
using Vortice.Direct3D11;
using Vortice.DXGI;
using AlphaMode = Vortice.DCommon.AlphaMode;

namespace RabotoraX.Interop.Direct3D11.Rendering;

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

	public ID2D1Bitmap1 GetD2DBitmap(ID2D1DeviceContext d2dContext)
	{
		using var surface = Texture.QueryInterface<IDXGISurface>();
		
		return d2dContext.CreateBitmapFromDxgiSurface(surface, new BitmapProperties1(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied),
			96, 96, BitmapOptions.Target | BitmapOptions.CannotDraw));
	}

	public void Dispose()
	{
		RTV.Dispose();
		SRV.Dispose();
		Texture.Dispose();
		GC.SuppressFinalize(this);
	}
}