using RabotoraEngine.Graphics.Internal;

namespace RabotoraEngine.Graphics;

public interface IRenderable
{
	void Render(IRenderContext ctx);
}