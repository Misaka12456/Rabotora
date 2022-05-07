using SharpDX;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Bitmap = System.Drawing.Bitmap;
using D2D = SharpDX.Direct2D1;
using DXGI = SharpDX.DXGI;
using Drawing2 = System.Drawing;
using System.Runtime.Versioning;

namespace System.Enhance.SharpDX.Direct2D
{
	[SupportedOSPlatform("windows")]
	public static class BitmapHelper
	{
        public static D2D.Bitmap LoadD2DBitmapFromFile(this D2D.RenderTarget renderTarget, string file, float scaleX = 1, float scaleY = 1)
        {
			try
			{
				using (var bitmap = (Bitmap)Image.FromFile(file))
				{
					var g = Graphics.FromImage(bitmap);
					g.ScaleTransform(scaleX, scaleY);
					g.Flush();
					g.Save();
					g.Dispose();
					var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
					var bitmapProperties = new D2D.BitmapProperties(new D2D.PixelFormat(DXGI.Format.R8G8B8A8_UNorm, D2D.AlphaMode.Premultiplied));
					var size = new Size2(bitmap.Width, bitmap.Height);
					// Transform pixels from BGRA to RGBA
					int stride = bitmap.Width * sizeof(int);
					using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
					{
						// Lock System.Drawing.Bitmap
						var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, Drawing2.Imaging.PixelFormat.Format32bppPArgb);
						// Convert all pixels 
						for (int y = 0; y < bitmap.Height; y++)
						{
							int offset = bitmapData.Stride * y;
							for (int x = 0; x < bitmap.Width; x++)
							{
								// Not optimized 
								byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
								byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
								byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
								byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
								int rgba = R | (G << 8) | (B << 16) | (A << 24);
								tempStream.Write(rgba);
							}
						}
						bitmap.UnlockBits(bitmapData);
						tempStream.Position = 0;
						return new D2D.Bitmap(renderTarget, size, tempStream, stride, bitmapProperties);
					}
				}
			}
			catch (SharpDXException)
			{
				throw;
			}
		}

		public static D2D.Bitmap LoadD2DBitmapFromBytes(this D2D.RenderTarget renderTarget, byte[] data)
		{
			using var stream = new MemoryStream(data);
			stream.Seek(0, SeekOrigin.Begin);
			// Loads from file using System.Drawing.Image
			using var bitmap = (Drawing2.Bitmap)Image.FromStream(stream);
			var sourceArea = new Drawing2.Rectangle(0, 0, bitmap.Width, bitmap.Height);
			var bitmapProperties = new D2D.BitmapProperties(new D2D.PixelFormat(DXGI.Format.R8G8B8A8_Typeless, D2D.AlphaMode.Premultiplied));
			var size = new Drawing2.Size(bitmap.Width, bitmap.Height);
			// Transform pixels from BGRA to RGBA
			int stride = bitmap.Width * sizeof(int);
			using var tempStream = new DataStream(bitmap.Height * stride, true, true);
			// Lock System.Drawing.Bitmap
			var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, Drawing2.Imaging.PixelFormat.Format32bppPArgb);
			// Convert all pixels 
			for (int y = 0; y < bitmap.Height; y++)
			{
				int offset = bitmapData.Stride * y;
				for (int x = 0; x < bitmap.Width; x++)
				{
					// Not optimized 
					byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
					byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
					byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
					byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
					int rgba = R | (G << 8) | (B << 16) | (A << 24);
					tempStream.Write(rgba);
				}
			}
			bitmap.UnlockBits(bitmapData);
			tempStream.Position = 0;
			return new D2D.Bitmap(renderTarget, new Size2(size.Width, size.Height), tempStream, stride, bitmapProperties);
		}

		public static D2D.Bitmap LoadD2DBitmapFromBitmap(this D2D.RenderTarget renderTarget, Bitmap bitmap)
		{
			try
			{
				var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
				var bitmapProperties = new D2D.BitmapProperties(new D2D.PixelFormat(DXGI.Format.R8G8B8A8_UNorm, D2D.AlphaMode.Premultiplied));
				var size = new Size2(bitmap.Width, bitmap.Height);
				// Transform pixels from BGRA to RGBA
				int stride = bitmap.Width * sizeof(int);
				using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
				{
					// Lock System.Drawing.Bitmap
					var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, Drawing2.Imaging.PixelFormat.Format32bppPArgb);

					// Convert all pixels 
					for (int y = 0; y < bitmap.Height; y++)
					{
						int offset = bitmapData.Stride * y;
						for (int x = 0; x < bitmap.Width; x++)
						{
							// Not optimized 
							byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
							byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
							byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
							byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
							int rgba = R | (G << 8) | (B << 16) | (A << 24);
							tempStream.Write(rgba);
						}
					}
					bitmap.UnlockBits(bitmapData);
					tempStream.Position = 0;
					return new D2D.Bitmap(renderTarget, size, tempStream, stride, bitmapProperties);
				}
			}
			finally
			{
				bitmap.Dispose();
			}
		}
	}
}
