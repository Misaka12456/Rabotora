using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.UI;
using RabotoraX.Core.Videos;
using RabotoraX.Interop.Direct3D11.Rendering;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Direct3D11;
using Vortice.DirectWrite;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.WIC;
using BitmapInterpolationMode = Vortice.Direct2D1.BitmapInterpolationMode;
using RRect = RabotoraX.Core.Mathematics.Rect;
using AlphaMode = Vortice.DCommon.AlphaMode;

namespace RabotoraX.Interop.Direct3D11;

public partial class DirectX11
{
	private partial class D2DContextImpl
	{
		private readonly DirectX11 _parent;
		private readonly Dictionary<Color4, ID2D1SolidColorBrush> _brushCache = new();
		private readonly Dictionary<(string font, float size), IDWriteTextFormat> _formatCache = new();
		private ID2D1DeviceContext D2DContext => _parent._d2dContext!;
		private INativeShader? _currentShader;
		
		public D2DContextImpl(DirectX11 parent)
		{
			_parent = parent;
		}

		public void BeginDraw()
		{
			D2DContext.Transform = Matrix3x2.Identity;
			D2DContext.SetDpi(96, 96); // Set DPI to default (96) because we have already calculated the sizes in RUILayout
			D2DContext.BeginDraw();
		}

		public void EndDraw()
		{
			D2DContext.EndDraw();
		}

		public void Clear(float r, float g, float b, float a)
		{
			D2DContext.Clear(new Color4(r, g, b, a));
		}

		public void DrawRectangle(float x, float y, float width, float height, float r, float g, float b, float a, float strokeWidth)
		{
			var brush = GetCachedBrush(r, g, b, a);
			var rect = new RawRectF(x, y, x + width, y + height);
			D2DContext.DrawRectangle(rect, brush, strokeWidth);
		}

		public void FillRectangle(float x, float y, float width, float height, float r, float g, float b, float a)
		{
			var brush = GetCachedBrush(r, g, b, a);
			var rect = new RawRectF(x, y, x + width, y + height);
			D2DContext.FillRectangle(rect, brush);
		}

		public void DrawImage(INativeTexture2D texture, float x, float y, float width, float height, float opacity = 1)
		{
			var descRect = new Rect(x, y, width, height); 
			// if (texture is not D2DTexture d2dTex) return;
			// D2DContext.DrawBitmap(d2dTex.Bitmap, opacity, BitmapInterpolationMode.Linear, descRect);
			switch (texture)
			{
				case DX11NV12VideoTexture nv12:
					nv12.D2DBitmap ??= CreateNV12D2DBitmap(nv12.OutputRgba, D2DContext);
					D2DContext.DrawBitmap(nv12.D2DBitmap, opacity, BitmapInterpolationMode.Linear, descRect);
					break;
				case D2DTexture d2dTex:
					D2DContext.DrawBitmap(d2dTex.Bitmap, opacity, BitmapInterpolationMode.Linear, descRect);
					break;
			}
		}

		public void DrawImage(INativeTexture2D texture, RRect sourceRect, float x, float y, float width, float height, float opacity = 1)
		{
			var destRect = new Rect(x, y, width, height);
			var srcRect = new RawRectF(sourceRect.X, sourceRect.Y, sourceRect.X + sourceRect.Width, sourceRect.Y + sourceRect.Height);
			switch (texture)
			{
				case DX11NV12VideoTexture nv12:
					nv12.D2DBitmap ??= CreateNV12D2DBitmap(nv12.OutputRgba, D2DContext);
					D2DContext.DrawBitmap(nv12.D2DBitmap, destRect, opacity, BitmapInterpolationMode.Linear, srcRect);
					return;
				case D2DTexture d2dTex:
					if (_currentShader is DX11CombinedShader {D2DEffectId: not null} combined)
					{
						var effect = _parent.GetEffectInstance(combined.D2DEffectId.Value);
						using var image = effect.QueryInterface<ID2D1Image>();
						D2DContext.DrawImage(image, new Vector2(x, y), srcRect, InterpolationMode.Linear, CompositeMode.SourceOver);
					}
					else
					{
						D2DContext.DrawBitmap(d2dTex.Bitmap, destRect, opacity, BitmapInterpolationMode.Linear, srcRect);
					}
					break;
			}
		}

		public void DrawText(string text, string fontName, float fontSize, float x, float y, float r, float g, float b, float a)
		{
			var brush = GetCachedBrush(r, g, b, a);
			var format = GetCachedFormat(fontName, fontSize);
			D2DContext.DrawText(text, format, new Rect(x, y, 10000, 10000), brush);
		}

