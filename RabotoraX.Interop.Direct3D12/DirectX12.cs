using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Threading;
using RabotoraX.Core.UI;
using RabotoraX.Interop.Direct3D11.Rendering;
using RabotoraX.Interop.Direct3D12.Rendering;
using TerraFX.Interop.Windows;
using Vortice;
using Vortice.D3DCompiler;
using Vortice.Direct3D12;
using Vortice.Direct3D12.Debug;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.Direct3D11;
using Vortice.Direct3D11on12;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using static TerraFX.Interop.Windows.Windows;
using IWICImagingFactory = Vortice.WIC.IWICImagingFactory;
using BlendOperation = RabotoraX.Core.Graphics.BlendOperation;
using BlendState = RabotoraX.Core.Graphics.BlendState;
using CullMode = RabotoraX.Core.Graphics.CullMode;
using InputElementDescription = RabotoraX.Core.Graphics.InputElementDescription;
using PrimitiveTopology = RabotoraX.Core.Graphics.PrimitiveTopology;
using Blend = Vortice.Direct3D12.Blend;
using BlendDescription = Vortice.Direct3D12.BlendDescription;
using ColorWriteEnable = Vortice.Direct3D12.ColorWriteEnable;
using ComparisonFunction = Vortice.Direct3D12.ComparisonFunction;
using FeatureLevel = Vortice.Direct3D.FeatureLevel;
using FillMode = Vortice.Direct3D12.FillMode;
using Filter = Vortice.Direct3D12.Filter;
using RasterizerDescription = Vortice.Direct3D12.RasterizerDescription;
using RenderTargetBlendDescription = Vortice.Direct3D12.RenderTargetBlendDescription;
using RenderTargetViewDescription = Vortice.Direct3D12.RenderTargetViewDescription;
using RenderTargetViewDimension = Vortice.Direct3D12.RenderTargetViewDimension;
using ResourceFlags = Vortice.Direct3D12.ResourceFlags;
using Texture2DRenderTargetView = Vortice.Direct3D12.Texture2DRenderTargetView;
using TextureAddressMode = Vortice.Direct3D12.TextureAddressMode;

namespace RabotoraX.Interop.Direct3D12;

