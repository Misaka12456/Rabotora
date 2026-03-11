using System.Numerics;

namespace RabotoraX.Core.Inputs;

public sealed class KeyboardState
{
	private readonly bool[] _physical = new bool[256];
	private readonly bool[] _current = new bool[256];
	private readonly bool[] _previous = new bool[256];
	
	public void SetPhysicalState(int key, bool isPressed)
	{
		if (key < 0 || key >= _physical.Length)
			return;

		_physical[key] = isPressed;
	}

	public void AdvanceFrame()
	{
		Array.Copy(_current, _previous, _current.Length);
		Array.Copy(_physical, _current, _current.Length);
	}
	
	public bool GetKey(KeyCode key) => _current[(int)key];
	public bool GetKeyDown(KeyCode key) => _current[(int)key] && !_previous[(int)key];
	public bool GetKeyUp(KeyCode key) => !_current[(int)key] && _previous[(int)key];
	
	public bool IsAnyKeyActionActive()
	{
		foreach (bool t in _current)
		{
			if (t) return true;
		}
		
		return false;
	}
}

public sealed class MouseState
{
	private readonly bool[] _physical = new bool[8];
	private readonly bool[] _current = new bool[8];
	private readonly bool[] _previous = new bool[8];
	public Vector2 Position;
	
	public void SetPhysicalState(int button, bool isPressed)
	{
		if (button < 0 || button >= _physical.Length)
			return;

		_physical[button] = isPressed;
	}

	public void AdvanceFrame(int x, int y)
	{
		Position = new Vector2(x, y);
		Array.Copy(_current, _previous, _current.Length);
		Array.Copy(_physical, _current, _current.Length);
	}
	
	public bool GetButton(int i) => _current[i];
	public bool GetButtonDown(int i) => _current[i] && !_previous[i];
	public bool GetButtonUp(int i) => !_current[i] && _previous[i];
	
	public bool IsAnyButtonActionActive()
	{
		foreach (bool t in _current)
		{
			if (t) return true;
		}

		return false;
	}
}