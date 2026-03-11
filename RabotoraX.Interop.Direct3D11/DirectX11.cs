using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Threading;
using RabotoraX.Core.Utility;
using Vortice.D3DCompiler;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DirectWrite;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.WIC;
using AlphaMode = Vortice.DCommon.AlphaMode;
using CullMode = RabotoraX.Core.Graphics.CullMode;
using FeatureLevel = Vortice.Direct3D.FeatureLevel;
using FillMode = Vortice.Direct3D11.FillMode;
using InputElementDescription = RabotoraX.Core.Graphics.InputElementDescription;
using PixelFormat = Vortice.DCommon.PixelFormat;
using PrimitiveTopology = RabotoraX.Core.Graphics.PrimitiveTopology;

namespace RabotoraX.Interop.Direct3D11;

[UsedImplicitly]
public partial class DirectX11 : INativeGraphicsAPI
{
	public string ApiName => "Direct3D 11";
	public string DeviceName { get; private set; } = "Unknown";
	public bool IsInitialized { get; private set; }
	public (int Width, int Height) FramebufferSize { get; private set; }
	public Lock RenderLock { get; } = new();
	public bool IgnoreAllPresents { get; set; } = false;
	
	private ID3D11Device? _device;
	private ID3D11DeviceContext? _context;
	private IDXGISwapChain? _swapChain;
	
	private ID3D11RenderTargetView? _renderTargetView;
	private ID3D11DepthStencilView? _depthStencilView;
	private ID3D11Texture2D? _depthBuffer;
	
	private ID3D11RasterizerState? _rasterizerStateCullBack;
	private ID3D11RasterizerState? _rasterizerStateCullFront;
	private ID3D11RasterizerState? _rasterizerStateCullNone;
	private readonly Stack<CullMode> _cullModeStack = new([CullMode.Back]); // 默认是 Back Cull
	
	private ID3D11DepthStencilState? _depthStateDefault;
	private ID3D11DepthStencilState? _depthStateReadOnly;
	private ID3D11DepthStencilState? _depthStateNone;
	
	private ID2D1Factory1? _d2dFactory;
	private ID2D1Device? _d2dDevice;
	private ID2D1DeviceContext? _d2dContext;
	private ID2D1Bitmap? _d2dTargetBitmap;
	private IDWriteFactory? _dwriteFactory;
	private D2DContextImpl? _d2dImpl;
	private IWICImagingFactory? _wicFactory;

	private partial class D2DContextImpl : INative2DRenderContext;

	public void Initialize(INativeWindow window)
	{
		if (IsInitialized) throw new InvalidOperationException("Graphics API is already initialized.");
		
		FramebufferSize = window.Size;
		CreateD3D11(window);
		CreateD2D();
		
		IsInitialized = true;
	}

	private void CreateD3D11(INativeWindow window)
	{
		CreateDevice();
		CreateSwapChain(window);
		CreateResources(window.Size.Width, window.Size.Height);
		CreateRasterizerStates();
		CreateDepthStencilTest();
	}

	private void CreateDevice()
	{
		var featureLevels = new[]
		{
			FeatureLevel.Level_11_1, FeatureLevel.Level_11_0
		};
		
		const DeviceCreationFlags creationFlags = DeviceCreationFlags.BgraSupport;

		if (D3D11.D3D11CreateDevice(null, DriverType.Hardware, creationFlags, featureLevels, out _device, out _context).Failure)
		{
			// Try WARP software rasterizer if hardware creation fails (e.g. on unsupported hardware or remote desktop)
			D3D11.D3D11CreateDevice(null, DriverType.Warp, creationFlags, featureLevels, out _device, out _context).CheckError();
		}
		
		DeviceName = _device.FeatureLevel switch
		{
			FeatureLevel.Level_11_1 => "Direct3D 11.1",
			FeatureLevel.Level_11_0 => "Direct3D 11.0",
			_ => "Direct3D (Unknown Feature Level)"
		};
	}

