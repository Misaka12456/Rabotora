using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Versioning;

namespace System.Enhance.Drawing
{
    [SupportedOSPlatform("windows")]
	public static class BitmapHelper
	{
        public static Bitmap ScaleBitmap(this Image bitmap, double multiply)
        {
            Bitmap destBitmap = new Bitmap(Convert.ToInt32(bitmap.Width * multiply), Convert.ToInt32(bitmap.Height * multiply));
            Graphics g = Graphics.FromImage(destBitmap);
            g.Clear(Color.Transparent);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(bitmap, new Rectangle(0, 0, destBitmap.Width, destBitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel);
            g.Dispose();
            return destBitmap;
        }
    }
}