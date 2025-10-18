using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Rabotora.Core;
using Rabotora.Internal;
using Vortice;
using Vortice.D3DCompiler;
using Vortice.DCommon;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DirectWrite;
using Vortice.DXGI;
using Vortice.Mathematics;
using Blend = Vortice.Direct3D11.Blend;
using BlendDescription = Vortice.Direct3D11.BlendDescription;
using BlendOperation = Vortice.Direct3D11.BlendOperation;
using Color = Vortice.Mathematics.Color;
using Filter = Vortice.Direct3D11.Filter;
using FontStyle = Vortice.DirectWrite.FontStyle;
using FontWeight = Rabotora.Core.FontWeight;
using InputElementDescription = Vortice.Direct3D11.InputElementDescription;
using Rectangle = Rabotora.Core.Rectangle;

namespace Rabotora.Graphics;

/** SHIT WARNING **
 * If you still want to refactor this code, I suggested use some AI LLM's first.
 */
public class DirectXRenderContext : IRenderContext, IDisposable
{
	private readonly RWindow _window;
	private ID3D11RenderTargetView _currentRenderTarget;
	private ID2D1RenderTarget? _d2dRenderTarget;
	private ID2D1Factory1? _d2dFactory;
	private IDWriteFactory? _dwriteFactory;
	private Vortice.WIC.IWICImagingFactory2? _wicFactory;
	private bool _disposed;

	private ID3D11VertexShader? _shaderVert;
	private ID3D11PixelShader? _shaderFrag; // a.k.a. _shaderPixel
	private ID3D11InputLayout? _inputLayout;
	private ID3D11Buffer? _bufferVert;
	private ID3D11Buffer? _bufferIndex;
	private ID3D11SamplerState? _stateSampler;
	private ID3D11BlendState? _stateBlend;

	public DirectXRenderContext(RWindow window)
	{
		_window = window;
		_currentRenderTarget = _window.RenderTarget;

		InitializeD2D();
		InitializeSpriteBatch();
	}

	private void InitializeD2D()
	{
		_d2dFactory = D2D1.D2D1CreateFactory<ID2D1Factory1>();

		_dwriteFactory = DWrite.DWriteCreateFactory<IDWriteFactory>();

		_wicFactory = new Vortice.WIC.IWICImagingFactory2();
		
		using var backBuffer = _window.SwapChain.GetBuffer<ID3D11Texture2D>(0);
		using var surface = backBuffer.QueryInterface<IDXGISurface>();

		var props = new RenderTargetProperties(RenderTargetType.Default, new PixelFormat(Format.R8G8B8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied),
			96, 96, RenderTargetUsage.None, Vortice.Direct2D1.FeatureLevel.Default);
		
		_d2dRenderTarget = _d2dFactory.CreateDxgiSurfaceRenderTarget(surface, props);
	}

