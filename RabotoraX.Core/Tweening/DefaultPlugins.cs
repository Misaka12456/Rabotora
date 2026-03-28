using System.Diagnostics.CodeAnalysis;

namespace RabotoraX.Core.Tweening;

/// <summary>
/// Provides default RaTween plugins for common types.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal static class DefaultPlugins
{
	public readonly static FloatPlugin Float = new();
	public readonly static Vector2Plugin Vector2 = new();
	public readonly static Vector3Plugin Vector3 = new();
	public readonly static Vector4Plugin Vector4 = new();
}