using System.Numerics;

namespace RabotoraX.Core.Inputs;

/// <summary>
/// Represents RaboInput, the input system of RabotoraX. It provides methods to query the state of keyboard and mouse inputs.
/// </summary>
public static class Input
{
	private static INativeInput _backend = null!;
	
	/// <summary>
	/// Current mouse position in screen coordinates, where (0, 0) is the top-left corner of the window and (window width, window height) is the bottom-right corner.
	/// </summary>
	public static Vector2 MousePosition => _backend.MousePosition;
	
	/// <summary>
	/// Indicates whether any key is being pressed down in the current frame.<br />
	/// This property will check both the keyboard and mouse inputs, and will return <see langword="true"/> if at least one key or mouse button is currently pressed down; otherwise, it will return <see langword="false"/>.
	/// </summary>
	public static bool AnyKeyDown => _backend.AnyKeyDown;
	
	/// <summary>
	/// Initializes the input system with the specified native input backend.<br />
	/// This method must be called before using any other methods of the <see cref="Input"/> class to ensure that the input system is properly set up and ready to handle input queries.
	/// </summary>
	/// <param name="backend">The native input backend to use for handling input queries.</param>
	public static void Initialize(INativeInput backend)
	{
		_backend = backend;
	}

	/// <summary>
	/// Updates the input state.<br />
	/// This method should be called once per frame, typically at the beginning of the frame, to ensure that the input state is up-to-date for the rest of the frame's processing.
	/// </summary>
	internal static void Update()
	{
		_backend.Update();
	}
	
	/// <summary>
	/// Checks if the specified key is currently being pressed down.
	/// </summary>
	/// <param name="key">The <see cref="KeyCode"/> representing the key to check.</param>
	/// <returns><see langword="true"/> if the key is currently pressed down; otherwise, <see langword="false"/>.</returns>
	public static bool GetKey(KeyCode key) => _backend.GetKey(key);
	
	/// <summary>
	/// Checks if the specified key was pressed down during the current frame.<br />
	/// This will only return <see langword="true"/> on the exact frame when the key transitions from an up state to a down state.
	/// </summary>
	/// <param name="key">The <see cref="KeyCode"/> representing the key to check.</param>
	/// <returns><see langword="true"/> if the key was pressed down during the current frame; otherwise, <see langword="false"/>.</returns>
	public static bool GetKeyDown(KeyCode key) => _backend.GetKeyDown(key);
	
	/// <summary>
	/// Checks if the specified key was released during the current frame.
	/// </summary>
	/// <param name="key">The <see cref="KeyCode"/> representing the key to check.</param>
	/// <returns><see langword="true"/> if the key was released during the current frame; otherwise, <see langword="false"/>.</returns>
	public static bool GetKeyUp(KeyCode key) => _backend.GetKeyUp(key);
	
	/// <summary>
	/// Checks if the specified mouse button is currently being pressed down.
	/// </summary>
	/// <param name="button">The index of the mouse button to check. Typically, 0 is the left button, 1 is the right button, and 2 is the middle button.</param>
	/// <returns><see langword="true"/> if the mouse button is currently pressed down; otherwise, <see langword="false"/>.</returns>
	public static bool GetMouseButton(int button) => _backend.GetMouseButton(button);
	
	/// <summary>
	/// Checks if the specified mouse button was pressed down during the current frame.<br />
	/// This will only return <see langword="true"/> on the exact frame when the mouse button transitions from an up state to a down state.
	/// </summary>
	/// <param name="button">The index of the mouse button to check. Typically, 0 is the left button, 1 is the right button, and 2 is the middle button.</param>
	/// <returns><see langword="true"/> if the mouse button was pressed down during the current frame; otherwise, <see langword="false"/>.</returns>
	public static bool GetMouseButtonDown(int button) => _backend.GetMouseButtonDown(button);
	
	/// <summary>
	/// Checks if the specified mouse button was released during the current frame.
	/// </summary>
	/// <param name="button">The index of the mouse button to check. Typically, 0 is the left button, 1 is the right button, and 2 is the middle button.</param>
	/// <returns><see langword="true"/> if the mouse button was released during the current frame; otherwise, <see langword="false"/>.</returns>
	public static bool GetMouseButtonUp(int button) => _backend.GetMouseButtonUp(button);
}