	private void InitializeSpriteBatch()
	{
		var device = _window.Device;
		string vertShaderCode = DotNetResourceManager.FetchResourceText("Internal/Shaders/VertexShader.hlsl");
		string fragShaderCode = DotNetResourceManager.FetchResourceText("Internal/Shaders/PixelShader.hlsl");

		var vertShaderBlob = Compiler.Compile(shaderSource: vertShaderCode, entryPoint: "main", sourceName: "VertexShader.hlsl",
			profile: "vs_4_0", ShaderFlags.Debug); // typeof(shaderBlob) == typeof(ReadOnlyMemory<byte>)
		var fragShaderBlob = Compiler.Compile(shaderSource: fragShaderCode, entryPoint: "main", sourceName: "PixelShader.hlsl",
			profile: "ps_4_0", ShaderFlags.Debug);

		_shaderVert = device.CreateVertexShader(vertShaderBlob.ToArray());
		_shaderFrag = device.CreatePixelShader(fragShaderBlob.ToArray());

		var inputElements = new InputElementDescription[]
		{
			new("POSITION", 0, Format.R32G32B32_Float, 0, 0),
			new("TEXCOORD", 0, Format.R32G32_Float, 12, 0),
			new("COLOR", 0, Format.R32G32B32A32_Float, 20, 0)
		};
		
		_inputLayout = device.CreateInputLayout(inputElements, vertShaderBlob.ToArray());
		
		var vertBufferDesc = new BufferDescription
		{
			Usage = ResourceUsage.Dynamic,
			BindFlags = BindFlags.VertexBuffer,
			CPUAccessFlags = CpuAccessFlags.Write,
			ByteWidth = 1024 * 1024,
		};
		
		_bufferVert = device.CreateBuffer(vertBufferDesc);
		
		var indices = new ushort[] { 0, 1, 2, 0, 2, 3 };
		var indexBufferDesc = new BufferDescription()
		{
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.IndexBuffer,
			ByteWidth = (uint) (sizeof(ushort) * indices.Length),
		};
		
		_bufferIndex = device.CreateBuffer(indexBufferDesc);

		var samplerDesc = new SamplerDescription()
		{
			Filter = Filter.MinMagMipLinear,
			AddressU = TextureAddressMode.Wrap,
			AddressV = TextureAddressMode.Wrap,
			MipLODBias = 0.0f,
			MaxAnisotropy = 1,
			ComparisonFunc = ComparisonFunction.Never,
			MinLOD = 0.0f,
			MaxLOD = float.MaxValue
		};
		
		_stateSampler = device.CreateSamplerState(samplerDesc);
		
		// Create blend state for additive (alpha) blending
		var blendDesc = new BlendDescription()
		{
			AlphaToCoverageEnable = false,
			IndependentBlendEnable = false
		};

		blendDesc.RenderTarget[0] = new RenderTargetBlendDescription()
		{
			BlendEnable = true,
			SourceBlend = Blend.SourceAlpha,
			DestinationBlend = Blend.InverseSourceAlpha,
			BlendOperation = BlendOperation.Add,
			SourceBlendAlpha = Blend.One,
			DestinationBlendAlpha = Blend.InverseSourceAlpha,
			BlendOperationAlpha = BlendOperation.Add,
			RenderTargetWriteMask = ColorWriteEnable.All
		};

		_stateBlend = device.CreateBlendState(blendDesc);
	}

	public void BeginFrame()
	{
		_window.Device.ImmediateContext.ClearRenderTargetView(_currentRenderTarget, new Color(0, 0, 0, 1));
		_window.Device.ImmediateContext.OMSetRenderTargets(1, [_currentRenderTarget]);
		
		var viewport = new Viewport(0, 0, _window.Width, _window.Height);
		_window.Device.ImmediateContext.RSSetViewports([viewport]);
		
		_d2dRenderTarget?.BeginDraw();
	}

	public void EndFrame()
	{
		_d2dRenderTarget?.EndDraw();

		_window.SwapChain.Present(1, PresentFlags.None);
	}

