#pragma warning disable 8618
#define TEST
using Rabotora.Core;
using Rabotora.Launcher.SubForms;
using Rabotora.Runtime;
using Rabotora.Runtime.Api;
using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using D2D = SharpDX.Direct2D1;
using DXGI = SharpDX.DXGI;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Activate2 = SharpDX.MediaFoundation.Activate;
using System.Runtime.InteropServices;

namespace Rabotora.Launcher
{
	/// <summary>
	/// Win_Start.xaml 的交互逻辑
	/// </summary>
	public partial class Win_Start : Window
	{
		/// <summary>
		/// Rabotora Main Game Project.
		/// </summary>
		private readonly RabotoraGameProject _project = Program.MainProject;
		/// <summary>
		/// Entry Game Script.
		/// </summary>
		private readonly RabotoraScript _entryScript = Program.EntryScript;
		/// <summary>
		/// Executed Rabotora Direct2D Commands Linked List.
		/// </summary>
		private LinkedList<RabotoraD2DCmd> ExecutedCommands = new();

		/// <summary>
		/// Global Render Target.
		/// </summary>
		private D2D.WindowRenderTarget RenderTarget;

		private bool IsStarted = false;
		private double PreviousTop = 0;
		private double PreviousLeft = 0;
		private double PreviousWidth = 1280;
		private double PreviousHeight = 720;
		private D2D.DrawingStateBlock drawingStateBlock;
#if TEST
		private List<RabotoraD2DCmd> cmds = new();
		private IStoppableCmd? RunningStoppableCmd = null;
#endif

		public Win_Start()
		{
			InitializeComponent();
			CompositionTarget.Rendering += CompositionTarget_Rendering;
			Loaded += Window_Loaded;
#if DEBUG
			Title = "Project Rabotora Launcher";
#else
			Title = !string.IsNullOrWhiteSpace(_project.Info.GameTitle)
					? _project.Info.GameTitle
					: "Project Rabotora Launcher";
			Task.Run(() =>
			{
				Thread.Sleep(500);
				ExecuteEntryScript();
			});
#endif
		}

		private void ExecuteEntryScript()
		{
			var programClass = (from @class in _entryScript.RawScript.Classes
								where @class.ClassName.Data == "Program"
								select @class).FirstOrDefault();
			if (programClass != null)
			{
				var mainFunc = (from func in programClass.Functions
								where (func.FunctionName.Data == "Main" && func.IsStaticFunction)
								select func).FirstOrDefault();
				if (mainFunc != null)
				{

				}
				else
				{
					throw new RabotoraScriptException("Runtime Error [RSR0002]: Cannot find accessible entry method 'Main'.");
				}
			}
			else
			{
				throw new RabotoraScriptException("Runtime Error [RSR0001]: Cannot find entry class called 'Program'.");
			}
		}

