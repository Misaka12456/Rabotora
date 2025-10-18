using Rabotora.Core;
using Vortice.Direct3D11;
using Vortice.DirectWrite;
using Color = Vortice.Mathematics.Color;
using Rectangle = Rabotora.Core.Rectangle;
using FontWeight = Rabotora.Core.FontWeight;

namespace Rabotora.Graphics;

public interface IRenderContext
{
	void BeginFrame();
	void EndFrame();
	void DrawTexture(ITexture2D texture, Rectangle rect, float alpha = 1.0f);
	void Clear(Color color);
	void DrawText(string text, IFont font, Color color, Point position, TextAlign align = TextAlign.Left, float alpha = 1.0f);
	void SetRenderTarget(object renderTarget);
	ITexture2D LoadTexture(string path);
	IFont CreateFont(string fontFamily, float size, FontWeight weight = FontWeight.Regular);
}

public interface IObject : IDisposable
{
	object NativeData { get; }
}

public interface ITexture2D : IObject
{
	int Width { get; }
	int Height { get; }
	object NativeTexture => NativeData;
}

public interface IFont : IObject
{
	string FontFamily { get; }
	float Size { get; }
	FontWeight Weight { get; }
	object NativeFont => NativeData;
}

public class DXTexture2D : ITexture2D
{
	public int Width { get; }
	public int Height { get; }
	public object NativeData => _texture ?? throw new InvalidOperationException("Texture is not initialized.");

	private readonly ID3D11ShaderResourceView? _resourceView;
	private readonly ID3D11Texture2D? _texture;
	private bool _disposed;

	public DXTexture2D(ID3D11Texture2D nativeTex, ID3D11ShaderResourceView resView)
	{
		_texture = nativeTex;
		_resourceView = resView;

		var desc = nativeTex.Description;
		Width = (int)desc.Width;
		Height = (int)desc.Height;
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_resourceView?.Dispose();
			_texture?.Dispose();
			_disposed = true;
		}
		GC.SuppressFinalize(this);
	}
	
	~DXTexture2D()
	{
		Dispose();
	}
}

public class DXFont : IFont
{
	public string FontFamily { get; }
	public float Size { get; }
	public FontWeight Weight { get; }
	public object NativeData => _textFormat ?? throw new InvalidOperationException("Font format is not initialized.");
	
	private readonly IDWriteTextFormat? _textFormat;
	private bool _disposed;
	
	public DXFont(IDWriteTextFormat textFormat, string fontFamily, float size, FontWeight weight)
	{
		_textFormat = textFormat;
		FontFamily = fontFamily;
		Size = size;
		Weight = weight;
	}
	
	public void Dispose()
	{
		if (!_disposed)
		{
			_textFormat?.Dispose();
			_disposed = true;
		}
		GC.SuppressFinalize(this);
	}

	~DXFont()
	{
		Dispose();
	}
}

public interface IDrawable
{
	void Draw(IRenderContext ctx);
}