	public void DrawTexture(ITexture2D texture, Rectangle rect, float alpha = 1.0f)
	{
		if (texture.NativeTexture is not ID3D11ShaderResourceView resView)
		{
			throw new ArgumentException("Invalid texture type or the texture type is not supported.", nameof(texture));
		}
		if (_bufferVert == null || _bufferIndex == null)
		{
			throw new InvalidOperationException("Vertex buffer or index buffer is not initialized.");
		}
		
		// The principle of this method is 'draw a rectangle texture with two triangles'.
		
		var device = _window.Device;
		var ctx = device.ImmediateContext;
		
		ctx.IASetInputLayout(_inputLayout);
		ctx.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
		ctx.VSSetShader(_shaderVert);
		ctx.PSSetShader(_shaderFrag);
		ctx.PSSetShaderResource(0, resView);
		ctx.PSSetSampler(0, _stateSampler);
		
		ctx.OMSetBlendState(_stateBlend, new Color(0, 0, 0, 0), 0xFFFFFFFF);
		
		// Convert window coordinates to normalized device coordinates (-1 to 1)
		float left = rect.X / (float)_window.Width * 2 - 1;
		float right = (rect.X + rect.Width) / (float)_window.Width * 2 - 1;
		float top = -(rect.Y / (float) _window.Height * 2 - 1);
		float bottom = -((rect.Y + rect.Height) / (float) _window.Height * 2 - 1);

		var vertices = new[]
		{
			// Position (x,y,z), TexCoord (u,v), Color (r,g,b,a)
			new {Position = new Vector3(left, top, 0), TexCoord = new Vector2(0, 0), Color = new Vector4(1, 1, 1, alpha)},
			new {Position = new Vector3(right, top, 0), TexCoord = new Vector2(1, 0), Color = new Vector4(1, 1, 1, alpha)},
			new {Position = new Vector3(right, bottom, 0), TexCoord = new Vector2(1, 1), Color = new Vector4(1, 1, 1, alpha)},
			new {Position = new Vector3(left, bottom, 0), TexCoord = new Vector2(0, 1), Color = new Vector4(1, 1, 1, alpha)}
		};

		var mappedResource = ctx.Map(_bufferVert, MapMode.WriteDiscard);
		unsafe
		{
			var dataPtr = (byte*)mappedResource.DataPointer;
			int vertexSize = sizeof(Vector3) + sizeof(Vector2) + sizeof(Vector4);

			for (int i = 0; i < vertices.Length; i++)
			{
				var offset = i * vertexSize;
				
				Marshal.StructureToPtr(vertices[i].Position, new IntPtr(dataPtr + offset), false);
				
				Marshal.StructureToPtr(vertices[i].TexCoord, new IntPtr(dataPtr + offset + sizeof(Vector3)), false);
				
				Marshal.StructureToPtr(vertices[i].Color, new IntPtr(dataPtr + offset + sizeof(Vector3) + sizeof(Vector2)), false);
			}
		}

		ctx.Unmap(_bufferVert, 0);

		ctx.IASetVertexBuffer(0, _bufferVert, (uint)(Marshal.SizeOf<Vector3>() + Marshal.SizeOf<Vector2>() + Marshal.SizeOf<Vector4>()));
		ctx.IASetIndexBuffer(_bufferIndex, Format.R16_UInt, 0);
		
		ctx.DrawIndexed(6, 0, 0);
	}

	public void Clear(Color color)
	{
		_window.Device.ImmediateContext.ClearRenderTargetView(_currentRenderTarget, color.ToColor4());
		_d2dRenderTarget?.Clear(color.ToColor4());
	}

	public void DrawText(string text, IFont font, Color color, Point position, TextAlign align = TextAlign.Left, float alpha = 1.0f)
	{
		if (font.NativeFont is not IDWriteTextFormat textFormat)
		{
			throw new ArgumentException("Invalid font type or the font type is not supported.", nameof(font));
		}
		if (_dwriteFactory == null || _d2dRenderTarget == null)
		{
			throw new InvalidOperationException("DirectWrite factory or Direct2D render target is not initialized.");
		}

		textFormat.TextAlignment = align switch
		{
			TextAlign.Left => TextAlignment.Leading,
			TextAlign.Center => TextAlignment.Center,
			TextAlign.Right => TextAlignment.Trailing,
			_ => TextAlignment.Leading
		};
		
		using var textLayout = _dwriteFactory.CreateTextLayout(text, textFormat, float.MaxValue, float.MaxValue);
		
		var metrics = textLayout.Metrics;
		var textRect = new RawRectF(position.X, position.Y, position.X + metrics.Width, position.Y + metrics.Height);
		
		// Premultiplied alpha (预乘 alpha)
		using var brush = _d2dRenderTarget.CreateSolidColorBrush(new Color4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f * alpha));

