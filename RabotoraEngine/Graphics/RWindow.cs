using Rabotora.Core;
using Rabotora.Graphics.Components;
using SkiaSharp;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Rabotora.Graphics;

public class RWindow : NativeWindow
{
	private const int MSAACount = 4;
	private const int MSAAQuality = 0;
	
	public IRenderContext RenderContext { get; protected init; } = null!;
	public ID3D11Device Device => _device ?? throw new InvalidOperationException("Device is not initialized or already disposed.");
	public IDXGISwapChain SwapChain => _swapChain ?? throw new InvalidOperationException("SwapChain is not initialized or already disposed.");
	public ID3D11RenderTargetView RenderTarget => _renderTarget ?? throw new InvalidOperationException("RenderTarget is not initialized or already disposed.");
	public bool EnableAntiAliasing { get; private set; } = false;
	
	private readonly List<UIComponent> _components = [];
	private ID3D11Texture2D? _renderTexture;
	private ID3D11ShaderResourceView? _textureView;
	private ID3D11Device? _device;
	private IDXGISwapChain? _swapChain;
	private ID3D11RenderTargetView? _renderTarget;
	private ID3D11Texture2D? _msaaRenderTexture; // for anti-aliasing
	private ID3D11RenderTargetView? _msaaRenderTarget; // also for anti-aliasing

	protected Thread? _renderThread;
	protected volatile bool _renderThreadRunning; // 控制渲染线程运行

	public RWindow(int width, int height, string title = "Rabotora Window", bool antiAliasing = false)
		: base(title, width, height)
	{
		FixedRatio = true; // 固定宽高比
		RatioRefResolution = new Resolution(1280, 720); // 参考分辨率
		EnableAntiAliasing = antiAliasing;
		InitializeDirectX();
		SetupEventHandlers();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_renderTarget?.Dispose();
			_swapChain?.Dispose();
			_device?.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeDirectX()
	{
		// 创建交换链描述
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

		// 创建设备和交换链
		D3D11.D3D11CreateDeviceAndSwapChain(adapter: null, DriverType.Hardware, DeviceCreationFlags.None, [FeatureLevel.Level_11_0], swapChainDesc,
			out _swapChain!, out _device!, out _, out _).CheckError();

		CreateRenderTarget();
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
		
		// 获取交换链的后缓冲，并基于其创建渲染目标视图
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
			Width = (uint)Width, Height = (uint)Height,
			Format = Format.B8G8R8A8_UNorm,
			BindFlags = BindFlags.ShaderResource,
			CPUAccessFlags = CpuAccessFlags.Write,
			Usage = ResourceUsage.Dynamic,
			MiscFlags = ResourceOptionFlags.None,
			ArraySize = 1, MipLevels = 1,
			SampleDescription = new SampleDescription(1, 0), // Always 1,0 for CPU accessible texture
		};
		_renderTexture = _device.CreateTexture2D(textureDesc);
		_textureView = _device.CreateShaderResourceView(_renderTexture);
	}

	private void SetupEventHandlers()
	{
		// 订阅窗口大小变化事件
		Resized += OnResized;
	}

	private void OnResized()
	{
		// 处理窗口大小变化：释放旧的RTV，调整交换链缓冲区，并重新创建RTV
		_renderTarget?.Dispose();
		_msaaRenderTarget?.Dispose();
		_msaaRenderTexture?.Dispose();
		_swapChain?.ResizeBuffers(1, (uint)Width, (uint)Height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
		CreateRenderTarget();
	}

	/// <summary>
	/// 渲染一帧
	/// </summary>
	protected virtual void RenderFrame()
	{
		var activeRenderTarget = EnableAntiAliasing ? _msaaRenderTarget : _renderTarget;

		RenderContext.SetRenderTarget(activeRenderTarget!);
		RenderContext.BeginFrame();
		foreach (var component in _components.Where(c => c.IsVisible))
		{
			component.Draw(RenderContext);
		}
		RenderContext.EndFrame();

		// 启用抗锯齿的情况下，将MSAA纹理解析(resolve)到最终输出渲染目标(render target)
		if (EnableAntiAliasing && _msaaRenderTexture != null)
		{
			var context = _device!.ImmediateContext;
			using var backBuffer = _swapChain!.GetBuffer<ID3D11Texture2D>(0);
			context.ResolveSubresource(_msaaRenderTexture, 0, backBuffer, 0, Format.B8G8R8A8_UNorm);
		}
	}

	/// <summary>
	/// 渲染线程的入口方法
	/// </summary>
	protected void RenderLoop()
	{
		var lastTime = DateTime.Now;
		while (_renderThreadRunning && IsRunning)
		{
			var currentTime = DateTime.Now;
			var deltaTime = (float)(currentTime - lastTime).TotalSeconds;
			lastTime = currentTime;

			// 更新所有组件
			foreach (var component in _components)
				component.Update(deltaTime);

			RenderFrame();
			Thread.Sleep(1);
		}
	}

	/// <summary>
	/// 重写Run方法，启动独立渲染线程后再进入主消息循环
	/// </summary>
	public override void Run()
	{
		// 启动独立渲染线程
		_renderThreadRunning = true;
		_renderThread = new Thread(RenderLoop) { IsBackground = true };
		_renderThread.Start();

		// 执行父类的消息循环（仅处理消息和更新）
		base.Run();

		// 消息循环退出后，停止渲染线程，并等待其结束
		_renderThreadRunning = false;
		_renderThread.Join();
	}
	
	public void AddComponent(UIComponent component) => _components.Add(component);
	public void RemoveComponent(UIComponent component) => _components.Remove(component);

	public void SetAntiAliasing(bool enable)
	{
		if (EnableAntiAliasing != enable)
		{
			EnableAntiAliasing = enable;
			CreateRenderTarget(); // Recreate render targets with new settings
		}
	}

	internal virtual void RenderWithSkia(Action<SKSurface> drawAction)
	{
		if (_renderTexture == null || _device == null) return;
		
		var context = _device.ImmediateContext;
		var mapped = context.Map(_renderTexture, 0, MapMode.WriteDiscard);
		using var surface = SKSurface.Create(new SKImageInfo(Width, Height, SKColorType.Bgra8888, SKAlphaType.Premul), mapped.DataPointer, (int) mapped.RowPitch); // Explicitly cast to `int` if necessary
		drawAction(surface);
		context.Unmap(_renderTexture, 0);
		
		using var backBuffer = _swapChain!.GetBuffer<ID3D11Texture2D>(0);
		context.CopyResource(backBuffer, _renderTexture);
	}
}