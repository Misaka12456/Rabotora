using System.Numerics;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.UI;
using RabotoraX.Interop.Direct3D12.Rendering;
using RabotoraX.Interop.Direct3D12.Utility;
using SkiaSharp;
using Vortice.Direct3D12;
using Vortice.DXGI;
using Vortice.Mathematics;
using Rect = RabotoraX.Core.Mathematics.Rect;

namespace RabotoraX.Interop.Direct3D12;

public partial class DirectX12
{

	private sealed partial class DX12SkiaContextImpl
	{
		private readonly DirectX12 _api;
		
		private GRContext? _grContext;
		private SKSurface? _surface;
		private GRBackendTexture? _uiBackendTexture;
		
		private DX12Texture2D? _uiGpuTexture;
		private SKShader? _customSkiaShader;
		private INativeShader? _lastCustomNativeShader;

		public DX12SkiaContextImpl(DirectX12 api)
		{
			_api = api;
			InitGPUContext();
			Resize(api.FramebufferSize.Width, api.FramebufferSize.Height);
		}

		private void InitGPUContext()
		{
			using var factory = DXGI.CreateDXGIFactory2<IDXGIFactory4>(false);
			IDXGIAdapter? adapter = null;
			for (uint i = 0; factory.EnumAdapters(i, out var tempAdapter).Success; i++)
			{
				if (tempAdapter.Description.Luid == _api._deviceLuid)
				{
					adapter = tempAdapter;
					break;
				}
				tempAdapter.Dispose();
			}

			var backendConext = new GRD3DBackendContext()
			{
				Adapter = adapter!.NativePointer,
				Device = _api.Device!.NativePointer,
				Queue = _api._commandQueue!.NativePointer,
				ProtectedContext = false
			};
			
			_grContext = GRContext.CreateDirect3D(backendConext);
			adapter.Dispose();
		}

		public void Resize(int width, int height)
		{
			_api.WaitIdle();
			_surface?.Dispose();
			_uiBackendTexture?.Dispose();
			_uiGpuTexture?.Dispose();

			var desc = ResourceDescription.Texture2D(Format.B8G8R8A8_UNorm, (uint) width, (uint) height, 1, 1, 1, 0,
				ResourceFlags.AllowRenderTarget);

			var res = _api.Device.CreateCommittedResource(new HeapProperties(HeapType.Default), HeapFlags.None, desc, ResourceStates.RenderTarget);

			_uiGpuTexture = new DX12Texture2D(res, width, height);

			var srvHandle = _api._srvHeap!.GetCPUDescriptorHandleForHeapStart();
			var srvDesc = new ShaderResourceViewDescription()
			{
				Format = Format.B8G8R8A8_UNorm,
				ViewDimension = ShaderResourceViewDimension.Texture2D,
				Shader4ComponentMapping = ShaderComponentMapping.Default,
				Texture2D = new Texture2DShaderResourceView() {MipLevels = 1}
			};
			_api.Device.CreateShaderResourceView(_uiGpuTexture.Resource, srvDesc, srvHandle);

			var resInfo = new GRD3DTextureResourceInfo()
			{
				Resource = _uiGpuTexture.Resource.NativePointer,
				ResourceState = (uint) ResourceStates.RenderTarget,
				Format = (uint) Format.B8G8R8A8_UNorm,
				LevelCount = 1,
				SampleCount = 1,
				SampleQualityPattern = 0,
				Protected = false // ProtectedResource = 0
			};

			_uiBackendTexture = new GRBackendTexture(width, height, resInfo);
			_surface = SKSurface.Create(_grContext, _uiBackendTexture, GRSurfaceOrigin.TopLeft, 1, SKColorType.Bgra8888);
		}

		public void BeginDraw()
		{
			var cmdList = _api.CommandList!;
			cmdList.ResourceBarrierTransition(_uiGpuTexture!.Resource, ResourceStates.PixelShaderResource, ResourceStates.RenderTarget);

			_grContext!.ResetContext();
			_surface!.Canvas.ResetMatrix();
			
			var rtvHandle = _api.MainRenderTexture!.RtvHandle;
			cmdList.ClearRenderTargetView(rtvHandle, new Color4(0, 0, 1, 1));
		}