		_d2dRenderTarget.DrawText(text, textFormat, textRect, brush);
	}

	public void SetRenderTarget(object renderTarget)
	{
		if (renderTarget is not ID3D11RenderTargetView rtv)
		{
			throw new ArgumentException("Invalid render target type. Expected ID3D11RenderTargetView.", nameof(renderTarget));
		}
		if (_d2dFactory == null)
		{
			throw new InvalidOperationException("Direct2D factory is not initialized.");
		}
		
		_currentRenderTarget = rtv;
		
		_d2dRenderTarget?.Dispose();
		
		using var backBuffer = _window.SwapChain.GetBuffer<ID3D11Texture2D>(0);
		using var surface = backBuffer.QueryInterface<IDXGISurface>();

		var props = new RenderTargetProperties(RenderTargetType.Default, new PixelFormat(Format.R8G8B8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied),
			96, 96, RenderTargetUsage.None, Vortice.Direct2D1.FeatureLevel.Default);
		
		_d2dRenderTarget = _d2dFactory.CreateDxgiSurfaceRenderTarget(surface, props);
	}

	public ITexture2D LoadTexture(string path)
	{
		if (_wicFactory == null)
		{
			throw new InvalidOperationException("WIC factory is not initialized.");
		}
		
		var device = _window.Device;

		using var decoder = _wicFactory.CreateDecoderFromFileName(path);
		
		using var frame = decoder.GetFrame(0);

		var converter = _wicFactory.CreateFormatConverter();
		converter.Initialize(frame, Vortice.WIC.PixelFormat.Format32bppBGRA, Vortice.WIC.BitmapDitherType.None, null, 0.0, Vortice.WIC.BitmapPaletteType.Custom);

		int width = converter.Size.Width;
		int height = converter.Size.Height;
		
		var textureDesc = new Texture2DDescription()
		{
			Width = (uint)width,
			Height = (uint)height,
			MipLevels = 1, ArraySize = 1,
			Format = Format.B8G8R8A8_UNorm,
			SampleDescription = new SampleDescription(1, 0),
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.ShaderResource,
			CPUAccessFlags = CpuAccessFlags.None,
			MiscFlags = ResourceOptionFlags.None
		};
		
		byte[] pixelData = new byte[width * height * 4];
		converter.CopyPixels((uint)(width * 4), pixelData);

		var initialData = new SubresourceData()
		{
			DataPointer = Marshal.UnsafeAddrOfPinnedArrayElement(pixelData, 0),
			RowPitch = (uint)(width * 4)
		};
		var texture = device.CreateTexture2D(textureDesc, initialData);

		var resView = device.CreateShaderResourceView(texture);
		
		return new DXTexture2D(texture, resView);
	}

	public IFont CreateFont(string fontFamily, float size, FontWeight weight = FontWeight.Regular)
	{
		if (_dwriteFactory == null)
		{
			throw new InvalidOperationException("DirectWrite factory is not initialized.");
		}
		
		// Convert FontWeight enum to DirectWrite font weight
		var dwriteWeight = weight switch
		{
			FontWeight.Thin => Vortice.DirectWrite.FontWeight.Thin,
			FontWeight.Light => Vortice.DirectWrite.FontWeight.Light,
			FontWeight.Regular => Vortice.DirectWrite.FontWeight.Normal,
			FontWeight.Medium => Vortice.DirectWrite.FontWeight.Medium,
			FontWeight.Bold => Vortice.DirectWrite.FontWeight.Bold,
			FontWeight.Black => Vortice.DirectWrite.FontWeight.Black,
			_ => Vortice.DirectWrite.FontWeight.Normal
		};

		string localeName = Encoding.Default.CodePage switch
		{
			936 or 2052 => "zh-CN",
			950 or 1028 => "zh-TW",
			932 or 1041 => "ja-JP",
			437 or 1033 => "en-US",
			_ => CultureInfo.CurrentCulture.Name
		};
		
		var textFormat = _dwriteFactory.CreateTextFormat(fontFamily, null!, dwriteWeight, FontStyle.Normal, FontStretch.Normal, size, localeName);
		
		return new DXFont(textFormat, fontFamily, size, weight);
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_d2dRenderTarget?.Dispose();
			_d2dFactory?.Dispose();
			_dwriteFactory?.Dispose();
			_wicFactory?.Dispose();
			
			_shaderVert?.Dispose();
			_shaderFrag?.Dispose();
			_inputLayout?.Dispose();
			_bufferVert?.Dispose();
			_bufferIndex?.Dispose();
			_stateSampler?.Dispose();
			_stateBlend?.Dispose();
			
			_disposed = true;
		}
	}
}