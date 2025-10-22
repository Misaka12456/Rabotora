using RabotoraEngine.Graphics.Internal;

namespace RabotoraEngine.Graphics.SceneManagement;

public interface IScene : IDisposable
{
	void Load(IRenderContext context);

	void Unload();
	
	void Update(float deltaTime);
	
	void Render(IRenderContext context);
}