		public void EndDraw()
		{
			_surface!.Canvas.Flush();
			_grContext!.Flush(true);

			var cmdList = _api.CommandList!;
			_api.ApplySetRenderTarget(null);

			cmdList.ResourceBarrierTransition(_uiGpuTexture!.Resource, ResourceStates.RenderTarget, ResourceStates.PixelShaderResource);
    
			cmdList.SetPipelineState(_api._uiBlitPso!);
			cmdList.RSSetViewport(new Viewport(0, 0, _api.FramebufferSize.Width, _api.FramebufferSize.Height));
			cmdList.RSSetScissorRects(new RectI(0, 0, _api.FramebufferSize.Width, _api.FramebufferSize.Height));
    
			cmdList.SetDescriptorHeaps(1, [_api._srvHeap!]);
			cmdList.SetGraphicsRootDescriptorTable(1, _api._srvHeap!.GetGPUDescriptorHandleForHeapStart());
			cmdList.IASetPrimitiveTopology(Vortice.Direct3D.PrimitiveTopology.TriangleList);
			
			_grContext.Flush(true);
			_grContext.ResetContext(); 
			cmdList.DrawInstanced(3, 1, 0, 0);
    
			cmdList.ResourceBarrierTransition(_uiGpuTexture.Resource, ResourceStates.PixelShaderResource, ResourceStates.RenderTarget);
		}

