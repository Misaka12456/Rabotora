using Rectangle = Rabotora.Core.Rectangle;

namespace Rabotora.Graphics.Components;

public abstract class UIComponent : IDrawable
{
	public bool IsVisible { get; set; } = true;
	public Rectangle Bounds { get; set; }

	public virtual void Update(float deltaTime) { /* 逻辑更新 */  }
	public abstract void Draw(IRenderContext context);
}