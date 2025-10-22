using System.Diagnostics;
using RabotoraEngine.Graphics.Internal;

namespace RabotoraEngine.Graphics.SceneManagement;

public class SceneManager : IDisposable
{
	private readonly IRenderContext _context;
	private IScene? _currentScene;
	private IScene? _nextScene;
	private bool _isLoading = false;

	public SceneManager(IRenderContext context)
	{
		_context = context;
	}

	public void SwitchScene(IScene newScene)
	{
		if (_nextScene != null && _nextScene != _currentScene)
		{
#if DEBUG
			Debug.WriteLine("Scene change already in progress. It is not recommended to queue multiple scene changes.", "Warning");
#endif
			_nextScene?.Dispose();
		}

		_nextScene = newScene;
	}

	private void PerformSceneTransition()
	{
		if (_isLoading) return;
		if (_nextScene == null || _nextScene == _currentScene)
		{
			return;
		}

		_isLoading = true;

		_currentScene?.Unload();
		_currentScene?.Dispose();

		_currentScene = _nextScene;
		_nextScene = null;

		_currentScene.Load(_context);

		_isLoading = false;
	}

	public void Update(float deltaTime)
	{
		PerformSceneTransition();

		if (_isLoading) return;
		_currentScene?.Update(deltaTime);
	}

	public void Render()
	{
		if (_isLoading) return;
		_currentScene?.Render(_context);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_currentScene?.Unload();
			_currentScene?.Dispose();
			_nextScene?.Dispose();
		}
		_currentScene = null;
		_nextScene = null;
	}
	
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	
	~SceneManager()
	{
		Dispose(false);
	}
}