using System;
using RabotoraX.Core.Graphics;
using Vortice.Direct2D1;
using Vortice.Direct3D11;

namespace RabotoraX.Interop.Direct3D11.Rendering;

public sealed class DX11NV12VideoTexture : INativeTexture2D
{
	public ID3D11Texture2D InputY { get; }
	public ID3D11Texture2D InputUV { get; }
	public ID3D11ShaderResourceView InputYSrv { get; }
	public ID3D11ShaderResourceView InputUVSrv { get; }
	
	public ID3D11Texture2D OutputRgba { get; }
	public ID3D11RenderTargetView OutputRgbaRtv { get; }
	
	public ID2D1Bitmap1? D2DBitmap { get; set; }
	
	public int Width { get; }
	public int Height { get; }
	
	public DX11NV12VideoTexture(ID3D11Texture2D yTex, ID3D11ShaderResourceView ySrv, ID3D11Texture2D uvTex, ID3D11ShaderResourceView uvSrv,
		ID3D11Texture2D rgbaTex, ID3D11RenderTargetView rgbaRtv,
		int width, int height)
	{
		InputY = yTex;
		InputYSrv = ySrv;
		InputUV = uvTex;
		InputUVSrv = uvSrv;
		OutputRgba = rgbaTex;
		OutputRgbaRtv = rgbaRtv;
		Width = width;
		Height = height;
	}
	
	public void Dispose()
	{
		D2DBitmap?.Dispose();
		InputYSrv.Dispose();
		InputY.Dispose();
		InputUVSrv.Dispose();
		InputUV.Dispose();
		OutputRgbaRtv.Dispose();
		OutputRgba.Dispose();
	}
}