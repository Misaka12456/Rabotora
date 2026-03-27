namespace RabotoraX.Core.UI;

public interface INativeTextFactory
{
	INativeTextLayout CreateTextLayout(string text, string fontName, float fontSize, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue);
}