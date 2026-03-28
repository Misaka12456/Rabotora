namespace RabotoraX.Core.Tweening;

/// <summary>
/// Represents a plugin that defines how to interpolate between two values of type T. This allows RaTween to support tweening of various types (e.g., float, Vector2, Vector3) by providing the appropriate interpolation logic for each type.
/// </summary>
/// <typeparam name="T">The type of value to be tweened (e.g., float, Vector2, Vector3).</typeparam>
public interface ITweenPlugin<T>
{
	T Interpolate(T from, T to, float t);
}