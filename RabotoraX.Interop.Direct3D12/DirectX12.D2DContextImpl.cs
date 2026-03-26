using System.Numerics;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.UI;
using RabotoraX.Interop.Direct3D11.Rendering;
using Vortice; // 复用 DX11 里的 D2DTexture 和 D2DTextLayout
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using BitmapInterpolationMode = Vortice.Direct2D1.BitmapInterpolationMode;
using RRect = RabotoraX.Core.Mathematics.Rect;

namespace RabotoraX.Interop.Direct3D12;

public partial class DirectX12
{
    private sealed partial class D2DContextImpl
    {
        private readonly DirectX12 _parent;
        private readonly Dictionary<Color4, ID2D1SolidColorBrush> _brushCache = new();
        private readonly Dictionary<(string font, float size), IDWriteTextFormat> _formatCache = new();
        private ID2D1DeviceContext D2DContext => _parent._d2dContext!;
        private INativeShader? _currentShader;
		
        public D2DContextImpl(DirectX12 parent)
        {
            _parent = parent;
        }

        public void BeginDraw()
        {
            // 关键：向 DX12 借用主纹理的写入权
            _parent._d3d11On12Device!.AcquireWrappedResources(new[] { _parent._wrappedBackBuffer! });
            
            D2DContext.Target = _parent._d2dTargetBitmap;
            D2DContext.Transform = Matrix3x2.Identity;
            D2DContext.SetDpi(96, 96);
            D2DContext.BeginDraw();
        }

        public void EndDraw()
        {
            D2DContext.EndDraw();
            
            // 关键：把纹理的控制权还给 DX12 队列
            _parent._d3d11On12Device!.ReleaseWrappedResources(new[] { _parent._wrappedBackBuffer! });
            _parent._d3d11Context!.Flush(); // 必须 Flush 让 DX11 提交命令到 DX12 队列
        }

        public void Clear(float r, float g, float b, float a) => D2DContext.Clear(new Color4(r, g, b, a));

        public void DrawRectangle(float x, float y, float w, float h, float r, float g, float b, float a, float stroke)
        {
            var rect = new RawRectF(x, y, x + w, y + h);
            D2DContext.DrawRectangle(rect, GetCachedBrush(r, g, b, a), stroke);
        }

        public void FillRectangle(float x, float y, float w, float h, float r, float g, float b, float a)
        {
            var rect = new RawRectF(x, y, x + w, y + h);
            D2DContext.FillRectangle(rect, GetCachedBrush(r, g, b, a));
        }

        public void DrawImage(INativeTexture2D texture, float x, float y, float w, float h, float opacity = 1)
        {
            if (texture is not D2DTexture d2dTex) return;
            D2DContext.DrawBitmap(d2dTex.Bitmap, opacity, BitmapInterpolationMode.Linear, new Rect(x, y, w, h));
        }

        public void DrawImage(INativeTexture2D texture, RRect srcRect, float x, float y, float w, float h, float opacity = 1)
        {
            if (texture is not D2DTexture d2dTex) return;
            var destRect = new Rect(x, y, w, h); 
            var src = new RawRectF(srcRect.X, srcRect.Y, srcRect.X + srcRect.Width, srcRect.Y + srcRect.Height);
            D2DContext.DrawBitmap(d2dTex.Bitmap, destRect, opacity, BitmapInterpolationMode.Linear, src);
        }

        public void DrawText(string text, string fontName, float fontSize, float x, float y, float r, float g, float b, float a)
        {
            D2DContext.DrawText(text, GetCachedFormat(fontName, fontSize), new Rect(x, y, 10000, 10000), GetCachedBrush(r, g, b, a));
        }

        public void DrawTextLayout(INativeTextLayout layout, float x, float y, float r, float g, float b, float a)
        {
            if (layout is not D2DTextLayout d2dLayout) return;
            D2DContext.DrawTextLayout(new Vector2(x, y), d2dLayout.InternalLayout, GetCachedBrush(r, g, b, a), DrawTextOptions.None);
        }

        public void SetTransform(Matrix3x2 matrix) => D2DContext.Transform = matrix;
        public void SetShader(INativeShader? shader) => _currentShader = shader; // 完美兼容原来的 Custom Shader

        public INativeTexture2D CreateTexture(Stream stream, bool leaveOpen = false)
        {
            try
            {
                using var decoder = _parent._wicFactory!.CreateDecoderFromStream(stream);
                using var frame = decoder.GetFrame(0);
                using var converter = _parent._wicFactory.CreateFormatConverter();
                converter.Initialize(frame, Vortice.WIC.PixelFormat.Format32bppPBGRA);
                var bitmap = D2DContext.CreateBitmapFromWicBitmap(converter, new BitmapProperties(new Vortice.DCommon.PixelFormat(Vortice.DXGI.Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied)));
                return new D2DTexture(bitmap);
            }
            finally { if (!leaveOpen) stream.Dispose(); }
        }

        public unsafe INativeTexture2D CreateTexture(int width, int height, ReadOnlyMemory<byte> pixelData)
        {
            var props = new BitmapProperties(new Vortice.DCommon.PixelFormat(Vortice.DXGI.Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied));
            fixed (byte* pData = pixelData.Span)
            {
                var bitmap = D2DContext.CreateBitmap(new SizeI(width, height), (nint)pData, (uint)(width * 4), props);
                return new D2DTexture(bitmap);
            }
        }

        public unsafe INativeTexture2D CreateVideoTexture(int width, int height, ReadOnlyMemory<byte>? initialData = null)
        {
            var props = new BitmapProperties(new Vortice.DCommon.PixelFormat(Vortice.DXGI.Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Ignore));
            if (initialData.HasValue)
            {
                fixed (byte* pData = initialData.Value.Span)
                    return new D2DTexture(D2DContext.CreateBitmap(new SizeI(width, height), (nint)pData, (uint)(width * 4), props));
            }
            return new D2DTexture(D2DContext.CreateBitmap(new SizeI(width, height), props));
        }

        public INativeTexture2D CreateEmptyTexture(int width, int height) => 
            new D2DTexture(D2DContext.CreateBitmap(new SizeI(width, height), new BitmapProperties(new Vortice.DCommon.PixelFormat(Vortice.DXGI.Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied))));

        public INativeTextLayout CreateTextLayout(string text, string fontName, float fontSize, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue) => 
            new D2DTextLayout(_parent._dwriteFactory!.CreateTextLayout(text, GetCachedFormat(fontName, fontSize), maxWidth, maxHeight));

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
            foreach (var brush in _brushCache.Values) brush.Dispose();
            foreach (var format in _formatCache.Values) format.Dispose();
            _brushCache.Clear();
            _formatCache.Clear();
        }
    }
}