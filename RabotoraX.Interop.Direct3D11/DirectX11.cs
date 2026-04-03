using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using JetBrains.Annotations;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Threading;
using RabotoraX.Core.UI;
using RabotoraX.Core.Utility;
using RabotoraX.Interop.Direct3D11.Rendering;
using TerraFX.Interop.Windows;
using Vortice.D3DCompiler;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DirectWrite;
using Vortice.DXGI;
using Vortice.Mathematics;
using static TerraFX.Interop.Windows.Windows;
using AlphaMode = Vortice.DCommon.AlphaMode;
using BlendDescription = Vortice.Direct3D11.BlendDescription;
using BlendOperation = RabotoraX.Core.Graphics.BlendOperation;
using CullMode = RabotoraX.Core.Graphics.CullMode;
using FeatureLevel = Vortice.Direct3D.FeatureLevel;
using FillMode = Vortice.Direct3D11.FillMode;
using InputElementDescription = RabotoraX.Core.Graphics.InputElementDescription;
using IWICImagingFactory = Vortice.WIC.IWICImagingFactory;
using PixelFormat = Vortice.DCommon.PixelFormat;
using PrimitiveTopology = RabotoraX.Core.Graphics.PrimitiveTopology;

namespace RabotoraX.Interop.Direct3D11;

[UsedImplicitly, SupportedOSPlatform("windows")]
public partial class DirectX11 : INativeGraphicsAPI
{
	public string ApiName => "Direct3D 11";
	public string DeviceName { get; private set; } = "Unknown";
	public bool IsInitialized { get; private set; }
	public (int Width, int Height) FramebufferSize { get; private set; }
	public Lock RenderLock { get; } = new();
	public bool IgnoreAllPresents { get; set; } = false;
	public D3D11RenderTexture? MainRenderTexture { get; private set; }
	
	private readonly Stopwatch _waitTimer = Stopwatch.StartNew();

	private ID3D11Device? _device;
	private ID3D11DeviceContext? _context;
	private INativeRenderTexture? _currentRenderTarget;
	private IDXGISwapChain? _swapChain;
	
	private ID3D11RenderTargetView? _rtvBackBuffer;
	private ID3D11DepthStencilView? _depthStencilView;
	private ID3D11Texture2D? _depthBuffer;
	
	private ID3D11RasterizerState? _rasterizerStateCullBack;
	private ID3D11RasterizerState? _rasterizerStateCullFront;
	private ID3D11RasterizerState? _rasterizerStateCullNone;
	private readonly Stack<CullMode> _cullModeStack = new([CullMode.Back]); // 默认是 Back Cull
	private readonly Stack<BlendState> _blendStateStack = new([BlendState.AlphaBlend]); // 默认是 Alpha Blend (Straight Alpha Blending)
	private readonly Dictionary<BlendState, ID3D11BlendState> _blendStateCache = [];
 	
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
	private nint _frameWaitableObject;

	private int _pendingWidth, _pendingHeight;
	private bool _resizePending;

	private partial class D2DContextImpl : INative2DRenderContext;

	public void Initialize(INativeWindow window)
	{
		if (IsInitialized) throw new InvalidOperationException("Graphics API is already initialized.");
		
		FramebufferSize = window.Size;
		CreateD3D11(window);
		CreateD2D();
		
		lock (RenderLock)
		{
			ApplySetRenderTarget(null);
			ApplyClear(0, 0, 0, 1);
#if DEBUG
			Console.WriteLine("[DirectX11] Redirected to MainRenderTexture. All draw calls will be collected on MainRenderTexture and Present will copy the whole texture to back buffer in one call");
#endif
        
			// Make window begin with black screen instead of white flashes
			_swapChain!.Present(0, PresentFlags.None); 
		}
		
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

		var swapChainDesc = new SwapChainDescription1()
		{
			Width = (uint)window.Size.Width,
			Height = (uint)window.Size.Height,
			Format = Format.R8G8B8A8_UNorm,
			Stereo = false,
			SampleDescription = new SampleDescription(1, 0),
			BufferUsage = Usage.RenderTargetOutput,
			BufferCount = 3,
			Scaling = Scaling.None,
			SwapEffect = SwapEffect.FlipDiscard, // 必须是 Flip 模式
			AlphaMode = Vortice.DXGI.AlphaMode.Ignore,
			// 关键：开启内核等待对象标志，这是解决 CPU 空转的“银弹”
			Flags = SwapChainFlags.FrameLatencyWaitableObject
		};
		
		_swapChain = factory.CreateSwapChainForHwnd(_device, window.Handle, swapChainDesc);
		
		using var swapChain2 = _swapChain.QueryInterfaceOrNull<IDXGISwapChain2>();
		if (swapChain2 != null)
		{
			swapChain2.MaximumFrameLatency = 1;
			_frameWaitableObject = swapChain2.FrameLatencyWaitableObject;
		}
		else
		{
			Debug.WriteLine("Warning: DXGI 1.3 Waitable Object is not supported on this system.");
			_frameWaitableObject = nint.Zero;
		}
		
		factory.MakeWindowAssociation(window.Handle, WindowAssociationFlags.IgnoreAltEnter); // Disable DXGI Default Alt+Enter FullScreen behavior
	}