		public void Clear(float r, float g, float b, float a)
		{
			_surface!.Canvas.Clear(new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255)));
			_grContext!.Flush();
		}

		public void DrawImage(INativeTexture2D texture, float x, float y, float width, float height, float opacity = 1)
		{
			if (texture is not DX12Texture2D dxTex) return;
			var texInfo = new GRD3DTextureResourceInfo()
			{
				Resource = dxTex.Resource.NativePointer,
				ResourceState = (uint) ResourceStates.PixelShaderResource,
				Format = (uint) Format.B8G8R8A8_UNorm,
				LevelCount = 1,
				SampleCount = 1,
				SampleQualityPattern = 0,
				Protected = false
			};
			
			using var backendTex = new GRBackendTexture(dxTex.Width, dxTex.Height, texInfo);
			using var skImage = SKImage.FromTexture(_grContext, backendTex, GRSurfaceOrigin.TopLeft, SKColorType.Bgra8888, SKAlphaType.Opaque);
			using var paint = new SKPaint();
			if (opacity < 1.0f)
			{
				paint.Color = new SKColor(255, 255, 255, (byte)(opacity * 255));
			}

			if (_customSkiaShader != null)
			{
				paint.Shader = _customSkiaShader;
			}
			
			_surface!.Canvas.DrawImage(skImage, new SKRect(x, y, x + width, y + height), paint);
		}
		
		public void DrawImage(INativeTexture2D texture, Rect sourceRect, float x, float y, float width, float height, float opacity = 1)
		{
			if (texture is not DX12Texture2D dxTex) return;

			var texInfo = new GRD3DTextureResourceInfo()
			{
				Resource = dxTex.Resource.NativePointer,
				ResourceState = (uint) ResourceStates.PixelShaderResource,
				Format = (uint) Format.B8G8R8A8_UNorm,
				LevelCount = 1,
				SampleCount = 1,
				SampleQualityPattern = 0,
				Protected = false
			};
			
			using var backendTex = new GRBackendTexture(dxTex.Width, dxTex.Height, texInfo);
			using var skImage = SKImage.FromTexture(_grContext, backendTex, GRSurfaceOrigin.TopLeft, SKColorType.Bgra8888, SKAlphaType.Opaque);
			
			using var paint = new SKPaint();
			if (opacity < 1.0f)
			{
				paint.Color = new SKColor(255, 255, 255, (byte)(opacity * 255));
			}
			if (_customSkiaShader != null)
			{
				paint.Shader = _customSkiaShader;
			}
			
			var srcSkRect = new SKRect(sourceRect.X, sourceRect.Y, sourceRect.X + sourceRect.Width, sourceRect.Y + sourceRect.Height);
			var dstSkRect = new SKRect(x, y, x + width, y + height);
			_surface!.Canvas.DrawImage(skImage, srcSkRect, dstSkRect, paint);
		}
		
		public void DrawText(string text, string fontName, float fontSize, float x, float y, float r, float g, float b, float a)
		{
			using var font = new SKFont(SKTypeface.FromFamilyName(fontName), fontSize);
			using var paint = new SKPaint();
			paint.Color = new SKColor((byte) (r * 255), (byte) (g * 255), (byte) (b * 255), (byte) (a * 255));
			paint.IsAntialias = true;
			_surface!.Canvas.DrawText(text, x, y + fontSize, SKTextAlign.Left, font, paint);
		}
		
		public void DrawTextLayout(INativeTextLayout textLayout, float x, float y, float r, float g, float b, float a)
		{
			if (textLayout is not DX12SkiaTextLayout layout) return;

			using var paint = new SKPaint();
			paint.Color = new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
			paint.IsAntialias = true;
          
			#region Draw Text Lines Separately to Handle Line Spacing and Baseline
			string[] lines = layout.Text.Split(["\n", "\r\n"], StringSplitOptions.None);
			float currentY = y + layout.FontSize; // Start drawing from the baseline of the first line
			foreach (string line in lines)
			{
				_surface!.Canvas.DrawText(line, x, currentY, SKTextAlign.Left, layout.Font, paint);
				currentY += layout.FontSize * layout.LineSpacing; // Move down for the next line based on line spacing
			}
			#endregion
		}
		
		public void DrawRectangle(float x, float y, float width, float height, float r, float g, float b, float a, float stroke)
		{
			using var paint = new SKPaint();
			paint.Color = new SKColor((byte) (r * 255), (byte) (g * 255), (byte) (b * 255), (byte) (a * 255));
			paint.Style = SKPaintStyle.Stroke;
			paint.StrokeWidth = stroke;
			paint.IsAntialias = true;
			_surface!.Canvas.DrawRect(x, y, width, height, paint);
		}

		public void FillRectangle(float x, float y, float width, float height, float r, float g, float b, float a)
		{
			using var paint = new SKPaint();
			paint.Color = new SKColor((byte) (r * 255), (byte) (g * 255), (byte) (b * 255), (byte) (a * 255));
			paint.Style = SKPaintStyle.Fill;
			_surface!.Canvas.DrawRect(x, y, width, height, paint);
		}

		public INativeTexture2D CreateTexture(Stream stream, bool leaveOpen = false)
		{
			using var codec = SKCodec.Create(stream);
			var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);
			using var skBitmap = new SKBitmap(info);
			codec.GetPixels(info, skBitmap.GetPixels());
			var texture = CreateTexture(skBitmap.Width, skBitmap.Height, skBitmap.GetPixelSpan().ToArray());
			
			if (!leaveOpen) stream.Dispose();
			return texture;
		}

		public INativeTexture2D CreateTexture(int width, int height, ReadOnlyMemory<byte> pixelData)
		{
			return _api.CreateTexture2D(width, height, pixelData.Span);
		}

		public INativeTexture2D CreateVideoTexture(int width, int height, ReadOnlyMemory<byte>? initialData = null)
		{
			return _api.CreateTexture2D(width, height, initialData.GetValueOrDefault().Span);
		}

		public INativeTexture2D CreateEmptyTexture(int width, int height)
		{
			return _api.CreateTexture2D(width, height, ReadOnlySpan<byte>.Empty);
		}

		public INativeTextLayout CreateTextLayout(string text, string fontName, float fontSize, float maxWidth = Single.MaxValue, float maxHeight = float.MaxValue)
		{
			return new DX12SkiaTextLayout(text, fontName, fontSize, maxWidth, maxHeight);
		}
		
		public void SetTransform(Matrix3x2 matrix)
		{
			_surface!.Canvas.SetMatrix(new SKMatrix(matrix.M11, matrix.M21, matrix.M31,
				matrix.M12, matrix.M22, matrix.M32,
				0, 0, 1));
		}

		public void SetShader(INativeShader? shader)
		{
			if (shader == null)
			{
				_customSkiaShader?.Dispose();
				_customSkiaShader = null;
				return;
			}
			if (shader == _lastCustomNativeShader) return; // No need to recompile if the same shader is set again
			if (shader is not DX12Shader or DX12ShaderProgram) return; // Only support DX12Shader or DX12ShaderProgram for now

			string? hlslSource = null;
			if (shader is DX12Shader dxShader)
			{
				hlslSource = dxShader.FragSource;
			}
			else if (shader is DX12ShaderProgram dxShaderProg)
			{
				hlslSource = dxShaderProg.FragSource;
			}
			if (string.IsNullOrWhiteSpace(hlslSource)) return;

			try
			{
				string sksl = ShaderTranspiler.CrossCompileHlslToSksl(hlslSource, "PSMain", "ps_5_0");

				using var effect = SKRuntimeEffect.CreateShader(sksl, out string errors);
				if (!string.IsNullOrEmpty(errors))
				{
					throw new InvalidOperationException($"Failed to compile shader: {errors}");
				}

				_customSkiaShader?.Dispose();
				_customSkiaShader = effect.ToShader();
				_lastCustomNativeShader = shader;
			}
			catch (Exception ex)
			{
#if DEBUG
				Console.WriteLine($"Shader compilation error: {ex.Message}");
#endif
			}
		}

		public void Dispose()
		{
			_surface?.Dispose();
			_uiBackendTexture?.Dispose();
			_uiGpuTexture?.Dispose();
			_grContext?.Dispose();
			_customSkiaShader?.Dispose();
		}
	}
}