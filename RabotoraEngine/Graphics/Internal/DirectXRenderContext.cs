using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RabotoraEngine.Internal.Utility.Resources;
using Vortice.D3DCompiler;
using Vortice.Direct2D1;
using Vortice.Direct3D11;
using Vortice.DirectWrite;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.WIC;
using AlphaMode = Vortice.DCommon.AlphaMode;
using Blend = Vortice.Direct3D11.Blend;
using BlendDescription = Vortice.Direct3D11.BlendDescription;
using BlendOperation = Vortice.Direct3D11.BlendOperation;
using FactoryType = Vortice.Direct2D1.FactoryType;
using Filter = Vortice.Direct3D11.Filter;
using InputElementDescription = Vortice.Direct3D11.InputElementDescription;
using PixelFormat = Vortice.DCommon.PixelFormat;

namespace RabotoraEngine.Graphics.Internal;

public class DirectXRenderContext : IRenderContext // IRenderContext already implements IDisposable, so we don't need to add it again
{
	// simple quad index data (two tris)
	private readonly static ushort[] QuadIndices = [0, 1, 2, 0, 2, 3];
	
	private readonly RWindow _window;

	private ID2D1Factory1? _d2dFactory;
	private IDWriteFactory? _dwriteFactory;
	private IWICImagingFactory? _wicFactory;
	private ID2D1Device? _d2dDevice;
	private ID2D1DeviceContext? _d2dContext;
	private ID2D1Bitmap1? _d2dRenderTarget;

	private ID3D11RenderTargetView? _currentRenderTarget;
	private ID3D11VertexShader? _vertShader;
	private ID3D11PixelShader? _fragShader;
	private ID3D11InputLayout? _inputLayout;
	private ID3D11Buffer? _vertBuffer, _indexBuffer;
	private ID3D11SamplerState? _sampler;
	private ID3D11BlendState? _blendState;

	private bool _disposed;

	[StructLayout(LayoutKind.Sequential)]
	private struct Vertex
	{
		public Vector3 Position;
		public Vector2 TexCoord;
		public Vector4 Color;
	}

	public DirectXRenderContext(RWindow parent)
	{
		_window = parent ?? throw new ArgumentNullException(nameof(parent), "Parent window cannot be null.");
		Initialize();
	}

	private void Initialize()
	{
		var device = _window.Device;
		_d2dFactory = D2D1.D2D1CreateFactory<ID2D1Factory1>(FactoryType.MultiThreaded);
		_dwriteFactory = DWrite.DWriteCreateFactory<IDWriteFactory>();
		_wicFactory = new IWICImagingFactory();
		
		using var dxgiDevice = device.QueryInterface<IDXGIDevice>();
		_d2dDevice = _d2dFactory!.CreateDevice(dxgiDevice);
		_d2dContext = _d2dDevice.CreateDeviceContext(DeviceContextOptions.None);
		
		CreateShadersAndResources();
		
		RecreateD2DRenderTarget();
	}
	
	public void RecreateD2DRenderTarget()
	{
		if (_d2dContext == null || _window?.SwapChain == null)
			return;

		// 释放旧的目标
		_d2dRenderTarget?.Dispose();

		using var surface = _window.SwapChain.GetBuffer<IDXGISurface>(0);
		var bitmapProps = new BitmapProperties1(
			new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
			96, 96,
			BitmapOptions.Target | BitmapOptions.CannotDraw
		);

		_d2dRenderTarget = _d2dContext.CreateBitmapFromDxgiSurface(surface, bitmapProps);
		_d2dContext.Target = _d2dRenderTarget;
	}