		public void DrawTextLayout(INativeTextLayout layout, float x, float y, float r, float g, float b, float a)
		{
			if (layout is not D2DTextLayout d2dLayout) return;
			if (D2DContext.NativePointer == 0)
			{
				return;
			}
			var brush = GetCachedBrush(r, g, b, a);
			D2DContext.DrawTextLayout(new Vector2(x, y), d2dLayout.InternalLayout, brush, DrawTextOptions.None);
		}

		public void SetTransform(Matrix3x2 matrix)
		{
			D2DContext.Transform = matrix;
		}

		public void SetShader(INativeShader? shader)
		{
			_currentShader = shader;
		}

		public INativeTexture2D CreateTexture(Stream stream, bool leaveOpen = false)
		{
			try
			{
				using var decoder = _parent._wicFactory!.CreateDecoderFromStream(stream);
				using var frame = decoder.GetFrame(0);
				using var converter = _parent._wicFactory.CreateFormatConverter();
				
				converter.Initialize(frame, PixelFormat.Format32bppPBGRA);

				var bitmap = D2DContext.CreateBitmapFromWicBitmap(converter, new BitmapProperties(
					new Vortice.DCommon.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
				return new D2DTexture(bitmap);
			}
			finally
			{
				if (!leaveOpen)
				{
					stream.Dispose();
				}
			}
		}

		public unsafe INativeTexture2D CreateTexture(int width, int height, ReadOnlyMemory<byte> pixelData)
		{
			var pixelFormat = new Vortice.DCommon.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);
			int pitch = width * 4; // Assuming 4 bytes per pixel (BGRA)
			
			var props = new BitmapProperties(pixelFormat);
			fixed (byte* pData = pixelData.Span)
			{
				var bitmap = D2DContext.CreateBitmap(new SizeI(width, height), (nint)pData, (uint)pitch, props);
				return new D2DTexture(bitmap);
			}
		}

		public unsafe INativeTexture2D CreateVideoTexture(int width, int height, ReadOnlyMemory<byte>? initialData = null)
		{
			var pixelFormat = new Vortice.DCommon.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore);
			int pitch = width * 4; // Assuming 4 bytes per pixel (BGRA)
			
			var props = new BitmapProperties(pixelFormat);
			if (initialData.HasValue)
			{
				var data = initialData.Value;
				fixed (byte* pData = data.Span)
				{
					var bitmap = D2DContext.CreateBitmap(new SizeI(width, height), (nint)pData, (uint)pitch, props);
					return new D2DTexture(bitmap);
				}
			}
			else
			{
				var bitmap = D2DContext.CreateBitmap(new SizeI(width, height), props);
				return new D2DTexture(bitmap);
			}
		}

		public INativeTexture2D CreateVideoTexture(int width, int height, VideoPixelFormat format, ReadOnlyMemory<byte>? initialData = null)
		{
			if (format == VideoPixelFormat.NV12)
			{
				return _parent.CreateNV12VideoTexture(width, height);
			}
			
			return CreateVideoTexture(width, height, initialData);
		}

		public INativeTexture2D CreateEmptyTexture(int width, int height)
		{
			var pixelFormat = new Vortice.DCommon.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);
			var props = new BitmapProperties(pixelFormat);
    
			var bitmap = D2DContext.CreateBitmap(new SizeI(width, height), props);
			return new D2DTexture(bitmap);
		}

		public INativeTextLayout CreateTextLayout(string text, string fontName, float fontSize, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue)
		{
			var format = GetCachedFormat(fontName, fontSize);
			var layout = _parent._dwriteFactory!.CreateTextLayout(text, format, maxWidth, maxHeight);
			return new D2DTextLayout(layout);
		}

		private ID2D1SolidColorBrush GetCachedBrush(float r, float g, float b, float a)
		{
			var color = new Color4(r, g, b, a);
			if (!_brushCache.TryGetValue(color, out var brush))
			{
				brush = D2DContext.CreateSolidColorBrush(color);
				_brushCache[color] = brush;
			}
			return brush;
		}

		private IDWriteTextFormat GetCachedFormat(string fontName, float fontSize)
		{
			var key = (fontName, fontSize);
			if (!_formatCache.TryGetValue(key, out var format))
			{
				format = _parent._dwriteFactory!.CreateTextFormat(fontName, fontSize);
				_formatCache[key] = format;
			}
			return format;
		}
		
		public void Dispose()
		{
			foreach (var brush in _brushCache.Values)
			{
				brush.Dispose();
			}
			foreach (var format in _formatCache.Values)
			{
				format.Dispose();
			}
			_brushCache.Clear();
			_formatCache.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ID2D1Bitmap1 CreateNV12D2DBitmap(ID3D11Texture2D rgbaTex, ID2D1DeviceContext context)
		{
			using var surface = rgbaTex.QueryInterface<IDXGISurface>();
			return context.CreateBitmapFromDxgiSurface(surface, new BitmapProperties1(new Vortice.DCommon.PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied),
				96, 96, BitmapOptions.None));
		}
	}
}