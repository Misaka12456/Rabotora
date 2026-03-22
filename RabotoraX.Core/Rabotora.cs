using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using RabotoraX.Core.Audios;
using RabotoraX.Core.Cinematics;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Infrastructure;
using RabotoraX.Core.Inputs;
using RabotoraX.Core.Mathematics;
using RabotoraX.Core.Threading;

namespace RabotoraX.Core;

/// <summary>
/// Represents the main application class for Rabotora, responsible for managing the window, graphics context, and the main loop.<br />
/// This implementation is not platform-specific; it relies on <see cref="INativeWindow"/> and <see cref="INativeGraphicsAPI"/> which both have platform-specific implementations.<br />
/// To extend functionality, inherit from this class and override the <see cref="OnUpdate"/> and <see cref="OnRender"/> methods for custom update and render logic.
/// The main loop will handle timing and stage management automatically.<br />
/// <br />
/// See https://docs.misakacastle.moe/rabotora for more details and examples on how to use and extend this class.
/// </summary>
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class Rabotora : IDisposable
{
	private const int MaxErrorFrameThreshold = 5; // If more than this number of consecutive frames fail to render, we will assume the application is in a bad state and exit to prevent hanging indefinitely.
	
	/// <summary>
	/// The native window instance used for rendering and event handling.
	/// </summary>
	public INativeWindow Window { get; }
	
	/// <summary>
	/// The native graphics API instance used for rendering.
	/// </summary>
	public INativeGraphicsAPI Graphics { get; }
	
	/// <summary>
	/// The fixed aspect ratio for the window, if specified. 
	/// If this is set, the window will maintain the specified aspect ratio when resized, adding letterboxing as necessary.
	/// If null, the window can be resized freely without maintaining a specific aspect ratio.
	/// </summary>
	public Fractional? FixedAspectRatio { get; }
	
	/// <summary>
	/// The currently active stage being performed by the cinema. This will be null if no stage is currently active.<br />
	/// You can set the active stage by calling <see cref="Cinema.Ready"/> with a new stage instance.
	/// </summary>
	public RStage? ActiveStage => Cinema.PerformingStage;

	private readonly INativeSystemHighPrecisionProvider _osHPProvider;
	private readonly Stopwatch _clock = new();
	private WindowStateSnapshot _currentFrameWindowState;
	private float _lastTime;
	private bool _isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="Rabotora"/> class with the specified window title and dimensions.
	/// </summary>
	/// <param name="title">The title of the application window.</param>
	/// <param name="width">The initial width of the application window in pixels. Default is 1280.</param>
	/// <param name="height">The initial height of the application window in pixels. Default is 720.</param>
	public Rabotora(string title, int width = 1280, int height = 720, Fractional? fixedAspectRatio = null)
	{
		FixedAspectRatio = fixedAspectRatio;
		AppDomain.CurrentDomain.UnhandledException += (s, e) => UnhandledException(s, e.ExceptionObject as Exception);
		SynchronizationContext.SetSynchronizationContext(new RabotoraSynchronizationContext(UnhandledException));
		_osHPProvider = INativeSystemHighPrecisionProvider.PlatformCreate();
		_osHPProvider.TryEnableHighPrecision();
		
		Window = INativeWindow.PlatformCreate();
		Window.Create(width, height, title, fixedAspectRatio: fixedAspectRatio);

		Graphics = INativeGraphicsAPI.PlatformDefaultCreate();
		Graphics.Initialize(Window);
		
		GraphicsService.Initialize(Graphics);
		Input.Initialize(Window.Input);
		AudioService.Initialize();
		
		Window.Resized += (_, size) => Graphics.Resize(size.Item1, size.Item2);
		Window.SwitchingFullScreen += (_, _) => Graphics.IgnoreAllPresents = true;
		Window.SwitchedFullScreen += (_, _) => Graphics.IgnoreAllPresents = false;
	}

	/// <summary>
	/// Run the main loop of the application, starting with the provided initial stage. This method will block until the window is closed.
	/// </summary>
	/// <param name="initialStage">The initial stage to set up before entering the main loop.</param>
	/// <returns>Returns 0 on normal exit, or -1 if an exception occurred during execution.</returns>
	public int Run(RStage initialStage)
	{
		try
		{
			Thread.CurrentThread.Name = "Rabotora Main (Window) Thread";
			Cinema.Ready(initialStage);
			
			lock (Graphics.RenderLock)
			{
				_clock.Start();
				RenderTickFrame(); // render the very first frame before showing the window to avoid unexpected white flashes.
			}
			
			Window.Show();
			// Multi-Thread Logic was introduced in RabotoraX v0.2.2 to avoid "render pause" when player dragging the window or when the window is not focused.
			// The main thread will be responsible for handling window events and updating input state,
			// while the render thread will be responsible for rendering frames continuously as long as the window is not closing.
			var renderThread = new Thread(RenderThreadLoop) { Name = "Rabotora Render Thread" }; // We shouldn't set IsBackground, because if error occurs we want it crash-fast (fail-fast) instead of silently ignore
			MultiThreadService.Initialize(renderThread);
			renderThread.Start();
			var audioThread = new Thread(AudioThreadLoop) { Name = "Rabotora Audio Thread", IsBackground = true };
			audioThread.Start();

			while (!Window.IsClosing)
			{
				Window.DoEvents();
			}
			
			renderThread.Join(); // wait for the render thread to finish before exiting
			audioThread.Join(); // wait for the audio thread to finish before exiting
			Graphics.WaitIdle(); // wait for GPU to finish all tasks before exiting
		}
		catch
		{
			return -1;
		}
		return 0;
	}

	private void RenderThreadLoop()
	{
		int errorCount = 0;
		while (!Window.IsClosing)
		{
			Graphics.WaitNextFrameReady();
			MultiThreadService.ExecutePendingTasks();
			_currentFrameWindowState = Window.GetStateSnapshot();
			GraphicsService.Update(_currentFrameWindowState);
			Input.Update(); // let RaboInput responsible for updating input state to reduce coupling
			if (_currentFrameWindowState.IsVisible) // use the snapshot
			{
				lock (Graphics.RenderLock)
				{
					// ReSharper disable once MergeIntoPattern // or the following result.err will be considered "possibly unassigned" even though it's actually assigned in the RenderTickFrame method
					if (RenderTickFrame() is var result && result.success && errorCount > 0)
					{
						errorCount = 0;
					}
					else
					{
						errorCount++;
						if (errorCount >= MaxErrorFrameThreshold)
						{
							throw new RabotoraException("Too many consecutive frame render errors and the application will be terminated.", result.err ?? new Exception("Unknown error during frame rendering."));
							// Because this is a front-end thread, this will cause the application to crash by the UnhandledException handler.
						}
					}
				}
			}
			else
			{
				Thread.Sleep(10); // Sleep briefly to avoid busy-waiting when the window is not visible
			}

			
		}
	}
	
	private void AudioThreadLoop()
	{
		while (!Window.IsClosing)
		{
			AudioService.Update();
			Thread.Sleep(5); // Sleep briefly to reduce CPU usage, adjust as necessary based on audio processing needs
		}
	}

	private (bool success, Exception? err) RenderTickFrame()
	{
		try
		{
			float currentTime = (float) _clock.Elapsed.TotalSeconds;
			float deltaTime = currentTime - _lastTime;
			_lastTime = currentTime;

			OnUpdate(deltaTime);
			Cinema.Update(deltaTime);
			// AudioService.Update();

			Graphics.BeginFrame();
			// Graphics.Clear(0, 0, 0, 1); // Clear to black by default, can be changed by user code in OnUpdate or stage updates
			// Clear logic was moved to Audience.ClearConfig (3D or 3DHybrid stages) or RStage.ClearColor (2D stages) since RabotoraX v0.2.1

			OnRender();
			Cinema.Render();

			Graphics.EndFrame();
			Graphics.Present(vsync: true);
			return (true, null);
		}
		catch (Exception ex)
		{
#if DEBUG
			Console.WriteLine($"[RabotoraX] Exception during RenderTickFrame: {ex}");
#endif
			return (false, ex);
		}
	}
	
	/// <summary>
	/// Called every frame before the stage updates.<br />
	/// Override this method to implement custom update logic that should run before the stage's update logic.
	/// </summary>
	/// <param name="deltaTime"></param>
	protected virtual void OnUpdate(float deltaTime) { }
	
	/// <summary>
	/// Called every frame after the stage updates and before rendering.<br />
	/// Override this method to implement custom rendering logic that should run after the stage's render logic but before the frame is presented.
	/// </summary>
	protected virtual void OnRender() { }
	
	protected virtual void UnhandledException(object? sender, Exception? ex)
	{
#if DEBUG
		Console.WriteLine($"[RabotoraX] Unhandled exception: {ex}");
#endif
		Dispose();
	}

	/// <summary>
	/// When overridden in a derived class, releases the unmanaged resources used by the <see cref="Rabotora"/> and optionally releases the managed resources.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	
	protected virtual void Dispose(bool disposing)
	{
		if (_isDisposed) return;
		if (disposing)
		{
			Cinema.PerformingStage?.Dispose();
			Graphics.Dispose();
			AudioService.Dispose();
			Window.Dispose();
			_osHPProvider.Dispose();
		}
		_isDisposed = true;
	}
	
	~Rabotora()
	{
		Dispose(false);
	}
}