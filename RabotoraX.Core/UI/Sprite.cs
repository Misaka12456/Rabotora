using RabotoraX.Core.Graphics;
using RabotoraX.Core.Mathematics;

namespace RabotoraX.Core.UI;

public class Sprite : Object
{
	public Texture2D Texture { get; }
	public INativeTexture2D NativeTexture => Texture.NativeTexture;
	public Rect SourceRect { get; init; }

	private Sprite(Texture2D texture)
	{
		Texture = texture;
		SourceRect = new Rect(0, 0, texture.Width, texture.Height);
	}
	
	public static Sprite Create(Texture2D texture)
	{
		return new Sprite(texture);
	}
	
	public static Sprite Create(Texture2D texture, Rect sourceRect)
	{
		return new Sprite(texture)
		{
			SourceRect = sourceRect
		};
	}
	
	public static Sprite Create(Texture2D texture, float x, float y, float width, float height)
	{
		return new Sprite(texture)
		{
			SourceRect = new Rect(x, y, width, height)
		};
	}
}