	private void CreateSwapChain(INativeWindow window)
	{
		using var dxgiDevice = _device!.QueryInterface<IDXGIDevice>();
		using var adapter = dxgiDevice.GetAdapter();
		using var factory = adapter.GetParent<IDXGIFactory2>();

		var swapChainDesc = new SwapChainDescription()
		{
			BufferCount = 3, // Triple buffering for better performance and smoother frame pacing. DXGI will handle the synchronization to avoid tearing.
			BufferDescription = new ModeDescription((uint) window.Size.Width, (uint) window.Size.Height, Format.R8G8B8A8_UNorm),
			Windowed = true,
			OutputWindow = window.Handle,
			SampleDescription = new SampleDescription(1, 0),
			SwapEffect = SwapEffect.FlipDiscard,
			BufferUsage = Usage.RenderTargetOutput,
			Flags = SwapChainFlags.AllowModeSwitch
		};
		
		_swapChain = factory.CreateSwapChain(_device, swapChainDesc);
		
		factory.MakeWindowAssociation(window.Handle, WindowAssociationFlags.IgnoreAltEnter); // Disable DXGI Default Alt+Enter FullScreen behavior
	}

	private void CreateResources(int width, int height)
	{
		using var backBuffer = _swapChain!.GetBuffer<ID3D11Texture2D>(0);
		_renderTargetView = _device!.CreateRenderTargetView(backBuffer);

		var depthDesc = new Texture2DDescription()
		{
			Width = (uint) width,
			Height = (uint) height,
			MipLevels = 1,
			ArraySize = 1,
			Format = Format.D24_UNorm_S8_UInt, // Standard 24-bit depth + 8-bit stencil template
			SampleDescription = new SampleDescription(1, 0),
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.DepthStencil,
			CPUAccessFlags = CpuAccessFlags.None,
			MiscFlags = ResourceOptionFlags.None
		};
		
		_depthBuffer = _device.CreateTexture2D(depthDesc);
		_depthStencilView = _device.CreateDepthStencilView(_depthBuffer);
		
		if (_d2dContext != null)
		{
			using var backBufferSurface = _swapChain.GetBuffer<IDXGISurface>(0);
	
			var bitmapProps = new BitmapProperties1(
				new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied),
				96, 96,
				BitmapOptions.Target | BitmapOptions.CannotDraw);

			// 重建 D2D Bitmap
			_d2dTargetBitmap = _d2dContext.CreateBitmapFromDxgiSurface(backBufferSurface, bitmapProps);
			_d2dContext.Target = _d2dTargetBitmap;
		}
		
