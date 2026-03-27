using System;
using System.Collections.Concurrent;
using RabotoraX.Core.UI;
using RabotoraX.Interop.Direct3D11.Rendering;
using Vortice.DirectWrite;

namespace RabotoraX.Interop.Direct3D11;

public partial class DirectX11 : INativeTextFactory
{
	private readonly ConcurrentDictionary<(string, float), IDWriteTextFormat> _textFormatCache = [];
	
	public INativeTextLayout CreateTextLayout(string text, string fontName, float fontSize, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue)
	{
		if (fontSize <= 0.1f) fontSize = 0.1f;
		float roundedSize = MathF.Round(fontSize, 2);
		var format = _textFormatCache.GetOrAdd((fontName, roundedSize), key => _dwriteFactory!.CreateTextFormat(key.Item1, key.Item2));
		var layout = _dwriteFactory!.CreateTextLayout(text, format, maxWidth, maxHeight);
		
		return new D2DTextLayout(layout);
	}
}