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
	public IRenderContext RenderContext { get; protected set; }
	public ID3D11Device Device => _device ?? throw new InvalidOperationException("Device is not initialized or already disposed.");
	public IDXGISwapChain SwapChain => _swapChain ?? throw new InvalidOperationException("SwapChain is not initialized or already disposed.");
	public ID3D11RenderTargetView RenderTarget => _renderTarget ?? throw new InvalidOperationException("RenderTarget is not initialized or already disposed.");
	
	private readonly List<UIComponent> _components = [];
	private ID3D11Texture2D? _renderTexture;
	private ID3D11ShaderResourceView? _textureView;
	private ID3D11Device? _device;
	private IDXGISwapChain? _swapChain;
	private ID3D11RenderTargetView? _renderTarget;

	private Thread? _renderThread;
	private volatile bool _renderThreadRunning; // 控制渲染线程运行

	public RWindow(int width, int height, string title = "Rabotora Window")
		: base(title, width, height)
	{
		FixedRatio = true; // 固定宽高比
		RatioRefResolution = new Resolution(1280, 720); // 参考分辨率
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
			SampleDescription = new SampleDescription(1, 0),
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
		// 获取交换链的后缓冲，并基于其创建渲染目标视图
		using var backBuffer = _swapChain!.GetBuffer<ID3D11Texture2D>(0);
		_renderTarget = _device!.CreateRenderTargetView(backBuffer);

		var textureDesc = new Texture2DDescription()
		{
			Width = (uint) Width, Height = (uint) Height,
			MipLevels = 1, ArraySize = 1, Format = Format.B8G8R8A8_UNorm,
			SampleDescription = new SampleDescription(1, 0),
			Usage = ResourceUsage.Default, BindFlags = BindFlags.ShaderResource,
			CPUAccessFlags = CpuAccessFlags.None, MiscFlags = ResourceOptionFlags.None
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
		_swapChain?.ResizeBuffers(1, (uint)Width, (uint)Height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
		CreateRenderTarget();
	}

	/// <summary>
	/// 渲染一帧
	/// </summary>
	protected virtual void RenderFrame()
	{
		RenderContext.BeginFrame();
		foreach (var component in _components.Where(c => c.IsVisible))
		{
			component.Draw(RenderContext);
		}
		RenderContext.EndFrame();
	}

	/// <summary>
	/// 渲染线程的入口方法
	/// </summary>
	private void RenderLoop()
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