		private void CompositionTarget_Rendering(object? sender, EventArgs e)
		{
			try
			{
				if (RenderTarget == null) return;
				RenderTarget.SaveDrawingState(drawingStateBlock);
				if (!IsStarted)
				{
					IsStarted = true;
					Task.Run(() =>
					{
						Thread.Sleep(2000);
					});
				}
			}
			catch (SharpDXException ex)
			{
				if (!_project.IsDebugMode)
				{
					MessageBox.Show($"A fatal error occurred in current game project ({_project.Info.GameTitle}).\n" +
						$"It is strongly recommended to report current problem to {_project.Info.Company}.\n" +
						$"Game program will shut down.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
					Environment.Exit(0);
				}
				else
				{
					MessageBox.Show($"A fatal error occurred in current game project ({_project.Info.GameName}).\n" +
						$"If you believe that this is caused by a bug, please create issue on GitHub:\n" +
						$"https://github.com/Misaka12456/Rabotora\n" +
						$"Project Rabotora Launcher will shut down.\n" +
						$"-----Technical Information-----\n" +
						$"{ex.GetType()}: {ex.Message}\n" +
						ex.StackTrace, "Fatal Error [Debug Mode]", MessageBoxButton.OK, MessageBoxImage.Error);
					Environment.Exit(0);
				}
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var factory = new D2D.Factory();
			var hwndRenderTargetProperties = new D2D.HwndRenderTargetProperties()
			{
				Hwnd = new WindowInteropHelper(this).Handle,
				PixelSize = new Size2((int)ActualWidth, (int)ActualHeight),
				PresentOptions = D2D.PresentOptions.None
			};
			var renderTargetProperties = new D2D.RenderTargetProperties(D2D.RenderTargetType.Default,
							new D2D.PixelFormat(DXGI.Format.R8G8B8A8_UNorm, D2D.AlphaMode.Premultiplied),
							96, 96, D2D.RenderTargetUsage.None, D2D.FeatureLevel.Level_DEFAULT);
			RenderTarget = new D2D.WindowRenderTarget(factory, renderTargetProperties, hwndRenderTargetProperties);
			PreviousWidth = Width;
			PreviousHeight = Height;
			var device = RenderTarget.QueryInterface<D2D.DeviceContext>().Device;
			drawingStateBlock = new(factory);
#if TEST
			cmds.Add(new BitmapFadeDrawCmd(File.ReadAllBytes(@"C:\Users\Misaka12456\Pictures\Rabotora\Splash-1.png"), new RawRectangleF(0, 0, 1280, 720), FadeType.FadeIn,
				new Size2F((float)ActualWidth, (float)ActualHeight)));
			cmds.Add(new BitmapFadeDrawCmd(File.ReadAllBytes(@"C:\Users\Misaka12456\Pictures\Rabotora\Splash-2.png"), new RawRectangleF(0, 0, 1280, 720), FadeType.FadeIn,
				new Size2F((float)ActualWidth, (float)ActualHeight)));
			cmds.Add(new BitmapFadeDrawCmd(File.ReadAllBytes(@"C:\Users\Misaka12456\Pictures\Rabotora\res_black.png"), new RawRectangleF(0, 0, 1280, 720), FadeType.FadeIn,
				new Size2F((float)ActualWidth, (float)ActualHeight)));
			Task.Run(() =>
			{
				Thread.Sleep(2000);
				foreach (var cmd in cmds)
				{
					if (cmd is IStoppableCmd)
					{
						RunningStoppableCmd = cmd as IStoppableCmd;
					}
					RenderTarget.BeginDraw();
					cmd.DoCommand(RenderTarget);
					for (int i = 0; i < 15; i++)
					{
						if (cmd as IStoppableCmd != null && RunningStoppableCmd == null)
						{
							break;
						}
						Thread.Sleep(100);
					}
					RenderTarget.EndDraw();
				}
			});
#endif
		}

		private void Window_MouseMove(object sender, MouseEventArgs e)
		{
			var position = e.GetPosition(this);
			Title = $"Project Rabotora Launcher (x:{position.X}, y: {position.Y})";
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (RunningStoppableCmd != null)
			{
				if (!RunningStoppableCmd.IsStopDoing)
				{
					RunningStoppableCmd.StopCommand();
				}
				else
				{
					RunningStoppableCmd = null;
				}
			}
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (Width / 16 * 9 != Height)
			{
				Height = Width / 16 * 9;
			}
			return;
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter) && Keyboard.Modifiers == ModifierKeys.Alt)
			{
				if (WindowStyle == WindowStyle.SingleBorderWindow)
				{
					PreviousTop = Top;
					PreviousLeft = Left;
					PreviousWidth = ActualWidth;
					PreviousHeight = ActualHeight;
					WindowStyle = WindowStyle.None;
					WindowState = WindowState.Normal;
					ResizeMode = ResizeMode.NoResize;
					Left = 0;
					Top = 0;
					Width = SystemParameters.PrimaryScreenWidth;
					Height = SystemParameters.PrimaryScreenHeight;
					CompositionTarget_Rendering(null, new());
				}
				else
				{
					WindowState = WindowState.Normal;
					WindowStyle = WindowStyle.SingleBorderWindow;
					ResizeMode = ResizeMode.CanMinimize;
					Top = PreviousTop;
					Left = PreviousLeft;
					Width = PreviousWidth;
					Height = PreviousHeight;
					CompositionTarget_Rendering(null, new());
				}
			}
			else if (e.KeyStates == Keyboard.GetKeyStates(Key.F12) && Keyboard.Modifiers == ModifierKeys.Control)
			{
				if (!Frm_About.IsShown)
				{
					var about = new Frm_About();
					about.ShowDialog();
				}
			}
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			if (RenderTarget == null) return;
			IsStarted = false;
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
			if (MessageBox.Show("Sure to quit the game?", "Please Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				Environment.Exit(0);
			}
		}

		/* [Obsolete("This function is deprecated due to the lag when Device Lost(we needs to restore render target). We are trying to find another way to solve the problem.")]
		private void RestoreRenderTarget()
		{
			factory.ReloadSystemMetrics();
			RenderTarget = new D2D.WindowRenderTarget(factory, renderTargetProperties, hwndRenderTargetProperties);
			device = RenderTarget.QueryInterface<D2D.DeviceContext>().Device;
			CompositionTarget.Rendering += CompositionTarget_Rendering;
			RenderTarget.RestoreDrawingState(drawingStateBlock);
			isRestoring = false;
			Debug.Print("Good job! We successfully restored render target. Enjoy!");
		}
		*/
	}
}
