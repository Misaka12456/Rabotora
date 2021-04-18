#nullable enable
using static Rabotora.Launcher.Globals;
using System;
using System.Threading;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.WIC;
using SharpDX.IO;
using SharpDX.Mathematics.Interop;
using Factory = SharpDX.Direct2D1.Factory;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using PixelFormat2 = SharpDX.WIC.PixelFormat;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using BitmapInterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode;
using System.Drawing;
using Microsoft.VisualBasic;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using Rabotora.Core.ScriptAnalyzer;

namespace Rabotora.Launcher
{
	public partial class FormBase : Form
	{
		protected Script EntryScript { get; }

		#region Variables
		#region DirectX2D Variables
		protected Factory? Factory;
		protected ImagingFactory? IFactory;
		protected WindowRenderTarget? RenderTarget;
		protected string? splashPath;
		protected Bitmap? b;
		#endregion
		#region DirectX2D(XAudio2) Variables
		protected XAudio2? xaudio;
		protected MasteringVoice? MVoice;
		Thread? t;
		#endregion
		#region Custom Variables
		protected bool isMinimizedBefore = false;
		protected bool isStarted = false;
		int normalHeight;
		int normalWidth;
		int startHeight;
		int startWidth;
		bool isAutoResizing = false;
		#endregion
		#endregion

		/// <summary>
		/// 使用适用于主窗口的入口脚本(Splash <see cref="Script"/>)对象初始化 <see cref="FormBase"/> 类的新实例。
		/// </summary>
		/// <param name="script">适用于当前 <see cref="FormBase"/> 类新实例的入口脚本(Splash <see cref="Script"/>)对象。</param>
		public FormBase(Script script)
		{
			InitializeComponent();
			SetStyle(ControlStyles.UserPaint, false);
			startHeight = Height;
			startWidth = Width;
			normalHeight = Height;
			normalWidth = Width;
			CreateDeviceResource(this);
			CreateXAudio2Resource();
			EntryScript = script;
		}

		protected void StartMain()
		{
			var g = CreateGraphics();
			g.Clear(Color.Black);
			Thread.Sleep(1000);
			if (Interaction.Command().Trim().ToLower() == "-fs")
			{
				isAutoResizing = true;
				WindowState = FormWindowState.Maximized;
				FormBorderStyle = FormBorderStyle.None;
				SetWindowPos(Handle, new IntPtr(-1), 0, 0, Screen.PrimaryScreen.Bounds.Size.Width, Screen.PrimaryScreen.Bounds.Size.Height, 0);
				isAutoResizing = false;
				CreateDeviceResource(this);
				RenderTarget.BeginDraw();
				RenderTarget.Clear(new RawColor4(0f, 0f, 0f, 0f));
				RenderTarget.EndDraw();
				Thread.Sleep(1500);
			}
		}

		protected void CreateDeviceResource(Control target)
		{
			Factory = new Factory();
			IFactory = new ImagingFactory();
			var p = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore);
			var h = new HwndRenderTargetProperties()
			{
				Hwnd = target.Handle,
				PixelSize = new Size2(target.Width, target.Height),
				PresentOptions = PresentOptions.None
			};
			var r = new RenderTargetProperties(RenderTargetType.Hardware, p, 0f, 0f, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);
			RenderTarget = new WindowRenderTarget(Factory, r, h);
		}

		protected void CreateXAudio2Resource()
		{
			xaudio = new XAudio2();
			xaudio.StartEngine();
			MVoice = new MasteringVoice(xaudio);
		}

		protected void StartSplash(string splashPicPath)
		{
			if (RenderTarget != null)
			{
				splashPath = splashPicPath;
				RenderTarget.BeginDraw();
				RenderTarget.Clear(new RawColor4(0f, 0f, 0f, 255f));
				b = LoadBitmap(RenderTarget, splashPicPath, 0);
				RenderTarget.DrawBitmap(b, new RawRectangleF(0f, 0f, Width, Height), 0f, BitmapInterpolationMode.Linear);
				RenderTarget.EndDraw();
				for (float i = 0f; i < 1.0f; i = i + 0.01f)
				{
					RenderTarget.BeginDraw();
					RenderTarget.DrawBitmap(b, new RawRectangleF(0f, 0f, Width, Height), i, BitmapInterpolationMode.Linear);
					RenderTarget.EndDraw();
					Thread.Sleep(30);
				}
			}
		}

		public void ChangePicture(string newPicPath,Action? funcWhenStartShowingNewPic = null, Action? funcWhenShowedNewPic = null)
		{
			lock (RenderTarget)
			{
				var res_black = LoadBitmap(RenderTarget, @"C:\Users\Misaka-12456\Pictures\Rabotora\res_black.png", 0);
				for (float i = 0f; i < 1.0f; i = i + 0.01f)
				{
					RenderTarget.BeginDraw();
					RenderTarget.DrawBitmap(res_black, new RawRectangleF(0f, 0f, Width, Height), i, BitmapInterpolationMode.Linear);
					RenderTarget.EndDraw();
					Thread.Sleep(20);
				}
				b = LoadBitmap(RenderTarget, newPicPath, 0);
				RenderTarget.BeginDraw();
				RenderTarget.DrawBitmap(b, new RawRectangleF(0f, 0f, Width, Height), 0f, BitmapInterpolationMode.Linear);
				RenderTarget.EndDraw();
				if (funcWhenStartShowingNewPic != null)
				{
					funcWhenStartShowingNewPic.BeginInvoke(new AsyncCallback((IAsyncResult r) => { }),null);
				}
				for (float j = 0f; j < 1.0f; j = j + 0.03f)
				{
					RenderTarget.BeginDraw();
					RenderTarget.DrawBitmap(b, new RawRectangleF(0f, 0f, Width, Height), j, BitmapInterpolationMode.Linear);
					RenderTarget.EndDraw();
					Thread.Sleep(30);
				}
				if (funcWhenShowedNewPic != null)
				{
					funcWhenShowedNewPic.BeginInvoke(new AsyncCallback((IAsyncResult r) => { }), null);
				}
			}
		}

