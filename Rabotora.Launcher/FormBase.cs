#nullable enable
using Rabotora.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rabotora.Launcher
{
	public partial class FormBase : Form
	{
		public FormBase()
		{
			InitializeComponent();
		}

		public void StartSplash(Image splash)
		{
			var g = CreateGraphics();
			g.Clear(Color.Black);
			int width = splash.Width;
			int height = splash.Height;
			var attributes = new ImageAttributes();
			var matrix = new ColorMatrix();
			matrix.Matrix00 = 0.0F;
			matrix.Matrix01 = 0.0F;
			matrix.Matrix02 = 0.0F;
			matrix.Matrix03 = 0.0F;
			matrix.Matrix04 = 0.0F;
			matrix.Matrix10 = 0.0F;
			matrix.Matrix11 = 0.0F;
			matrix.Matrix12 = 0.0F;
			matrix.Matrix13 = 0.0F;
			matrix.Matrix14 = 0.0F;
			matrix.Matrix20 = 0.0F;
			matrix.Matrix21 = 0.0F;
			matrix.Matrix22 = 0.0F;
			matrix.Matrix23 = 0.0F;
			matrix.Matrix24 = 0.0F;
			matrix.Matrix30 = 0.0F;
			matrix.Matrix31 = 0.0F;
			matrix.Matrix32 = 0.0F;
			matrix.Matrix33 = 0.0F;
			matrix.Matrix34 = 0.0F;
			matrix.Matrix40 = 0.0F;
			matrix.Matrix41 = 0.0F;
			matrix.Matrix42 = 0.0F;
			matrix.Matrix43 = 0.0F;
			matrix.Matrix44 = 0.0F;
			matrix.Matrix33 = 1.0F;
			matrix.Matrix44 = 1.0F;
			float count = 0.0F;
			while (count < 1.0F)
			{
				matrix.Matrix00 = count;
				matrix.Matrix11 = count;
				matrix.Matrix22 = count;
				matrix.Matrix33 = count;
				attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
				g.DrawImage(splash, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel, attributes);
				Thread.Sleep(200);
				count += 0.02F;
			}
		}
	}
}
