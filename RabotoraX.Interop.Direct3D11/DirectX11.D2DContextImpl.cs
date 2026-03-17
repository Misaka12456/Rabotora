using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using RabotoraX.Core.Graphics;
using RabotoraX.Interop.Direct3D11.Rendering;
using Vortice;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using Vortice.WIC;
using BitmapInterpolationMode = Vortice.Direct2D1.BitmapInterpolationMode;
using RRect = RabotoraX.Core.Mathematics.Rect;

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
			// if (texture is not D2DTexture d2dTex) return;
			var nativeTex = texture is Texture2D managedTex ? managedTex.NativeTexture : texture;
			if (nativeTex is not D2DTexture d2dTex) return;
			var descRect = new Rect(x, y, width, height); 
			D2DContext.DrawBitmap(d2dTex.Bitmap, opacity, BitmapInterpolationMode.Linear, descRect);
		}

		public void DrawImage(INativeTexture2D texture, RRect sourceRect, float x, float y, float width, float height, float opacity = 1)
		{
			var nativeTex = texture is Texture2D managedTex ? managedTex.NativeTexture : texture;
			if (nativeTex is not D2DTexture d2dTex) return;
			var destRect = new Rect(x, y, width, height); 
			var srcRect = new RawRectF(sourceRect.X, sourceRect.Y, sourceRect.X + sourceRect.Width, sourceRect.Y + sourceRect.Height);

			if (_currentShader is DX11CombinedShader { D2DEffectId: not null } combined)
			{
				var effect = _parent.GetEffectInstance(combined.D2DEffectId.Value);
				using var image = effect.QueryInterface<ID2D1Image>();
				D2DContext.DrawImage(image, new Vector2(x, y), srcRect, InterpolationMode.Linear, CompositeMode.SourceOver);
			}
			else
			{
				D2DContext.DrawBitmap(d2dTex.Bitmap, destRect, opacity, BitmapInterpolationMode.Linear, srcRect);
			}
		}

		public void DrawText(string text, string fontName, float fontSize, float x, float y, float r, float g, float b, float a)
		{
			var brush = GetCachedBrush(r, g, b, a);
			var format = GetCachedFormat(fontName, fontSize);
			D2DContext.DrawText(text, format, new Rect(x, y, 10000, 10000), brush);
		}

		public void SetTransform(Matrix3x2 matrix)
		{
			D2DContext.Transform = matrix;
		}

		public void SetShader(INativeShader? shader)
		{
			_currentShader = shader;
		}

		public INativeTexture2D CreateTexture(string path)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException($"Texture file not found: {path}");
			}
			
			using var decoder = _parent._wicFactory!.CreateDecoderFromFileName(path);
			using var frame = decoder.GetFrame(0);
			
			using var converter = _parent._wicFactory.CreateFormatConverter();
			
			converter.Initialize(frame, PixelFormat.Format32bppPBGRA);
			
			var bitmap = D2DContext.CreateBitmapFromWicBitmap(converter, new BitmapProperties(
				new Vortice.DCommon.PixelFormat(Vortice.DXGI.Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied)));
			
			return new D2DTexture(bitmap);
		}

		public INativeTexture2D CreateTexture(byte[] data)
		{
			var stream = new MemoryStream(data);
			return CreateTexture(stream, leaveOpen: false);
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
					new Vortice.DCommon.PixelFormat(Vortice.DXGI.Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied)));
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

		// 统一获取 TextFormat 的逻辑 (重点优化)
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
	}
}