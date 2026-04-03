using System.Runtime.InteropServices;
using RabotoraX.Core.Videos;
using StbImageSharp;

namespace RabotoraX.Core.Graphics;

public class Texture2D : Object
{
	private INativeTexture2D? _nativeTexture;
	
	public int Width => _nativeTexture?.Width ?? 0;
	public int Height => _nativeTexture?.Height ?? 0;
	public bool IsLoaded => _nativeTexture != null;
	
	public INativeTexture2D NativeTexture => _nativeTexture ?? throw new InvalidOperationException("Texture not loaded yet.");

	public void WrapNative(INativeTexture2D native)
	{
		DisposeNativeTexture();
		_nativeTexture = native;
	}
	
	public void CreateEmpty(int width, int height, byte r = 255, byte g = 255, byte b = 255, byte a = 255)
	{
		DisposeNativeTexture();
        
		byte[] data = new byte[width * height * 4];
		for (int i = 0; i < data.Length; i += 4)
		{
			data[i] = r;
			data[i + 1] = g;
			data[i + 2] = b;
			data[i + 3] = a;
		}

		_nativeTexture = GraphicsService.API.CreateTexture2D(width, height, data);
	}
	
	public void CreateEmpty2D(int width, int height, byte r = 255, byte g = 255, byte b = 255, byte a = 255)
	{
		DisposeNativeTexture();
        
		var context2D = GraphicsService.API.Get2DContext() ?? throw new InvalidOperationException("No 2D context available.");

		if (!GraphicsService.API.ApiName.StartsWith("Direct"))
		{
			CreateEmpty(width, height, r, g, b, a);
			return;
		}

		byte[] data = new byte[width * height * 4];
		for (int i = 0; i < data.Length; i += 4)
		{
			data[i] = b;
			data[i + 1] = g;
			data[i + 2] = r;
			data[i + 3] = a;
		}
		
		_nativeTexture = context2D.CreateTexture(width, height, data);
	}
	
	public void CreateEmpty2DForVideo(int width, int height, byte r = 255, byte g = 255, byte b = 255, byte a = 255)
	{
		DisposeNativeTexture();
        
		var context2D = GraphicsService.API.Get2DContext() ?? throw new InvalidOperationException("No 2D context available.");

		if (!GraphicsService.API.ApiName.StartsWith("Direct"))
		{
			CreateEmpty(width, height, r, g, b, a);
			return;
		}

		byte[] data = new byte[width * height * 4];
		for (int i = 0; i < data.Length; i += 4)
		{
			data[i] = b;
			data[i + 1] = g;
			data[i + 2] = r;
			data[i + 3] = a;
		}
		
		_nativeTexture = context2D.CreateVideoTexture(width, height, data);
	}
	
	public void CreateEmpty2DForVideo(int width, int height, VideoPixelFormat format, byte r = 255, byte g = 255, byte b = 255, byte a = 255)
	{
		DisposeNativeTexture();
		
		var context2D = GraphicsService.API.Get2DContext() ?? throw new InvalidOperationException("No 2D context available.");

		if (!GraphicsService.API.ApiName.StartsWith("Direct"))
		{
			CreateEmpty(width, height, r, g, b, a);
			return;
		}

		byte[] data = new byte[width * height * 4];
		for (int i = 0; i < data.Length; i += 4)
		{
			data[i] = b;
			data[i + 1] = g;
			data[i + 2] = r;
			data[i + 3] = a;
		}
		
		_nativeTexture = context2D.CreateVideoTexture(width, height, format, data);
	}
	
	public void LoadImage(byte[] data)
	{
		LoadImage(new MemoryStream(data));
	}

	public void LoadImage(ReadOnlyMemory<byte> data)
	{
		if (MemoryMarshal.TryGetArray(data, out var segment) && segment.Array != null)
		{
			LoadImage(new MemoryStream(segment.Array, segment.Offset, segment.Count));
		}
		else
		{
			LoadImage(new MemoryStream(data.ToArray()));
		}
	}

	public void LoadImage(Stream stream)
	{
		DisposeNativeTexture();
		
		var imageResult = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha); // RGBA Format
		
		_nativeTexture = GraphicsService.API.CreateTexture2D(imageResult.Width, imageResult.Height, imageResult.Data);
	}
	
	public void Load2DImage(byte[] data)
	{
		Load2DImage(new MemoryStream(data));
	}
	
	public void Load2DImage(ReadOnlyMemory<byte> data)
	{
		if (MemoryMarshal.TryGetArray(data, out var segment) && segment.Array != null)
		{
			Load2DImage(new MemoryStream(segment.Array, segment.Offset, segment.Count));
		}
		else
		{
			Load2DImage(new MemoryStream(data.ToArray()));
		}
	}

	public void Load2DImage(Stream stream)
	{
		DisposeNativeTexture();
		var context2D = GraphicsService.API.Get2DContext() ?? throw new InvalidOperationException("No 2D context available. Is current stage a Render2D or Render3DHybrid stage?");
		if (!GraphicsService.API.ApiName.StartsWith("Direct"))
		{
			LoadImage(stream);
		}
		_nativeTexture = context2D.CreateTexture(stream); // On Windows, Direct2D (WIC) will process this; on other platforms, it will fall back to the same implementation as LoadImage.
	}

	private void DisposeNativeTexture()
	{
		_nativeTexture?.Dispose();
		_nativeTexture = null;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			DisposeNativeTexture();
		}
		base.Dispose(disposing);
	}
}