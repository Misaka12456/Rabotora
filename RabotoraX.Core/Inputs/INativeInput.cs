using System.Numerics;

namespace RabotoraX.Core.Inputs;

public interface INativeInput : IDisposable
{
	Vector2 MousePosition { get; }
	bool AnyKeyDown { get; }
	Vector2 MouseDelta => throw new PlatformNotSupportedException("MouseDelta is not supported by this input backend.");
	
	void Update();

	bool GetKey(KeyCode key);
	bool GetKeyDown(KeyCode key);
	bool GetKeyUp(KeyCode key);
	
	bool GetMouseButton(int button);
	bool GetMouseButtonDown(int button);
	bool GetMouseButtonUp(int button);
}