	private void CreateShadersAndResources()
	{
		var device = _window.Device;
		if (device == null) throw new InvalidOperationException("Device is not initialized or already disposed.");
		
		string vertShaderSource = DotNetResourceManager.FetchResourceText("Resources/Shaders/Sprite-Default/VertShader.hlsl");
		string fragShaderSource = DotNetResourceManager.FetchResourceText("Resources/Shaders/Sprite-Default/FragShader.hlsl");

		var vertShaderBlob = Compiler.Compile(vertShaderSource, "main", "VertexShader.hlsl", "vs_4_0", ShaderFlags.None);
		var fragShaderBlob = Compiler.Compile(fragShaderSource, "main", "FragmentShader.hlsl", "ps_4_0", ShaderFlags.None); // ps means pixel shader, same as frag shader

		_vertShader = device.CreateVertexShader(vertShaderBlob.ToArray());
		_fragShader = device.CreatePixelShader(fragShaderBlob.ToArray());

		var inputElements = new[]
		{
			new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
			new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 12, 0),
			new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 20, 0)
		};
		_inputLayout = device.CreateInputLayout(inputElements, vertShaderBlob.ToArray());

		// vertex buffer (dynamic, for quads)
		var vbDesc = new BufferDescription()
		{
			Usage = ResourceUsage.Dynamic,
			BindFlags = BindFlags.VertexBuffer,
			CPUAccessFlags = CpuAccessFlags.Write,
			ByteWidth = (uint) (Marshal.SizeOf<Vertex>() * 4), // quad has 4 vertices
		};
		_vertBuffer = device.CreateBuffer(vbDesc);
		
		// index buffer (static, for quads)
		var ibDesc = new BufferDescription()
		{
			Usage = ResourceUsage.Immutable,
			BindFlags = BindFlags.IndexBuffer,
			ByteWidth = (uint) (sizeof(ushort) * QuadIndices.Length),
		};
		var ibInit = new SubresourceData(Marshal.UnsafeAddrOfPinnedArrayElement(QuadIndices, 0));
		_indexBuffer = device.CreateBuffer(ibDesc, ibInit);

		// sampler
		var sampDesc = new SamplerDescription()
		{
			Filter = Filter.MinMagMipLinear,
			AddressU = TextureAddressMode.Wrap,
			AddressV = TextureAddressMode.Wrap,
			AddressW = TextureAddressMode.Wrap,
			ComparisonFunc = ComparisonFunction.Never,
			MinLOD = 0, MaxLOD = float.MaxValue
		};
		_sampler = device.CreateSamplerState(sampDesc);

		// blend state for alpha blending
		var blendDesc = new BlendDescription()
		{
			AlphaToCoverageEnable = false,
			IndependentBlendEnable = false
		};
		blendDesc.RenderTarget[0] = new RenderTargetBlendDescription()
		{
			BlendEnable = true,
			SourceBlend = Blend.SourceAlpha, // SrcAlpha
			DestinationBlend = Blend.InverseSourceAlpha, // OneMinusSrcAlpha
			BlendOperation = BlendOperation.Add,
			SourceBlendAlpha = Blend.One, // One
			DestinationBlendAlpha = Blend.InverseSourceAlpha, // OneMinusSrcAlpha
			BlendOperationAlpha = BlendOperation.Add, // Addition
			RenderTargetWriteMask = ColorWriteEnable.All
		};
		_blendState = device.CreateBlendState(blendDesc);
	}
	
	public void BeginFrame()
	{
		EnsureNotDisposed();
		
		var ctx = _window.Device.ImmediateContext;
		_currentRenderTarget ??= _window.RenderTarget;
		
		ctx.ClearRenderTargetView(_currentRenderTarget, new Color4(0, 0, 0, 1)); // clear to black
		ctx.OMSetRenderTargets(_currentRenderTarget, null);

		// viewport
		var vp = new Viewport(0, 0, _window.Width, _window.Height);
		ctx.RSSetViewports([vp]);
		
		_d2dContext?.BeginDraw();
	}

	public void EndFrame()
	{
		EnsureNotDisposed();
		
		// end D2D
		_d2dContext?.EndDraw();
		
		// present
		_window.SwapChain.Present(1, PresentFlags.None);
	}

	public void Clear(Color color)
	{
		EnsureNotDisposed();
		var ctx = _window.Device.ImmediateContext;
		
		// clear both D3D and D2D render targets
		ctx.ClearRenderTargetView(_currentRenderTarget ?? _window.RenderTarget, color.ToColor4());
		_d2dContext?.Clear(color.ToColor4());
	}

	public void SetRenderTarget(object renderTarget)
	{
		EnsureNotDisposed();
		if (renderTarget is not ID3D11RenderTargetView rtv)
		{
			throw new ArgumentException("Render target must be of type ID3D11RenderTargetView1.", nameof(renderTarget));
		}

		_currentRenderTarget = rtv;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnsureNotDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(DirectXRenderContext), "Cannot access a disposed DirectXRenderContext.");
		}
	}
	
	public void Dispose()
	{
		if (_disposed) return;
		
		_d2dRenderTarget?.Dispose();
		_d2dContext?.Dispose();
		_d2dDevice?.Dispose();
		_dwriteFactory?.Dispose();
		_d2dFactory?.Dispose();
		_wicFactory?.Dispose();
		
		_vertShader?.Dispose();
		_fragShader?.Dispose();
		_inputLayout?.Dispose();
		_vertBuffer?.Dispose();
		_indexBuffer?.Dispose();
		_sampler?.Dispose();
		_blendState?.Dispose();

		_disposed = true;
		GC.SuppressFinalize(this);
	}
}