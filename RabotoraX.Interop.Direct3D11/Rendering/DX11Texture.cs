using RabotoraX.Core.Graphics;
using Vortice.Direct2D1;
using Vortice.Direct3D11;

namespace RabotoraX.Interop.Direct3D11.Rendering;

public class D2DTexture : ITexture2D
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