	private void CreateResources(int width, int height)
	{
		using var backBuffer = _swapChain!.GetBuffer<ID3D11Texture2D>(0);
		_rtvBackBuffer = _device!.CreateRenderTargetView(backBuffer);
		MainRenderTexture = (D3D11RenderTexture)CreateRenderTexture(width, height);
		_currentRenderTarget = MainRenderTexture;

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
			BindD2DTarget();
		}
		
		ApplySetViewport(0, 0, width, height);
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
		
		ApplySetDepthEnabled(true); // Enable depth testing and writing by default
	}
	
	private void CreateD2D()
	{
		_d2dFactory = D2D1.D2D1CreateFactory<ID2D1Factory1>();
		_dwriteFactory = DWrite.DWriteCreateFactory<IDWriteFactory>();
		RUIService.Initialize(this); // Initialize text factory for UI system
		_wicFactory = new IWICImagingFactory();
		
		using var dxgiDevice = _device!.QueryInterface<IDXGIDevice>();
		_d2dDevice = _d2dFactory.CreateDevice(dxgiDevice);
		_d2dContext = _d2dDevice.CreateDeviceContext(DeviceContextOptions.None);
		_d2dContext.UnitMode = UnitMode.Pixels; // Use pixel units for easier integration with D3D11 render targets
		_d2dContext.SetDpi(96, 96);

		BindD2DTarget();

		_d2dImpl = new D2DContextImpl(this);
	}

	private void BindD2DTarget()
	{
		// using var backBuffer = _swapChain!.GetBuffer<IDXGISurface>(0);
		using var mainRTBuffer = MainRenderTexture!.Texture.QueryInterface<IDXGISurface>();
		
		var bitmapProps = new BitmapProperties1(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied),
			96, 96, BitmapOptions.Target | BitmapOptions.CannotDraw);
		
		_d2dTargetBitmap = _d2dContext!.CreateBitmapFromDxgiSurface(mainRTBuffer, bitmapProps);
		_d2dContext.Target = _d2dTargetBitmap;
	}

	private void ReleaseResources()
	{
		if (_d2dContext != null)
		{
			_d2dContext.Target = null;
		}
		_d2dTargetBitmap?.Dispose();
		_d2dTargetBitmap = null;
		_rtvBackBuffer?.Dispose();
		_rtvBackBuffer = null;
		MainRenderTexture?.Dispose();
		MainRenderTexture = null;
		
		_depthStencilView?.Dispose();
		_depthStencilView = null;
		_depthBuffer?.Dispose();
		_depthBuffer = null;
		
		_context?.ClearState();
		_context?.Flush();
	}

	public void Resize(int width, int height)
	{
		lock (RenderLock)
		{
			_pendingWidth = width;
			_pendingHeight = height;
			_resizePending = true;
		}
		if (!MultiThreadService.IsRenderThread)
		{
			MultiThreadService.Invoke(() =>
			{
				lock (RenderLock)
				{
					if (!_resizePending) return;
					_resizePending = false;
					ResizeInternal(_pendingWidth, _pendingHeight);
				}
			});
			return;
		}
    
		lock (RenderLock)
		{
			if (!_resizePending) return;
			_resizePending = false;
			ResizeInternal(_pendingWidth, _pendingHeight);
		}
	}

	private void ResizeInternal(int width, int height)
	{
		if (!IsInitialized) return;
		if (width <= 0 || height <= 0) return;

		_context!.OMSetRenderTargets((ID3D11RenderTargetView)null!);
		if (_d2dContext != null) _d2dContext.Target = null;
        
		_rtvBackBuffer?.Dispose();
		_rtvBackBuffer = null;
		_d2dTargetBitmap?.Dispose();
		_d2dTargetBitmap = null;
        
		_context!.Flush();

		FramebufferSize = (width, height);
        
		const SwapChainFlags resizeFlags = SwapChainFlags.FrameLatencyWaitableObject;
		var hr = _swapChain!.ResizeBuffers(0, (uint)width, (uint)height, Format.Unknown, resizeFlags);
        
		if (hr.Failure)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			_swapChain.ResizeBuffers(0, (uint)width, (uint)height, Format.Unknown, resizeFlags).CheckError();
		}

		using var swapChain2 = _swapChain.QueryInterfaceOrNull<IDXGISwapChain2>();
		if (swapChain2 != null) _frameWaitableObject = swapChain2.FrameLatencyWaitableObject;

		CreateResources(width, height);
	}

	public IEnumerable<ShaderPlatform> GetSupportedShaderPlatforms()
	{
		return [ShaderPlatform.HLSL11, ShaderPlatform.HLSL2D11];
	}

	public void BeginFrame()
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(BeginFrame));
		ApplySetRenderTarget(null); // 默认回 BackBuffer
		ApplySetViewport(0, 0, FramebufferSize.Width, FramebufferSize.Height);
		ApplyClear(0, 0, 0, 1);
	}

	public void EndFrame()
	{
		// we don't need to do anything here for DX11, but we could add some GPU-side synchronization if needed (e.g. for readback or async compute)
	}

	public void Present(bool vsync = true)
	{
		if (IgnoreAllPresents) return;
		
		using var backBuffer = _swapChain!.GetBuffer<ID3D11Texture2D>(0);
		_context!.OMSetRenderTargets(_rtvBackBuffer!);
		_context!.CopyResource(backBuffer, MainRenderTexture!.Texture);
		
		// TODO: To enable production-ready advanced shader supported logic (use Draw Call to make MainTexture on-screen instead of CopyResource), uncomment the following line and remove the CopyResource above.
		// _context!.RSSetState(_rasterizerStateCullBack);
		// _context!.OMSetBlendState(null, null, 0xFFFFFFFF);

		_swapChain!.Present(vsync ? 1u : 0u, PresentFlags.None);
		_context!.OMSetRenderTargets(MainRenderTexture.RTV, _depthStencilView);
	}
	
	public void WaitNextFrameReady()
	{
		if (IgnoreAllPresents || _frameWaitableObject == nint.Zero)
		{
			Thread.Yield();
			return;
		}
		uint result = WaitForSingleObject((HANDLE) _frameWaitableObject, 1000);

		if (_waitTimer.ElapsedMilliseconds >= 1000)
		{
			_waitTimer.Restart();
		}
		if (result != 0)
		{
			Thread.Yield();
		}
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

	public INativeShader CreateNativeShader(ShaderType type, string sourceCode, string entryPoint = "main", InputElementDescription[]? inputLayout = null)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(CreateNativeShader));
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
						var format = item.Format;

						elements[i] = new Vortice.Direct3D11.InputElementDescription(
							item.SemanticName, 
							(uint)item.SemanticIndex, 
							MapFormat(format),
							(uint)item.AlignedByteOffset, 
							0 // Slot 0
						);
					}
                    
					layout = _device.CreateInputLayout(elements, shaderByteCode);
				}

				return new DX11Shader(vs, ShaderType.VertexShader, layout, shaderByteCode)
				{
					VertSource = sourceCode,
				};
			}
			else
			{
				var ps = _device!.CreatePixelShader(shaderByteCode);
				return new DX11Shader(ps, ShaderType.FragmentShader)
				{
					FragSource = sourceCode
				};
			}
		}
		finally
		{
			// 释放编译过程中产生的临时 Blob，因为 CreateVertexShader 会拷贝一份数据
			shaderBlob?.Dispose();
			errorBlob?.Dispose();
		}
	}

	public INativeShader CreateNativeShaderProgram(string vertSource, string fragSource, InputElementDescription[] inputLayout, string vertEntryPoint = "main", string fragEntryPoint = "main")
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(CreateNativeShaderProgram));
		var vertexShader = CreateNativeShader(ShaderType.VertexShader, vertSource, vertEntryPoint, inputLayout) as DX11Shader;
		var fragmentShader = CreateNativeShader(ShaderType.FragmentShader, fragSource, fragEntryPoint) as DX11Shader;

		if (vertexShader == null || fragmentShader == null)
		{
			throw new InvalidOperationException("Failed to create shader program: Invalid vertex or fragment shader.");
		}

		return new DX11ShaderProgram((ID3D11VertexShader) vertexShader.NativeShader, (ID3D11PixelShader) fragmentShader.NativeShader, vertexShader.InputLayout)
		{
			VertSource = vertSource,
			FragSource = fragSource
		};
	}

	public unsafe INativeTexture2D CreateTexture2D(int width, int height, ReadOnlySpan<byte> pixelData)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(CreateTexture2D));

		var desc = new Texture2DDescription()
		{
			Width = (uint) width,
			Height = (uint) height,
			MipLevels = 1,
			ArraySize = 1,
			Format = Format.R8G8B8A8_UNorm, // RGBA 8bit Format
			SampleDescription = new SampleDescription(1, 0),
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.ShaderResource,
			CPUAccessFlags = CpuAccessFlags.None
		};

		fixed (byte* pData = pixelData)
		{
			var initData = new SubresourceData(pData, (uint)width * 4);
			var texture = _device!.CreateTexture2D(desc, new[] { initData });
			var srv = _device!.CreateShaderResourceView(texture);
			return new D3D11Texture2D(texture, srv, width, height);
		}
	}
	
	public unsafe INativeTexture2D CreateTexture2D(int width, int height, GpuFormat format, ReadOnlySpan<byte> pixelData)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(CreateTexture2D));

		var desc = new Texture2DDescription()
		{
			Width = (uint) width,
			Height = (uint) height,
			MipLevels = 1,
			ArraySize = 1,
			Format = MapFormat(format),
			SampleDescription = new SampleDescription(1, 0),
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.ShaderResource,
			CPUAccessFlags = CpuAccessFlags.None
		};

		fixed (byte* pData = pixelData)
		{
			var initData = new SubresourceData(pData, (uint)width * 4);
			var texture = _device!.CreateTexture2D(desc, new[] { initData });
			var srv = _device!.CreateShaderResourceView(texture);
			return new D3D11Texture2D(texture, srv, width, height);
		}
	}

	public INativeRenderTexture CreateRenderTexture(int width, int height)
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(CreateRenderTexture));

		var desc = new Texture2DDescription()
		{
			Width = (uint) width,
			Height = (uint) height,
			MipLevels = 1,
			ArraySize = 1,
			Format = Format.R8G8B8A8_UNorm, // RGBA 8bit Format
			SampleDescription = new SampleDescription(1, 0),
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
			CPUAccessFlags = CpuAccessFlags.None
		};

		var texture = _device!.CreateTexture2D(desc);
		var rtv = _device.CreateRenderTargetView(texture);
		var srv = _device.CreateShaderResourceView(texture);

		return new D3D11RenderTexture(texture, rtv, srv, width, height);
	}
	
	public void UpdateTexture2D(INativeTexture2D texture, ReadOnlySpan<byte> pixelData, int stride = 0)
	{
		if (texture is DX11NV12VideoTexture nv12)
		{
			UpdateNV12Texture(nv12, pixelData, stride);
		}
		else
		{
			uint rowPitch = stride > 0 ? (uint)stride : (uint)(texture.Width * 4);
    
			long requiredSize = rowPitch * texture.Height;

			if (pixelData.Length < requiredSize)
			{
#if DEBUG
				Console.WriteLine($"[Video Error] Buffer size mismatch! Have: {pixelData.Length}, Need: {requiredSize}");
#endif
				return;
			}

			unsafe
			{
				fixed (void* pData = pixelData)
				{
					if (texture is D3D11Texture2D d3dTex)
					{
						_context!.UpdateSubresource(d3dTex.Texture, 0u, null, (nint)pData, rowPitch, 0u);
					}
					else if (texture is D3D11RenderTexture d3dRT)
					{
						_context!.UpdateSubresource(d3dRT.Texture, 0u, null, (nint)pData, rowPitch, 0u);
					}
					else if (texture is D2DTexture d2dTex)
					{
						d2dTex.Bitmap.CopyFromMemory((nint)pData, rowPitch);
					}
				}
			}
		}
	}
	

	public INative2DRenderContext? Get2DContext()
	{
		MultiThreadService.ThrowIfNotRenderThread(nameof(Get2DContext));
		return _d2dImpl;
	}

	public void Dispose()
	{
		ReleaseResources();
		RUIService.Dispose();
		_wicFactory?.Dispose();
		_dwriteFactory?.Dispose();
		_d2dFactory?.Dispose();
		_d2dContext?.Dispose();
		_d2dImpl?.Dispose();
		_d2dDevice?.Dispose();
		foreach (var format in _textFormatCache.Values)
		{
			format.Dispose();
		}
		_textFormatCache.Clear();
		
		_swapChain?.Dispose();
		_context?.Dispose();
		_device?.Dispose();
		
		_rasterizerStateCullBack?.Dispose();
		_rasterizerStateCullFront?.Dispose();
		_rasterizerStateCullNone?.Dispose();
		
		_depthStateDefault?.Dispose();
		_depthStateReadOnly?.Dispose();
		_depthStateNone?.Dispose();
		
		DisposeNV12Pipeline();
		
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vortice.Direct3D11.Blend MapBlend(BlendFactor factor)
	{
		return factor switch
		{
			BlendFactor.Zero => Vortice.Direct3D11.Blend.Zero,
			BlendFactor.One => Vortice.Direct3D11.Blend.One,
			BlendFactor.SrcColor => Vortice.Direct3D11.Blend.SourceColor,
			BlendFactor.OneMinusSrcColor => Vortice.Direct3D11.Blend.InverseSourceColor,
			BlendFactor.SrcAlpha => Vortice.Direct3D11.Blend.SourceAlpha,
			BlendFactor.OneMinusSrcAlpha => Vortice.Direct3D11.Blend.InverseSourceAlpha,
			BlendFactor.DstAlpha => Vortice.Direct3D11.Blend.DestinationAlpha,
			BlendFactor.OneMinusDstAlpha => Vortice.Direct3D11.Blend.InverseDestinationAlpha,
			BlendFactor.DstColor => Vortice.Direct3D11.Blend.DestinationColor,
			BlendFactor.OneMinusDstColor => Vortice.Direct3D11.Blend.InverseDestinationColor,
			BlendFactor.SrcAlphaSaturate => Vortice.Direct3D11.Blend.SourceAlphaSaturate,
			BlendFactor.ConstantColor => Vortice.Direct3D11.Blend.BlendFactor,
			BlendFactor.OneMinusConstantColor => Vortice.Direct3D11.Blend.InverseBlendFactor,
			BlendFactor.OneMinusConstantAlpha => Vortice.Direct3D11.Blend.InverseBlendFactor,
			_ => Vortice.Direct3D11.Blend.One
		};
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vortice.Direct3D11.BlendOperation MapOp(BlendOperation op)
	{
		return op switch
		{
			BlendOperation.Add => Vortice.Direct3D11.BlendOperation.Add,
			BlendOperation.Subtract => Vortice.Direct3D11.BlendOperation.Subtract,
			BlendOperation.ReverseSubtract => Vortice.Direct3D11.BlendOperation.ReverseSubtract,
			BlendOperation.Min => Vortice.Direct3D11.BlendOperation.Min,
			BlendOperation.Max => Vortice.Direct3D11.BlendOperation.Max,
			_ => Vortice.Direct3D11.BlendOperation.Add
		};
	}
}