[UsedImplicitly, SupportedOSPlatform("windows")]
[SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
[SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
public partial class DirectX12 : INativeGraphicsAPI
{
	private sealed partial class D2DContextImpl : INative2DRenderContext;
	
	public string ApiName => "Direct3D 12";
	public string DeviceName { get; private set; } = "Unknown";
	public bool IsInitialized { get; private set; }
	public (int Width, int Height) FramebufferSize { get; private set; }
	public Lock RenderLock { get; } = new();
	public bool IgnoreAllPresents { get; set; } = false;
	public DX12RenderTexture? MainRenderTexture { get; private set; }
	
	private readonly Stopwatch _waitTimer = Stopwatch.StartNew();
	private readonly Lock _fenceLock = new();

	internal ID3D12Device Device => _device!;
	internal Luid _deviceLuid;
	private ID3D12Device? _device;
	private ID3D12CommandQueue? _commandQueue;
	internal ID3D12GraphicsCommandList? CommandList { get; private set; }
	private ID3D12GraphicsCommandList? _copyCommandList;
	private ID3D12CommandAllocator? _commandAllocator, _copyAllocator;
	private IDXGISwapChain3? _swapChain;
	
	private readonly ID3D12Resource[] _backBuffers = new ID3D12Resource[3];
	private ID3D12DescriptorHeap? _rtvHeap, _dsvHeap;
	private int _rtvDescriptorSize;
	private int _frameIndex;

	private ID3D12Resource? _depthBuffer;
	private ID3D12RootSignature? _globalRootSignature;
	
	private readonly AutoResetEvent _fenceEvent = new(false);
	private ID3D12Fence? _fence;
	private ulong _fenceValue;
	private nint _frameWaitableObject;
	
	internal INativeRenderTexture? _currentRenderTarget;
	// internal INativeShader? _currentShader;
	internal DX12ShaderProgram? _currentShaderProgram;
	internal DX12Shader? _currentVS, _currentPS;
	internal DX12Buffer? _currentVBuffer, _currentIBuffer;
	internal int _currentVStride, _currentVOffset;
	internal PrimitiveTopology _currentTopology = PrimitiveTopology.TriangleList;
	
	private readonly Stack<CullMode> _cullModeStack = new([CullMode.Back]);
	private readonly Stack<BlendState> _blendStateStack = new([BlendState.AlphaBlend]);
	private bool _depthEnabled = true;
	private volatile bool _isRecording;

	private readonly Dictionary<string, ID3D12PipelineState> _psoCache = [];
	
	
	private ID3D11Device5? _d3d11Device;
	internal ID3D11DeviceContext4? _d3d11Context;
	internal ID3D11On12Device2? _d3d11On12Device;
	private ID2D1Factory8? _d2dFactory;
	protected ID2D1Device7? _d2dDevice;
	internal ID2D1DeviceContext7? _d2dContext;
	internal IDWriteFactory8? _dwriteFactory;
	internal IWICImagingFactory? _wicFactory;
	
	internal ID3D11Resource? _wrappedBackBuffer;
	internal ID2D1Bitmap1? _d2dTargetBitmap;
	private D2DContextImpl? _d2dImpl;
	
	private int _pendingWidth, _pendingHeight;
	private bool _resizePending;
	
	public void Initialize(INativeWindow window)
	{
		if (IsInitialized) throw new InvalidOperationException("Graphics API is already initialized.");

		FramebufferSize = window.Size;
		CreateD3D12(window);
		CreateResources(window.Size.Width, window.Size.Height);
		CreateD2D11On12();
		
#if DEBUG
		Console.WriteLine($"Initialized Graphics API Backend as {ApiName} on device {DeviceName}");
#endif
		IsInitialized = true;
	}

	private void CreateD3D12(INativeWindow window)
	{
		CreateDevice();
		CreateCommandQueue();
		CreateSwapChain(window);
		CreateDescriptorHeaps();
		CreateRootSignature();
	}

	private void CreateDevice()
	{
#if DEBUG
		if (D3D12.D3D12GetDebugInterface(out ID3D12Debug? debug).Success)
		{
			debug!.EnableDebugLayer();
			debug.Dispose();
		}
#endif
		D3D12.D3D12CreateDevice(null, FeatureLevel.Level_11_0, out _device).CheckError();
		_deviceLuid = (Luid)_device!.AdapterLuid;
		DeviceName = "Direct3D 12 Hardware Adapter";
	}

	private void CreateCommandQueue()
	{
		var queueDesc = new CommandQueueDescription(CommandListType.Direct);
		_commandQueue = _device!.CreateCommandQueue(queueDesc);
		_commandAllocator = _device.CreateCommandAllocator(CommandListType.Direct);
		CommandList = _device.CreateCommandList<ID3D12GraphicsCommandList>(0, CommandListType.Direct, _commandAllocator);
		CommandList.Close(); // Close before start; BeginFrame will reset this

		_copyAllocator = _device.CreateCommandAllocator(CommandListType.Direct);
		_copyCommandList = _device.CreateCommandList<ID3D12GraphicsCommandList>(0, CommandListType.Direct, _copyAllocator);
		_copyCommandList.Close();

		_fence = _device.CreateFence();
		_fenceValue = 1;
	}

	private void CreateSwapChain(INativeWindow window)
	{
		IDXGIFactory4 factory;
		using (var adapter = GetAdapter(DXGI.CreateDXGIFactory1<IDXGIFactory1>()))
		{
			factory = adapter.GetParent<IDXGIFactory4>();
		}
		var swapChainDesc = new SwapChainDescription1()
		{
			BufferCount = 3,
			Width = (uint) window.Size.Width,
			Height = (uint) window.Size.Height,
			Format = Format.B8G8R8A8_UNorm,
			AlphaMode = AlphaMode.Ignore,
			Scaling = Scaling.None,
			BufferUsage = Usage.RenderTargetOutput,
			SwapEffect = SwapEffect.FlipDiscard,
			SampleDescription = new SampleDescription(1, 0),
			Flags = SwapChainFlags.FrameLatencyWaitableObject | SwapChainFlags.AllowTearing
		};
		
		using (var swapChain = factory.CreateSwapChainForHwnd(_commandQueue!, window.Handle, swapChainDesc))
		{
			_swapChain = swapChain.QueryInterface<IDXGISwapChain3>();
		}
		factory.MakeWindowAssociation(window.Handle, WindowAssociationFlags.IgnoreAltEnter);
		factory.Dispose();
		_swapChain.MaximumFrameLatency = 1;
		_frameWaitableObject = _swapChain.FrameLatencyWaitableObject;
		_frameIndex = (int)_swapChain.CurrentBackBufferIndex;
	}

	private void CreateDescriptorHeaps()
	{
		_rtvHeap = _device!.CreateDescriptorHeap(new DescriptorHeapDescription(DescriptorHeapType.RenderTargetView, 10));
		_dsvHeap = _device.CreateDescriptorHeap(new DescriptorHeapDescription(DescriptorHeapType.DepthStencilView, 10));
		_rtvDescriptorSize = (int)_device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);
	}

	private void CreateRootSignature()
	{
		var @params = new[]
		{
			new RootParameter(RootParameterType.ConstantBufferView, new RootDescriptor(0, 0), ShaderVisibility.All),
			new RootParameter(new RootDescriptorTable(new DescriptorRange(DescriptorRangeType.ShaderResourceView, 4, 0)), ShaderVisibility.Pixel)
		};

		var samplers = new[]
		{
			new StaticSamplerDescription(ShaderVisibility.All, 0, 0)
			{
				Filter = Filter.MinMagMipPoint,
				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
				ComparisonFunction = ComparisonFunction.Never,
				MaxLOD = float.MaxValue,
				MinLOD = 0,
				MipLODBias = 0f
			}
		};
		
		var desc = new RootSignatureDescription(RootSignatureFlags.AllowInputAssemblerInputLayout, @params, samplers);
		_ = D3D12.D3D12SerializeVersionedRootSignature(new VersionedRootSignatureDescription(desc), out var blob);
		_globalRootSignature = _device!.CreateRootSignature(blob);
	}
	
	private void CreateResources(int width, int height)
	{
		var rtvHandleStart = _rtvHeap!.GetCPUDescriptorHandleForHeapStart();
    
		var rtvDesc = new RenderTargetViewDescription()
		{
			Format = Format.B8G8R8A8_UNorm,
			ViewDimension = RenderTargetViewDimension.Texture2D,
			Texture2D = new Texture2DRenderTargetView() { MipSlice = 0, PlaneSlice = 0 }
		};
		for (uint i = 0; i < 3; i++)
		{
			_backBuffers[i] = _swapChain!.GetBuffer<ID3D12Resource>(i);
			_device!.CreateRenderTargetView(_backBuffers[i], rtvDesc, rtvHandleStart + ((int)i * _rtvDescriptorSize));
		}

		MainRenderTexture = (DX12RenderTexture)CreateRenderTexture(width, height);
		MainRenderTexture.RtvHandle = rtvHandleStart + (3 * _rtvDescriptorSize);
    
		_device!.CreateRenderTargetView(MainRenderTexture.Resource, rtvDesc, MainRenderTexture.RtvHandle);
		_currentRenderTarget = MainRenderTexture;
		
		var depthDesc = ResourceDescription.Texture2D(Format.D24_UNorm_S8_UInt, (uint)width, (uint)height, 1, 1, 1, 0, ResourceFlags.AllowDepthStencil);
		var clearValue = new ClearValue(Format.D24_UNorm_S8_UInt, new DepthStencilValue(1.0f));
		_depthBuffer = _device!.CreateCommittedResource(new HeapProperties(HeapType.Default), HeapFlags.None, depthDesc, ResourceStates.DepthWrite, clearValue);
		_device.CreateDepthStencilView(_depthBuffer, null, _dsvHeap!.GetCPUDescriptorHandleForHeapStart());
		if (_d2dContext != null)
		{
			BindD2DTarget();
		}
	}

	private void CreateD2D11On12()
	{
		Apis.D3D11On12CreateDevice(_device!, DeviceCreationFlags.BgraSupport, [FeatureLevel.Level_11_0], [_commandQueue!], 0,
			out var tempD3D11Device, out var tempD3D11Context, out _).CheckError();
		
		_d3d11Device = tempD3D11Device.QueryInterface<ID3D11Device5>();
		_d3d11Context = tempD3D11Context.QueryInterface<ID3D11DeviceContext4>();
		_d3d11On12Device = _d3d11Device.QueryInterface<ID3D11On12Device2>();
		
		tempD3D11Device.Dispose();
		tempD3D11Context.Dispose();

		_d2dFactory = D2D1.D2D1CreateFactory<ID2D1Factory8>();
		_dwriteFactory = DWrite.DWriteCreateFactory<IDWriteFactory8>();
		RUIService.Initialize(this); // Initialize text factory for RUI rendering
		_wicFactory = new IWICImagingFactory();

		using var dxgiDevice = _d3d11Device!.QueryInterface<IDXGIDevice>();
		_d2dDevice = _d2dFactory.CreateDevice(dxgiDevice);
		_d2dContext = _d2dDevice.CreateDeviceContext(DeviceContextOptions.None);
		_d2dContext.UnitMode = UnitMode.Pixels;
		_d2dContext.SetDpi(96, 96);
		
		BindD2DTarget();
		
		_d2dImpl = new D2DContextImpl(this);
	}

	private void BindD2DTarget()
	{
		var d3d11Flags = new Vortice.Direct3D11on12.ResourceFlags() { BindFlags = BindFlags.RenderTarget };
		_d3d11On12Device!.CreateWrappedResource(MainRenderTexture!.Resource, d3d11Flags, ResourceStates.RenderTarget,
			ResourceStates.RenderTarget, out _wrappedBackBuffer).CheckError();

		using var surface = _wrappedBackBuffer!.QueryInterface<IDXGISurface>();
		var props = new BitmapProperties1(new Vortice.DCommon.PixelFormat(Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied), 96, 96, BitmapOptions.Target | BitmapOptions.CannotDraw);
		_d2dTargetBitmap = _d2dContext!.CreateBitmapFromDxgiSurface(surface, props);
		_d2dContext.Target = _d2dTargetBitmap;
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
		if (width <= 0 || height <= 0) return; 

		WaitIdle();

		_d2dContext!.Target = null;
		_d2dTargetBitmap?.Dispose();
		_wrappedBackBuffer?.Dispose();
		_d3d11Context!.Flush();

		FramebufferSize = (width, height);

		foreach (var buffer in _backBuffers)
		{
			buffer.Dispose();
		}
		MainRenderTexture?.Dispose();
		_depthBuffer?.Dispose();
		
		var hr = _swapChain!.ResizeBuffers(3, (uint)width, (uint)height, Format.Unknown,
			SwapChainFlags.FrameLatencyWaitableObject | SwapChainFlags.AllowTearing);

		if (hr.Failure)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			_swapChain.ResizeBuffers(3, (uint)width, (uint)height, Format.Unknown,
				SwapChainFlags.FrameLatencyWaitableObject | SwapChainFlags.AllowTearing).CheckError();
		}

		// Regain Frame Latency Waitable Object after DXGI reset
		_frameWaitableObject = _swapChain.FrameLatencyWaitableObject;
		_frameIndex = (int)_swapChain.CurrentBackBufferIndex;
		
		CreateResources(width, height);
	}

	public void BeginFrame()
	{
		if (_isRecording)
		{
			CommandList!.Close();
			_isRecording = false;
		}
		_commandAllocator!.Reset();
		CommandList!.Reset(_commandAllocator, null);
		_isRecording = true;
		CommandList.SetGraphicsRootSignature(_globalRootSignature);

		ApplySetRenderTarget(null); // default to bind MainRenderTexture
		ApplySetViewport(0, 0, FramebufferSize.Width, FramebufferSize.Height);
	}

	public void EndFrame()
	{
		// leave blank as we don't need to implement this in DirectX 12
	}

	public void Present(bool vsync = true)
	{
		if (IgnoreAllPresents)
		{
			if (_isRecording)
			{
				CommandList!.Close();
				_isRecording = false;
			}
			return;
		}

		if (_isRecording)
		{
			CommandList!.Close();
			_commandQueue!.ExecuteCommandList(CommandList);
			_isRecording = false;
		}
		
		var backBuffer = _backBuffers[_frameIndex];
		
		_copyAllocator!.Reset();
		_copyCommandList!.Reset(_copyAllocator, null);
		
		_copyCommandList!.ResourceBarrierTransition(MainRenderTexture!.Resource, ResourceStates.RenderTarget, ResourceStates.CopySource);
		_copyCommandList.ResourceBarrierTransition(backBuffer, ResourceStates.Present, ResourceStates.CopyDest);
		
		_copyCommandList.CopyResource(backBuffer, MainRenderTexture.Resource);
		
		_copyCommandList.ResourceBarrierTransition(backBuffer, ResourceStates.CopyDest, ResourceStates.Present);
		_copyCommandList.ResourceBarrierTransition(MainRenderTexture.Resource, ResourceStates.CopySource, ResourceStates.RenderTarget);
		
		_copyCommandList.Close();
		_commandQueue!.ExecuteCommandList(_copyCommandList);

		_swapChain!.Present(vsync ? 1u : 0u, PresentFlags.None); // we don't CheckError here because some drivers may return an error code when the window is minimized, which we want to ignore
		
		FlushGpuCommandQueue();
		
		_frameIndex = (int)_swapChain.CurrentBackBufferIndex;
	}

	public void WaitIdle()
	{
		FlushGpuCommandQueue();
	}

	public INativeCommandList CreateCommandList()
	{
		return new D3D12CommandList(this);
	}

	public void Submit(INativeCommandList commandList)
	{
		if (commandList is D3D12CommandList dx12List)
		{
			dx12List.Execute();
		}

		if (_isRecording)
		{
			CommandList!.Close();
			_commandQueue!.ExecuteCommandList(CommandList);
			_isRecording = false; // Mark current command list as not recording since it's already executed
		}
	}
	
	private void FlushGpuCommandQueue()
	{
		lock (_fenceLock)
		{
			ulong fenceValueToWaitFor = _fenceValue;
            _commandQueue!.Signal(_fence!, fenceValueToWaitFor);
            _fenceValue++;
            
            if (_fence!.CompletedValue < fenceValueToWaitFor)
            {
            	_fence.SetEventOnCompletion(fenceValueToWaitFor, _fenceEvent);
            	_fenceEvent.WaitOne();
            }
		}
	}
	
	public IGpuBuffer CreateBuffer(BufferType type, int sizeInBytes)
	{
		var resource = _device!.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, ResourceDescription.Buffer((ulong)sizeInBytes), ResourceStates.GenericRead);
		return new DX12Buffer(resource, sizeInBytes);
	}

	public IGpuBuffer CreateBuffer<T>(BufferType type, T[] data) where T : unmanaged
	{
		int size = data.Length * Unsafe.SizeOf<T>();
		// 使用 Upload Heap，方便 CPU 频繁更新（映射 DX11 的 Dynamic Buffer）
		var resource = _device!.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, ResourceDescription.Buffer((ulong)size), ResourceStates.GenericRead);
		var buffer = new DX12Buffer(resource, size);
		UpdateBuffer(buffer, data);
		return buffer;
	}
	
	public unsafe void UpdateBuffer<T>(IGpuBuffer buffer, T[] data) where T : unmanaged
	{
		if (buffer is DX12Buffer dxBuf)
		{
			void* pMappedData = null;
			var res = dxBuf.Resource.Map(0, null, &pMappedData);
			if (res.Success && pMappedData != null)
			{
				fixed (void* pData = data)
				{
					Unsafe.CopyBlock(pMappedData, pData, (uint)(data.Length * Unsafe.SizeOf<T>()));
				}
				dxBuf.Resource.Unmap(0);
			}
		}
	}

	public unsafe void UpdateBuffer<T>(IGpuBuffer buffer, ref T data) where T : unmanaged
	{
		if (buffer is DX12Buffer dxBuf)
		{
			void* pMappedData = null;
			var res = dxBuf.Resource.Map(0, null, &pMappedData);
			if (res.Success && pMappedData != null)
			{
				fixed (void* pData = &data)
				{
					Unsafe.CopyBlock(pMappedData, pData, (uint)Unsafe.SizeOf<T>());
				}
				dxBuf.Resource.Unmap(0);
			}
		}
	}
	
	public INativeTexture2D CreateTexture2D(int width, int height, ReadOnlySpan<byte> pixelData)
	{
		if (width <= 0) width = 1;
		if (height <= 0) height = 1;

		var desc = ResourceDescription.Texture2D(Format.B8G8R8A8_UNorm, (uint)width, (uint)height);
    
		var initialState = pixelData.IsEmpty ? ResourceStates.PixelShaderResource : ResourceStates.CopyDest;
    
		var resource = _device!.CreateCommittedResource(new HeapProperties(HeapType.Default), HeapFlags.None, desc, initialState);
		var tex = new DX12Texture2D(resource, width, height);
    
		if (!pixelData.IsEmpty)
		{
			UpdateTexture2D(tex, pixelData, width * 4);
		}
    
		return tex;
	}
	
	public unsafe void UpdateTexture2D(INativeTexture2D texture, ReadOnlySpan<byte> pixelData, int stride = 0)
	{
		if (texture is D2DTexture d2dTex)
		{
			int rowPitch = stride > 0 ? stride : d2dTex.Width * 4;
			fixed (void* pData = pixelData)
			{
				d2dTex.Bitmap.CopyFromMemory((nint)pData, (uint)rowPitch);
			}
		}
		else if (texture is DX12Texture2D dxTex)
		{
			int rowPitch = stride > 0 ? stride : dxTex.Width * 4;
			int alignedRowPitch = (rowPitch + 255) & ~255;
			int slicePitch = alignedRowPitch * dxTex.Height;

			if (slicePitch <= 0) return;

			var uploadResource = _device!.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, ResourceDescription.Buffer((ulong) slicePitch), ResourceStates.GenericRead);

			void* pMappedData = null;
			var res = uploadResource.Map(0, null, &pMappedData);
			if (res.Success && pMappedData != null)
			{
				byte* pDstBase = (byte*) pMappedData;
				fixed (byte* pSrc = pixelData)
				{
					for (int y = 0; y < dxTex.Height; y++)
					{
						byte* pDstRow = pDstBase + y * alignedRowPitch;
						void* pSrcRow = pSrc + y * rowPitch;

						Unsafe.CopyBlock(pDstRow, pSrcRow, (uint) rowPitch);
						Unsafe.InitBlock(pDstRow + rowPitch, 0, (uint) (alignedRowPitch - rowPitch));
					}
				}
			}

			uploadResource.Unmap(0);

			var cmdAlloc = _device.CreateCommandAllocator(CommandListType.Direct);
			var cmdList = _device.CreateCommandList<ID3D12GraphicsCommandList>(0, CommandListType.Direct, cmdAlloc);

			var dstLoc = new TextureCopyLocation(dxTex.Resource);
			var srcLoc = new TextureCopyLocation(uploadResource, new PlacedSubresourceFootPrint()
			{
				Offset = 0,
				Footprint = new SubresourceFootPrint()
				{
					Format = Format.B8G8R8A8_UNorm,
					Width = (uint) dxTex.Width,
					Height = (uint) dxTex.Height,
					Depth = 1,
					RowPitch = (uint) alignedRowPitch
				}
			});

			cmdList.CopyTextureRegion(dstLoc, 0, 0, 0, srcLoc);
			cmdList.ResourceBarrierTransition(dxTex.Resource, ResourceStates.CopyDest, ResourceStates.PixelShaderResource);
			cmdList.Close();

			_commandQueue!.ExecuteCommandList(cmdList);
			WaitIdle();

			cmdList.Dispose();
			cmdAlloc.Dispose();
			uploadResource.Dispose();
		}
	}
	
	public INativeRenderTexture CreateRenderTexture(int width, int height)
	{
		var desc = ResourceDescription.Texture2D(Format.B8G8R8A8_UNorm, (uint)width, (uint)height, 1, 1, 1, 0, ResourceFlags.AllowRenderTarget);
		var clearVal = new ClearValue(Format.B8G8R8A8_UNorm, new Color4(0, 0, 0));
		var resource = _device!.CreateCommittedResource(new HeapProperties(HeapType.Default), HeapFlags.None, desc, ResourceStates.RenderTarget, clearVal);
		return new DX12RenderTexture(resource, width, height);
	}
	
	public INativeShader CreateNativeShader(ShaderType type, string sourceCode, string entryPoint = "main", InputElementDescription[]? inputLayout = null)
	{
		string profile = type == ShaderType.VertexShader ? "vs_5_0" : "ps_5_0";
		var result = Compiler.Compile(sourceCode, entryPoint, "Source", profile, out var shaderBlob, out var errorBlob);

		if (result.Failure)
		{
			throw new InvalidOperationException($"Shader compile error: {errorBlob?.AsString()}");
		}

		var byteCode = shaderBlob.AsBytes();
		shaderBlob?.Dispose();
		errorBlob?.Dispose();

		Vortice.Direct3D12.InputElementDescription[]? layout = null;
		if (inputLayout != null)
		{
			layout = new Vortice.Direct3D12.InputElementDescription[inputLayout.Length];
			for (int i = 0; i < inputLayout.Length; i++)
			{
				var item = inputLayout[i];
				layout[i] = new Vortice.Direct3D12.InputElementDescription(
					item.SemanticName,
					(uint)item.SemanticIndex, GetDxgiFormat(item.Format), (uint)item.AlignedByteOffset, 0);
			}
		}

		return new DX12Shader(byteCode, type, layout)
		{
			VertSource = type is ShaderType.VertexShader or ShaderType.VertexFragment ? sourceCode : null,
			FragSource = type is ShaderType.FragmentShader or ShaderType.VertexFragment ? sourceCode : null
		};
	}

	public INativeShader CreateNativeShaderProgram(string vertSource, string fragSource, InputElementDescription[] inputLayout, string vertEntryPoint = "main", string fragEntryPoint = "main")
	{
		var vs = (DX12Shader)CreateNativeShader(ShaderType.VertexShader, vertSource, vertEntryPoint, inputLayout);
		var ps = (DX12Shader)CreateNativeShader(ShaderType.FragmentShader, fragSource, fragEntryPoint);

		return new DX12ShaderProgram(vs.ByteCode, ps.ByteCode, vs.InputElements!)
		{
			VertSource = vertSource,
			FragSource = fragSource
		};
	}

	internal void ApplySetRenderTarget(INativeRenderTexture? rt)
	{
		var target = (rt as DX12RenderTexture) ?? MainRenderTexture;
		if (target == null) return;

		_currentRenderTarget = target;

		var rtvHandle = target.RtvHandle;
		var dsvHandle = _dsvHeap!.GetCPUDescriptorHandleForHeapStart();

		CommandList!.OMSetRenderTargets(rtvHandle, dsvHandle);
	}

	internal void ApplyClear(float r, float g, float b, float a)
	{
		if (_currentRenderTarget is not DX12RenderTexture target) return;

		CommandList!.ClearRenderTargetView(target.RtvHandle, new Color4(r, g, b, a));

		if (_depthEnabled)
		{
			CommandList.ClearDepthStencilView(_dsvHeap!.GetCPUDescriptorHandleForHeapStart(), ClearFlags.Depth | ClearFlags.Stencil, 1.0f, 0);
		}
	}
	
	internal void ApplySetViewport(float x, float y, float w, float h, float minDepth = 0, float maxDepth = 1)
	{
		CommandList!.RSSetViewport(new Viewport(x, y, w, h, minDepth, maxDepth));
		CommandList.RSSetScissorRect(new RawRect((int)x, (int)y, (int)(x + w), (int)(y + h)));
	}

	internal void ApplyDraw(int vertCount, int startVertLoc, PrimitiveTopology topology)
	{
		PrepareDrawState(topology);
		CommandList!.DrawInstanced((uint)vertCount, 1, (uint)startVertLoc, 0);
	}

	internal void ApplyDrawIndexed(int indexCount, int startIndexLoc, int baseVertLoc, PrimitiveTopology topology)
	{
		PrepareDrawState(topology);
		CommandList!.DrawIndexedInstanced((uint) indexCount, 1, (uint) startIndexLoc, baseVertLoc, 0);
	}
	
	internal void ApplySetConstantBuffer(int slot, IGpuBuffer buffer, ShaderType stage)
	{
		if (slot == 0 && buffer is DX12Buffer dxBuf)
		{
			CommandList!.SetGraphicsRootConstantBufferView(0, dxBuf.Resource.GPUVirtualAddress);
		}
	}

	internal void ApplySetCullMode(CullMode mode)
	{
		_cullModeStack.Push(mode);
	}

	internal void ResumeCullMode()
	{
		if (_cullModeStack.Count > 1) _cullModeStack.Pop();
	}

	internal void ApplySetBlendState(BlendState state)
	{
		_blendStateStack.Push(state);
	}
	
	internal void ResumeBlendState()
	{
		if (_blendStateStack.Count > 1) _blendStateStack.Pop();
	}
	
	internal void ApplySetDepthEnabled(bool enabled, bool writeEnabled = true)
	{
		_depthEnabled = enabled;
	}

	internal void ApplySetShader(INativeShader shader)
	{
		if (shader is DX12ShaderProgram program)
		{
			_currentShaderProgram = program;
			_currentVS = null;
			_currentPS = null;
		}
		else if (shader is DX12Shader dxShader)
		{
			_currentShaderProgram = null;
			if (dxShader.Type == ShaderType.VertexShader)
			{
				_currentVS = dxShader;
			}
			else if (dxShader.Type == ShaderType.FragmentShader)
			{
				_currentPS = dxShader;
			}
			else
			{
				throw new InvalidOperationException("Unsupported shader type.");
			}
		}
	}

	private void PrepareDrawState(PrimitiveTopology topology)
	{
		CommandList!.IASetPrimitiveTopology(topology switch
		{
			PrimitiveTopology.TriangleList => Vortice.Direct3D.PrimitiveTopology.TriangleList,
			PrimitiveTopology.LineList => Vortice.Direct3D.PrimitiveTopology.LineList,
			_ => Vortice.Direct3D.PrimitiveTopology.TriangleList
		});

		if (_currentVBuffer != null)
		{
			CommandList.IASetVertexBuffers(0, new VertexBufferView(_currentVBuffer.Resource.GPUVirtualAddress, (uint)_currentVBuffer.SizeInBytes, (uint)_currentVStride));
		}
		if (_currentIBuffer != null)
		{
			CommandList.IASetIndexBuffer(new IndexBufferView(_currentIBuffer.Resource.GPUVirtualAddress, (uint)_currentIBuffer.SizeInBytes, Format.R32_UInt)); // Assuming both R32
		}

		if (_currentShaderProgram != null)
		{
			string hash = $"{_currentShaderProgram.GetHashCode()}_{_blendStateStack.Peek().GetHashCode()}_{_cullModeStack.Peek()}_{_depthEnabled}_{topology}";
			if (!_psoCache.TryGetValue(hash, out var pso))
			{
				pso = CreatePipelineState(_currentShaderProgram.VSByteCode, _currentShaderProgram.PSByteCode, _currentShaderProgram.InputElements, topology);
				_psoCache[hash] = pso;
			}
			CommandList.SetPipelineState(pso);
		}
		else if (_currentVS != null && _currentPS != null)
		{
			string hash = $"{_currentVS.GetHashCode()}_{_currentPS.GetHashCode()}_{_blendStateStack.Peek().GetHashCode()}_{_cullModeStack.Peek()}_{_depthEnabled}_{topology}";
			if (!_psoCache.TryGetValue(hash, out var pso))
			{
				pso = CreatePipelineState(_currentVS.ByteCode, _currentPS.ByteCode, _currentVS.InputElements, topology);
				_psoCache[hash] = pso;
			}
			CommandList.SetPipelineState(pso);
		}
	}

	private ID3D12PipelineState CreatePipelineState(byte[] vsByteCode, byte[] psByteCode, Vortice.Direct3D12.InputElementDescription[]? inputElements, PrimitiveTopology topology)
	{
		var blend = _blendStateStack.Peek();
		var cull = _cullModeStack.Peek();

		var desc = new GraphicsPipelineStateDescription()
		{
			RootSignature = _globalRootSignature,
			VertexShader = vsByteCode,
			PixelShader = psByteCode,
			InputLayout = inputElements != null ? new InputLayoutDescription(inputElements) : new InputLayoutDescription(),
			RasterizerState = new RasterizerDescription(cull switch
			{
				CullMode.Back => Vortice.Direct3D12.CullMode.Back,
				CullMode.Front => Vortice.Direct3D12.CullMode.Front,
				_ => Vortice.Direct3D12.CullMode.None
			}, FillMode.Solid),
			BlendState = MapBlendState(blend),
			PrimitiveTopologyType = topology == PrimitiveTopology.LineList ? PrimitiveTopologyType.Line : PrimitiveTopologyType.Triangle,
			RenderTargetFormats = [Format.B8G8R8A8_UNorm],
			DepthStencilFormat = Format.D24_UNorm_S8_UInt,
			SampleDescription = new SampleDescription(1, 0)
		};
		return _device!.CreateGraphicsPipelineState(desc);
	}

	public void WaitNextFrameReady()
	{
		if (IgnoreAllPresents || _frameWaitableObject == nint.Zero)
		{
			Thread.Yield();
			return;
		}
		WaitForSingleObject((HANDLE)_frameWaitableObject, 1000);
	}

	public IEnumerable<ShaderPlatform> GetSupportedShaderPlatforms()
	{
		return [ShaderPlatform.HLSL12];
	}

	public INative2DRenderContext? Get2DContext()
	{
		return _d2dImpl;
	}

	public void Dispose()
	{
		WaitIdle();
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
		
		_commandQueue?.Dispose();
		_commandAllocator?.Dispose();
		CommandList?.Dispose();
		_copyCommandList?.Dispose();
		_copyAllocator?.Dispose();
		_swapChain?.Dispose();
		_rtvHeap?.Dispose();
		_dsvHeap?.Dispose();
		_depthBuffer?.Dispose();
		_device?.Dispose();

		IsInitialized = false;
		GC.SuppressFinalize(this);
	}

	private IDXGIAdapter1 GetAdapter(IDXGIFactory1 factory1)
	{
		for (uint i = 0; factory1.EnumAdapters1(i, out var adapter).Success; i++)
		{
			var desc = adapter.Description1;
			if (desc.Luid == _deviceLuid)
			{
				return adapter;
			}
			adapter.Dispose();
		}
		throw new InvalidOperationException("Cannot find a matching DXGI adapter.");
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Format GetDxgiFormat(GpuFormat format)
	{
		return format switch
		{
			GpuFormat.R32G32B32_Float => Format.R32G32B32_Float,
			GpuFormat.R32G32_Float => Format.R32G32_Float,
			GpuFormat.R32G32B32A32_Float => Format.R32G32B32A32_Float,
			GpuFormat.R16G16B16A16_Float => Format.R16G16B16A16_Float,
			GpuFormat.R8G8B8A8_UNorm => Format.R8G8B8A8_UNorm,
			GpuFormat.B8G8R8A8_UNorm => Format.B8G8R8A8_UNorm,
			_ => throw new NotSupportedException($"Unsupported format: {format}")
		};
	}
	
	private static BlendDescription MapBlendState(BlendState state)
	{
		if (!state.EnableBlending) return BlendDescription.Opaque;

		var desc = new BlendDescription { AlphaToCoverageEnable = false, IndependentBlendEnable = false };
		desc.RenderTarget[0] = new RenderTargetBlendDescription
		{
			BlendEnable = true,
			SourceBlend = MapBlend(state.SrcColor),
			DestinationBlend = MapBlend(state.DstColor),
			BlendOperation = MapOp(state.ColorOp),
			SourceBlendAlpha = MapBlend(state.SrcAlpha),
			DestinationBlendAlpha = MapBlend(state.DstAlpha),
			BlendOperationAlpha = MapOp(state.AlphaOp),
			RenderTargetWriteMask = ColorWriteEnable.All
		};
		return desc;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Blend MapBlend(BlendFactor factor)
	{
		return factor switch
		{
			BlendFactor.Zero => Blend.Zero,
			BlendFactor.One => Blend.One,
			BlendFactor.SrcColor => Blend.SourceColor,
			BlendFactor.OneMinusSrcColor => Blend.InverseSourceColor,
			BlendFactor.SrcAlpha => Blend.SourceAlpha,
			BlendFactor.OneMinusSrcAlpha => Blend.InverseSourceAlpha,
			BlendFactor.DstAlpha => Blend.DestinationAlpha,
			BlendFactor.OneMinusDstAlpha => Blend.InverseDestinationAlpha,
			BlendFactor.DstColor => Blend.DestinationColor,
			BlendFactor.OneMinusDstColor => Blend.InverseDestinationColor,
			BlendFactor.SrcAlphaSaturate => Blend.SourceAlphaSaturate,
			BlendFactor.ConstantColor => Blend.BlendFactor,
			BlendFactor.OneMinusConstantColor => Blend.InverseBlendFactor,
			BlendFactor.OneMinusConstantAlpha => Blend.InverseBlendFactor,
			_ => Blend.One
		};
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vortice.Direct3D12.BlendOperation MapOp(BlendOperation op)
	{
		return op switch
		{
			BlendOperation.Add => Vortice.Direct3D12.BlendOperation.Add,
			BlendOperation.Subtract => Vortice.Direct3D12.BlendOperation.Subtract,
			BlendOperation.ReverseSubtract => Vortice.Direct3D12.BlendOperation.RevSubtract,
			BlendOperation.Min => Vortice.Direct3D12.BlendOperation.Min,
			BlendOperation.Max => Vortice.Direct3D12.BlendOperation.Max,
			_ => Vortice.Direct3D12.BlendOperation.Add
		};
	}
}