		SetViewport(0, 0, width, height);
	}

	private void CreateRasterizerStates()
	{
		var descBack = new RasterizerDescription(Vortice.Direct3D11.CullMode.Back, FillMode.Solid);
		_rasterizerStateCullBack = _device!.CreateRasterizerState(descBack);
		
		var descFront = new RasterizerDescription(Vortice.Direct3D11.CullMode.Front, FillMode.Solid);
		_rasterizerStateCullFront = _device.CreateRasterizerState(descFront);
		
		var descNone = new RasterizerDescription(Vortice.Direct3D11.CullMode.None, FillMode.Solid);
		_rasterizerStateCullNone = _device.CreateRasterizerState(descNone);
	}

	private void CreateDepthStencilTest()
	{
		var descDefault = new DepthStencilDescription()
		{
			DepthEnable = true,
			DepthWriteMask = DepthWriteMask.All,
			DepthFunc = ComparisonFunction.Less
		};
		_depthStateDefault = _device!.CreateDepthStencilState(descDefault);
		
		var descReadOnly = new DepthStencilDescription()
		{
			DepthEnable = true,
			DepthWriteMask = DepthWriteMask.Zero, // Disable depth writing
			DepthFunc = ComparisonFunction.Less
		};
		_depthStateReadOnly = _device.CreateDepthStencilState(descReadOnly);
		
		var descNone = new DepthStencilDescription()
		{
			DepthEnable = false,
			DepthWriteMask = DepthWriteMask.Zero,
			DepthFunc = ComparisonFunction.Always
		};
		_depthStateNone = _device.CreateDepthStencilState(descNone);
		
		SetDepthEnabled(true); // Enable depth testing and writing by default
	}
	
	private void CreateD2D()
	{
		_d2dFactory = D2D1.D2D1CreateFactory<ID2D1Factory1>();
		_dwriteFactory = DWrite.DWriteCreateFactory<IDWriteFactory>();
		_wicFactory = new IWICImagingFactory();
		
		using var dxgiDevice = _device!.QueryInterface<IDXGIDevice>();
		_d2dDevice = _d2dFactory.CreateDevice(dxgiDevice);
		_d2dContext = _d2dDevice.CreateDeviceContext(DeviceContextOptions.None);

		BindD2DTarget();

		_d2dImpl = new D2DContextImpl(this);
	}

	private void BindD2DTarget()
	{
		using var backBuffer = _swapChain!.GetBuffer<IDXGISurface>(0);
		
		var bitmapProps = new BitmapProperties1(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied),
			96, 96, BitmapOptions.Target | BitmapOptions.CannotDraw);
		
		using var targetBitmap = _d2dContext!.CreateBitmapFromDxgiSurface(backBuffer, bitmapProps);
		_d2dContext.Target = targetBitmap;
	}

	private void ReleaseResources()
	{
		if (_d2dContext != null)
		{
			_d2dContext.Target = null;
		}
		_d2dTargetBitmap?.Dispose();
		_d2dTargetBitmap = null;
		_renderTargetView?.Dispose();
		_renderTargetView = null;
		_depthStencilView?.Dispose();
		_depthStencilView = null;
		_depthBuffer?.Dispose();
		_depthBuffer = null;
		
		_context?.ClearState();
		_context?.Flush();
	}

	public void Resize(int width, int height)
	{
		if (IgnoreAllPresents) return;
		lock (RenderLock)
		{
			if (!IsInitialized) return;
			if (width <= 0 || height <= 0) return; // Ignore invalid sizes (e.g. when minimizing)
		
			FramebufferSize = (width, height);
		
			_context!.OMSetRenderTargets((ID3D11RenderTargetView?)null!); // Unbind before resizing
			ReleaseResources();
		
			_swapChain!.ResizeBuffers(0, (uint) width, (uint) height, Format.Unknown, SwapChainFlags.None).CheckError();
		
			CreateResources(width, height);
		}
	}

	public void BeginFrame()
	{
		_context!.OMSetRenderTargets(_renderTargetView!, _depthStencilView);
	}

	public void Clear(float r, float g, float b, float a)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(Clear));
		_context!.ClearRenderTargetView(_renderTargetView!, new Color4(r, g, b, a));
		_context!.ClearDepthStencilView(_depthStencilView!, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
	}

	public void EndFrame()
	{
		// we don't need to do anything here for DX11, but we could add some GPU-side synchronization if needed (e.g. for readback or async compute)
	}

	public void Present(bool vsync = true)
	{
		if (IgnoreAllPresents) return;
		_context!.Flush();
		_swapChain!.Present(vsync ? 1u : 0u, PresentFlags.None)/*.CheckError()*/; // Present 失败通常是因为窗口被最小化了，这时候不需要抛异常
	}

	public void WaitIdle()
	{
		// DX11 doesn't need to explicitly wait for idle, but we can flush the context to ensure all commands are submitted
		_context!.Flush();
	}

	public IGpuBuffer CreateBuffer<T>(BufferType type, T[] data) where T : unmanaged
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(CreateBuffer));
		var flags = type switch
		{
			BufferType.VertexBuffer => BindFlags.VertexBuffer,
			BufferType.IndexBuffer => BindFlags.IndexBuffer,
			BufferType.ConstantBuffer => BindFlags.ConstantBuffer,
			_ => BindFlags.None
		};
		
		int size = data.Length * Unsafe.SizeOf<T>();
		
		var desc = new BufferDescription((uint)size, flags);
		var buffer = _device!.CreateBuffer(data, desc);
		return new DX11Buffer(buffer, size);
	}

	public IGpuBuffer CreateBuffer(BufferType type, int sizeInBytes)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(CreateBuffer));
		var flags = type switch
		{
			BufferType.VertexBuffer => BindFlags.VertexBuffer,
			BufferType.IndexBuffer => BindFlags.IndexBuffer,
			BufferType.ConstantBuffer => BindFlags.ConstantBuffer,
			_ => BindFlags.None
		};

		var desc = new BufferDescription((uint)sizeInBytes, flags);
		var buffer = _device!.CreateBuffer(desc);
		return new DX11Buffer(buffer, sizeInBytes);
	}

	public void UpdateBuffer<T>(IGpuBuffer buffer, T[] data) where T : unmanaged
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(UpdateBuffer));
		if (buffer is DX11Buffer dxBuffer)
		{
			try
			{
				var mappedResource = _context!.Map(dxBuffer.NativeBuffer, 0, MapMode.WriteDiscard);

				unsafe
				{
					Unsafe.Copy(mappedResource.DataPointer.ToPointer(), ref data);
				}
				
				_context.Unmap(dxBuffer.NativeBuffer, 0);
			}
			catch
			{
				_context!.UpdateSubresource(data, dxBuffer.NativeBuffer);
			}
		}
	}

	public void UpdateBuffer<T>(IGpuBuffer buffer, ref T data) where T : unmanaged
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(UpdateBuffer));
		if (buffer is DX11Buffer dxBuffer)
		{
			_context!.UpdateSubresource(in data, dxBuffer.NativeBuffer); // in is a read-only version of ref, which is fine for UpdateSubresource
		}
	}

	public IShader CreateShader(ShaderType type, string sourceCode, string entryPoint = "main", InputElementDescription[]? inputLayout = null)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(CreateShader));
		string profile = type == ShaderType.VertexShader ? "vs_5_0" : "ps_5_0"; // We target Shader Model 5.0 for maximum compatibility with DX11 feature levels

		Blob? shaderBlob = null;
		Blob? errorBlob = null;

		try
		{
			var result = Compiler.Compile(sourceCode, entryPoint, "Source", profile, out shaderBlob, out errorBlob);
			if (result.Failure)
			{
				string message = errorBlob.AsString();
				throw new InvalidOperationException($"Shader compilation failed: {message}");
			}
			
			if (shaderBlob == null)
			{
				throw new InvalidOperationException("Shader compilation failed: Unknown error (no blob returned)");
			}
			
			var shaderByteCode = shaderBlob.AsBytes();if (type == ShaderType.VertexShader)
			{
				var vs = _device!.CreateVertexShader(shaderByteCode);
				ID3D11InputLayout? layout = null;

				if (inputLayout != null)
				{
					var elements = new Vortice.Direct3D11.InputElementDescription[inputLayout.Length];
					for (int i = 0; i < inputLayout.Length; i++)
					{
						var item = inputLayout[i];
                        
						// 格式推导
						var format = item.FormatSize switch
						{
							1 => Format.R32_Float,
							2 => Format.R32G32_Float,
							3 => Format.R32G32B32_Float,
							4 => Format.R32G32B32A32_Float,
							_ => Format.R32G32B32_Float 
						};

						elements[i] = new Vortice.Direct3D11.InputElementDescription(
							item.SemanticName, 
							(uint)item.SemanticIndex, 
							format, 
							(uint)item.Offset, 
							0 // Slot 0
						);
					}
                    
					layout = _device.CreateInputLayout(elements, shaderByteCode);
				}

				return new DX11Shader(vs, ShaderType.VertexShader, layout, shaderByteCode);
			}
			else
			{
				var ps = _device!.CreatePixelShader(shaderByteCode);
				return new DX11Shader(ps, ShaderType.FragmentShader);
			}
		}
		finally
		{
			// 释放编译过程中产生的临时 Blob，因为 CreateVertexShader 会拷贝一份数据
			shaderBlob?.Dispose();
			errorBlob?.Dispose();
		}
	}

	public IShader CreateShaderProgram(string vertSource, string fragSource, InputElementDescription[] inputLayout, string vertEntryPoint = "main", string fragEntryPoint = "main")
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(CreateShaderProgram));
		var vertexShader = CreateShader(ShaderType.VertexShader, vertSource, vertEntryPoint, inputLayout) as DX11Shader;
		var fragmentShader = CreateShader(ShaderType.FragmentShader, fragSource, fragEntryPoint) as DX11Shader;

		if (vertexShader == null || fragmentShader == null)
		{
			throw new InvalidOperationException("Failed to create shader program: Invalid vertex or fragment shader.");
		}

		return new DX11ShaderProgram((ID3D11VertexShader)vertexShader.NativeShader, (ID3D11PixelShader)fragmentShader.NativeShader, vertexShader.InputLayout);
	}

	public void SetViewport(float x, float y, float width, float height, float minDepth = 0, float maxDepth = 1)
	{
		_context!.RSSetViewport(x, y, width, height, minDepth, maxDepth);
	}

	public void SetShader(IShader shader)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(SetShader));
		switch (shader)
		{
			case DX11Shader {Type: ShaderType.VertexShader} dxShader:
			{
				_context!.VSSetShader((ID3D11VertexShader)dxShader.NativeShader);
				if (dxShader.InputLayout != null)
				{
					_context!.IASetInputLayout(dxShader.InputLayout);
				}

				break;
			}
			case DX11Shader {Type: ShaderType.FragmentShader} dxShader:
			{
				_context!.PSSetShader((ID3D11PixelShader)dxShader.NativeShader);
				break;
			}
			case DX11ShaderProgram {Type: ShaderType.VertexFragment} program:
			{
				_context!.VSSetShader(program.VertexShader);
				_context!.PSSetShader(program.FragmentShader);
				if (program.InputLayout != null)
				{
					_context!.IASetInputLayout(program.InputLayout);
				}
				break;
			}
		}
	}

	public void SetVertexBuffer(IGpuBuffer buffer, int stride, int offset = 0)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(SetVertexBuffer));
		if (buffer is DX11Buffer dxBuffer)
		{
			_context!.IASetVertexBuffer(0, dxBuffer.NativeBuffer, (uint)stride, (uint)offset); // Slot 0
		}
	}

	public void SetIndexBuffer(IGpuBuffer buffer)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(SetIndexBuffer));
		if (buffer is DX11Buffer dxBuffer)
		{
			_context!.IASetIndexBuffer(dxBuffer.NativeBuffer, Format.R32_UInt, 0);
		}
	}

	public void SetConstantBuffer(int slot, IGpuBuffer buffer, ShaderType stage)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(SetConstantBuffer));
		if (buffer is DX11Buffer dxBuffer)
		{
			if (stage == ShaderType.VertexShader)
			{
				_context!.VSSetConstantBuffer((uint)slot, dxBuffer.NativeBuffer);
			}
			else if (stage == ShaderType.FragmentShader)
			{
				_context!.PSSetConstantBuffer((uint)slot, dxBuffer.NativeBuffer);
			}
		}
	}
	
	[MustDisposeResource]
	public IDisposable SetCullMode(CullMode mode)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(SetCullMode));
		// _lastCullMode = _context!.RSGetState().GetManagedCullMode(); // Store the current cull mode before changing
		// if (_lastCullMode == mode) return; // No change needed
		var lastCullMode = _cullModeStack.Peek();
		if (lastCullMode == mode) return new AutoScope(); // No change needed
		if (_cullModeStack.Count >= 32)
		{
			Debug.Assert(false, "History Cull Modes Count >= 32: too many nested SetCullMode calls without ResumeCullMode. Has any code logic causing circular Cull Mode changes?");
		}
		_cullModeStack.Push(mode);
		var state = mode switch
		{
			CullMode.Back => _rasterizerStateCullBack,
			CullMode.Front => _rasterizerStateCullFront,
			CullMode.None => _rasterizerStateCullNone,
			_ => _rasterizerStateCullBack
		};
		
		_context!.RSSetState(state);
		
		return new AutoScope(onExit: ResumeCullMode);
	}

	public void ResumeCullMode()
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(ResumeCullMode));
		if (_cullModeStack.Count <= 1) return;
		_cullModeStack.Pop(); // ignore the popped value because we will peek the new current mode
		var currentMode = _cullModeStack.Peek();
		var state = currentMode switch
		{
			CullMode.Back => _rasterizerStateCullBack,
			CullMode.Front => _rasterizerStateCullFront,
			CullMode.None => _rasterizerStateCullNone,
			_ => _rasterizerStateCullBack
		};
			
		_context!.RSSetState(state);
	}

	public void SetDepthEnabled(bool enabled, bool writeEnabled = true)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(SetDepthEnabled));
		if (!enabled)
		{
			_context!.OMSetDepthStencilState(_depthStateNone);
		}
		else
		{
			_context!.OMSetDepthStencilState(writeEnabled ? _depthStateDefault : _depthStateReadOnly);
		}
	}

	public void Draw(int vertexCount, int startVertexLocation, PrimitiveTopology topology = PrimitiveTopology.TriangleList)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(Draw));
		_context!.IASetPrimitiveTopology(ToDxTopology(topology));
		_context!.Draw((uint)vertexCount, (uint)startVertexLocation);
	}

	public void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation, PrimitiveTopology topology = PrimitiveTopology.TriangleList)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(DrawIndexed));
		_context!.IASetPrimitiveTopology(ToDxTopology(topology));
		_context!.DrawIndexed((uint)indexCount, (uint)startIndexLocation, baseVertexLocation);
	}

	public INative2DRenderContext? Get2DContext()
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(Get2DContext));
		return _d2dImpl;
	}

	public void Dispose()
	{
		ReleaseResources();
		_wicFactory?.Dispose();
		_dwriteFactory?.Dispose();
		_d2dFactory?.Dispose();
		_d2dContext?.Dispose();
		_d2dImpl?.Dispose();
		_d2dDevice?.Dispose();
		
		_swapChain?.Dispose();
		_context?.Dispose();
		_device?.Dispose();
		
		_rasterizerStateCullBack?.Dispose();
		_rasterizerStateCullFront?.Dispose();
		_rasterizerStateCullNone?.Dispose();
		
		_depthStateDefault?.Dispose();
		_depthStateReadOnly?.Dispose();
		_depthStateNone?.Dispose();
		
		IsInitialized = false;
		GC.SuppressFinalize(this);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vortice.Direct3D.PrimitiveTopology ToDxTopology(PrimitiveTopology topology)
	{
		return topology switch
		{
			PrimitiveTopology.TriangleList => Vortice.Direct3D.PrimitiveTopology.TriangleList,
			PrimitiveTopology.TriangleStrip => Vortice.Direct3D.PrimitiveTopology.TriangleStrip,
			PrimitiveTopology.LineList => Vortice.Direct3D.PrimitiveTopology.LineList,
			PrimitiveTopology.PointList => Vortice.Direct3D.PrimitiveTopology.PointList,
			_ => Vortice.Direct3D.PrimitiveTopology.TriangleList
		};
	}
}