		public void PlaySound(string soundPath)
		{
			var stream = new NativeFileStream(soundPath, NativeFileMode.Open,
				NativeFileAccess.Read);
			var soundStream = new SoundStream(stream);
			var buffer = new AudioBuffer()
			{
				Stream = soundStream,
				AudioBytes = (int)soundStream.Length,
				Flags = BufferFlags.EndOfStream
			};
			var sourceVoice = new SourceVoice(xaudio, soundStream.Format);
			sourceVoice.SubmitSourceBuffer(buffer, soundStream.DecodedPacketsInfo);
			sourceVoice.Start();
		}

		protected void ReDrawSplash(bool isStartAgain = false, int width = -1, int height = -1)
		{
			try
			{
				if (isStartAgain)
				{
					CreateDeviceResource(this);
				}
				if (RenderTarget != null)
				{
					RenderTarget.BeginDraw();
					if (width == -1) width = Width;
					if (height == -1) height = Height;
					if (isStartAgain)
					{
						b = LoadBitmap(RenderTarget, splashPath!, 0);
						RenderTarget.DrawBitmap(b, new RawRectangleF(0f, 0f, width, height), 1f, BitmapInterpolationMode.Linear, new RawRectangleF(0,0, width, height));
					}
					else
					{
						RenderTarget.DrawBitmap(b, new RawRectangleF(0f, 0f, width, height), 1f, BitmapInterpolationMode.Linear, new RawRectangleF(0, 0, width, height));
					}
					RenderTarget.EndDraw();
				}
			}
			catch (SharpDXException ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		protected Bitmap LoadBitmap(RenderTarget render, string file,int frameIndex)
		{
			var decoder = new BitmapDecoder(IFactory, file, NativeFileAccess.Read, DecodeOptions.CacheOnLoad);
			if (frameIndex > decoder.FrameCount - 1 || frameIndex < 0)
			{
				frameIndex = 0;
			}
			var source = decoder.GetFrame(frameIndex);
			var converter = new FormatConverter(IFactory);
			converter.Initialize(source, PixelFormat2.Format32bppPBGRA);
			return Bitmap.FromWicBitmap(render, converter);
		}

		private void FormBase_Shown(object sender, EventArgs e)
		{
			isStarted = true;
			t = new Thread(StartMain)
			{
				IsBackground = true
			};
			t.Start();
		}

		private void FormBase_Resize(object sender, EventArgs e)
		{
			if (isMinimizedBefore && WindowState != FormWindowState.Minimized)
			{
				isMinimizedBefore = false;
				if (!isAutoResizing)
				{
					ReDrawSplash(true);
					normalHeight = Height;
					normalWidth = Width;
				}
			}
			else if (WindowState == FormWindowState.Maximized && isStarted && !isAutoResizing)
			{
				isAutoResizing = true;
				FormBorderStyle = FormBorderStyle.None;
				SetWindowPos(Handle, new IntPtr(0), 0, 0, Screen.PrimaryScreen.Bounds.Size.Width, Screen.PrimaryScreen.Bounds.Size.Height, 0);
				ReDrawSplash(true);
				isAutoResizing = false;
			}
			else
			{
				if (Width != startWidth)
				{
					float proportion = (float)startWidth / Width;
					int newHeight = (int)(startHeight / proportion);
					Height = newHeight;
				}
				else if (Height != startHeight)
				{
					float proportion = (float)startHeight / Height;
					int newWidth = (int)(startWidth / proportion);
					Width = newWidth;
				}
				if (!isAutoResizing)
				{
					normalHeight = Height;
					normalWidth = Width;
					ReDrawSplash(width: normalWidth, height: normalHeight);
				}
			}
		}

		private void FormBase_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Alt && e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				if (WindowState == FormWindowState.Maximized && FormBorderStyle == FormBorderStyle.None)
				{
					isAutoResizing = true;
					WindowState = FormWindowState.Normal;
					FormBorderStyle = FormBorderStyle.Sizable;
					SetWindowPos(Handle, new IntPtr(-2), Left, Top, normalWidth, normalHeight, 0);
					ControlBox = true;
					ReDrawSplash(true, normalWidth, normalHeight);
					isAutoResizing = false;
				}
				else if (WindowState == FormWindowState.Normal)
				{
					isAutoResizing = true;
					FormBorderStyle = FormBorderStyle.None;
					SetWindowPos(Handle, new IntPtr(0), 0, 0, Screen.PrimaryScreen.Bounds.Size.Width, Screen.PrimaryScreen.Bounds.Size.Height, 0);
					ReDrawSplash(true);
					isAutoResizing = false;
				}
			}
		}
	}
}