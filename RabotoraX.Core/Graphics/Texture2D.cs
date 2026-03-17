using System.Runtime.InteropServices;
using StbImageSharp;

namespace RabotoraX.Core.Graphics;

public class Texture2D : Object
{
	private INativeTexture2D? _nativeTexture;
	
	public int Width => _nativeTexture?.Width ?? 0;
	public int Height => _nativeTexture?.Height ?? 0;
	public bool IsLoaded => _nativeTexture != null;
	
	public INativeTexture2D NativeTexture => _nativeTexture ?? throw new InvalidOperationException("Texture not loaded yet.");

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