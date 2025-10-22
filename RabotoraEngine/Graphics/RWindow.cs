using RabotoraEngine.Graphics.Internal;
using RabotoraEngine.Graphics.SceneManagement;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace RabotoraEngine.Graphics;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global // for future RWindow extensibility
// ReSharper disable InconsistentlySynchronizedField
/// <summary>
/// Represents the Rabotora VN engine window with DirectX rendering capabilities.
/// </summary>
public class RWindow : NativeWindow
{
	private const int MSAACount = 4;
	private const int MSAAQuality = 0;

	public IRenderContext RenderContext { get; protected set; } = null!;
	public ID3D11Device Device => _device ?? throw new InvalidOperationException("Device is not initialized or already disposed.");
	public IDXGISwapChain SwapChain => _swapChain ?? throw new InvalidOperationException("SwapChain is not initialized or already disposed.");
	public ID3D11RenderTargetView RenderTarget => _renderTarget ?? throw new InvalidOperationException("RenderTarget is not initialized or already disposed.");
	public bool EnableAntiAliasing { get; private set; } = false;
	
	private readonly SceneManager? _sceneManager;
	private readonly object _renderLock = new();
	private ID3D11Texture2D? _renderTexture;
	private ID3D11ShaderResourceView? _textureView;
	private ID3D11Device? _device;
	private IDXGISwapChain? _swapChain;
	private ID3D11RenderTargetView? _renderTarget;
	private ID3D11Texture2D? _msaaRenderTexture; // for anti-aliasing
	private ID3D11RenderTargetView? _msaaRenderTarget; // for anti-aliasing
	
	protected Thread? _renderThread;
	protected volatile bool _renderThreadRunning;
	
	public RWindow(int width, int height, string title = "Rabotora DirectX Release", bool antiAliasing = false)
		: base(title, width, height)
	{
		FixedRatio = true;
		RatioRefResolution = new Resolution(1280, 720);
		EnableAntiAliasing = antiAliasing;
		InitializeDirectX();
		_sceneManager = new SceneManager(RenderContext);
		SetupEventHandlers();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			lock (_renderLock)
			{
				_sceneManager?.Dispose();
				RenderContext?.Dispose();
				_renderTarget?.Dispose();
				_swapChain?.Dispose();
				_device?.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void InitializeDirectX()
	{
		var swapChainDesc = new SwapChainDescription()
		{
			BufferCount = 1,
			BufferUsage = Usage.RenderTargetOutput,
			OutputWindow = Handle,
			SampleDescription = new SampleDescription(1, 0), // always 1,0 for SwapChain
			Windowed = true,
			BufferDescription = new ModeDescription((uint) Width, (uint) Height, new Rational(60, 1), Format.B8G8R8A8_UNorm),
			SwapEffect = SwapEffect.Discard
		};

		var creationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
		creationFlags |= DeviceCreationFlags.Debug;
#endif
		
		D3D11.D3D11CreateDeviceAndSwapChain(adapter: null, DriverType.Hardware, creationFlags, [FeatureLevel.Level_11_0], swapChainDesc,
			out _swapChain!, out _device!, out _, out _).CheckError();
		
		CreateRenderTarget();
		RenderContext = new DirectXRenderContext(this);
	}

	private void CreateRenderTarget()
	{
		if (_swapChain == null || _device == null)
		{
			throw new InvalidOperationException("SwapChain or Device is not initialized or already disposed.");
		}
		
		_renderTarget?.Dispose();
		_msaaRenderTarget?.Dispose();
		_msaaRenderTexture?.Dispose();
		_renderTexture?.Dispose();
		_textureView?.Dispose();
		
		var sampleDesc = EnableAntiAliasing
			? new SampleDescription(MSAACount, MSAAQuality)
			: new SampleDescription(1, 0);
		
		using var backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
		
		if (EnableAntiAliasing)
		{
			var msaaTextureDesc = new Texture2DDescription()
			{
				Width = (uint) Width, Height = (uint) Height,
				MipLevels = 1,
				ArraySize = 1,
				Format = Format.B8G8R8A8_UNorm,
				SampleDescription = sampleDesc,
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.RenderTarget,
				CPUAccessFlags = CpuAccessFlags.None,
				MiscFlags = ResourceOptionFlags.None
			};

			_msaaRenderTexture = _device.CreateTexture2D(msaaTextureDesc);
			_msaaRenderTarget = _device.CreateRenderTargetView(_msaaRenderTexture);

			_renderTarget = _device.CreateRenderTargetView(backBuffer);
		}
		else
		{
			_renderTarget = _device.CreateRenderTargetView(backBuffer);
		}

		var textureDesc = new Texture2DDescription()
		{
			Width = (uint) Width, Height = (uint) Height,
			Format = Format.B8G8R8A8_UNorm,
			BindFlags = BindFlags.ShaderResource,
			CPUAccessFlags = CpuAccessFlags.Write,
			Usage = ResourceUsage.Dynamic,
			MiscFlags = ResourceOptionFlags.None,
			ArraySize = 1, MipLevels = 1,
			SampleDescription = new SampleDescription(1, 0), // Always 1,0 for CPU accessible textures
		};
		_renderTexture = _device.CreateTexture2D(textureDesc);
		_textureView = _device.CreateShaderResourceView(_renderTexture);
	}

	private void SetupEventHandlers()
	{
		Resized += OnResized;
	}

	private void OnResized(object? sender, EventArgs args)
	{
		if (Width == 0 || Height == 0)
		{
			return; // Skip any re-creation if the window is minimized
		}
		lock (_renderLock)
		{
			_renderTarget?.Dispose();
			_msaaRenderTarget?.Dispose();
			_msaaRenderTexture?.Dispose();
			_swapChain?.ResizeBuffers(1, (uint) Width, (uint) Height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
			CreateRenderTarget();

			if (RenderContext is DirectXRenderContext dx)
			{
				dx.RecreateD2DRenderTarget();
			}
		}
	}

	protected virtual void RenderFrame()
	{
		lock (_renderLock)
		{
			var activeRT = EnableAntiAliasing ? _msaaRenderTarget : _renderTarget;
		
			RenderContext.SetRenderTarget(activeRT!);
			RenderContext.BeginFrame();
			
			_sceneManager?.Render();
			
			RenderContext.EndFrame();

			if (EnableAntiAliasing && _msaaRenderTarget != null)
			{
				var context = _device!.ImmediateContext;
				using var backBuffer = _swapChain!.GetBuffer<ID3D11Texture2D>(0);
				context.ResolveSubresource(_msaaRenderTexture!, 0, backBuffer, 0, Format.B8G8R8A8_UNorm);
			}
		}
	}

	protected void RenderLoop()
	{
		var lastTime = DateTime.UtcNow;
		while (_renderThreadRunning && IsRunning)
		{
			var currentTime = DateTime.UtcNow;
			float deltaTime = (float)(currentTime - lastTime).TotalSeconds;
			lastTime = currentTime;
			
			_sceneManager?.Update(deltaTime);
			
			RenderFrame();
			Thread.Sleep(1); // prevent high CPU usage
		}
	}

	public override void Run()
	{
		_renderThreadRunning = true;
		_renderThread = new Thread(RenderLoop) { IsBackground = true };
		_renderThread.Start();
		
		base.Run();

		_renderThreadRunning = false;
		_renderThread.Join(); // wait for the render thread to finish
	}

	public void SetAntiAliasing(bool enable)
	{
		if (EnableAntiAliasing == enable) return;
		EnableAntiAliasing = enable;
		CreateRenderTarget(); // Recreate render targets with new settings
	}
}