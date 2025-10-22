using System.Drawing;
using RabotoraEngine.Graphics.Internal;

namespace RabotoraEngine.Graphics.Components;

public abstract class UIComponent : IObject, IRenderable
{
	public bool IsVisible { get; set; } = true;
	public Rectangle Bounds { get; set; }
	public abstract object NativeData { get; }

	public virtual void Update(float deltaTime)
	{
		/* Logic update */
	}
	
	public virtual void PostUpdate(float deltaTime)
	{
		/* Post logic update */
	}
	
	public abstract void Render(IRenderContext ctx);
	public abstract void Dispose();
}