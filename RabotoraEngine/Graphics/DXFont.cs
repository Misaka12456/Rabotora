using Vortice.DirectWrite;

namespace RabotoraEngine.Graphics;

public class DXFont : IFont
{
	public string FontFamily { get; }
	public float Size { get; }
	public FontWeight Weight { get; }
	public object NativeData => _textFormat ?? throw new InvalidOperationException("Font is not initialized or already disposed.");
	
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