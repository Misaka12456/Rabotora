using Vortice.Mathematics;

namespace RabotoraEngine.Graphics.Internal;

public interface IRenderContext : IDisposable
{
	void BeginFrame();
	void EndFrame();
	void Clear(Color color);
	void SetRenderTarget(object renderTarget);
}