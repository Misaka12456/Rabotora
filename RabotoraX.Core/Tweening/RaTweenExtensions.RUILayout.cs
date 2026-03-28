using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using RabotoraX.Core.UI;

namespace RabotoraX.Core.Tweening;

/// <summary>
/// Provides extension methods for <see cref="RObject"/> and related classes to easily create RaTween instances for common properties.
/// </summary>
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static partial class RaTweenExtensions
{
	extension(RUILayout target)
	{
		public RaTweenCore<Vector2> RaMove(Vector2 to, float duration)
		{
			return RaTween.To(target.RObject, DefaultPlugins.Vector2, () => target.AnchoredPosition, v => target.AnchoredPosition = v, to, duration);
		}

		public RaTweenCore<Vector2> RaMoveX(float to, float duration)
		{
			var prevPos = target.AnchoredPosition;
			return RaTween.To(target.RObject, DefaultPlugins.Vector2, () => target.AnchoredPosition, v => target.AnchoredPosition = new Vector2(v.X, target.AnchoredPosition.Y),
				prevPos with { X = to }, duration);
		}

		public RaTweenCore<Vector2> RaMoveY(float to, float duration)
		{
			var prevPos = target.AnchoredPosition;
			return RaTween.To(target.RObject, DefaultPlugins.Vector2, () => target.AnchoredPosition, v => target.AnchoredPosition = new Vector2(target.AnchoredPosition.X, v.Y),
				prevPos with { Y = to }, duration);
		}

		public RaTweenCore<Vector2> RaSize(Vector2 to, float duration)
		{
			return RaTween.To(target.RObject, DefaultPlugins.Vector2, () => target.Size, v => target.Size = v, to, duration);
		}

		public RaTweenCore<Vector2> RaSizeX(float to, float duration)
		{
			var prevSize = target.Size;
			return RaTween.To(target.RObject, DefaultPlugins.Vector2, () => target.Size, v => target.Size = new Vector2(v.X, target.Size.Y),
				prevSize with { X = to }, duration);
		}

		public RaTweenCore<Vector2> RaSizeY(float to, float duration)
		{
			var prevSize = target.Size;
			return RaTween.To(target.RObject, DefaultPlugins.Vector2, () => target.Size, v => target.Size = new Vector2(target.Size.X, v.Y),
				prevSize with {Y = to}, duration);
		}

		public RaTweenCore<Vector2> RaPivot(Vector2 to, float duration)
		{
			return RaTween.To(target.RObject, DefaultPlugins.Vector2, () => target.Pivot, v => target.Pivot = v, to, duration);
		}

		public RaTweenCore<Vector2> RaPivotX(float to, float duration)
		{
			var prevPivot = target.Pivot;
			return RaTween.To(target.RObject, DefaultPlugins.Vector2, () => target.Pivot, v => target.Pivot = new Vector2(v.X, target.Pivot.Y),
				prevPivot with { X = to }, duration);
		}

		public RaTweenCore<Vector2> RaPivotY(float to, float duration)
		{
			var prevPivot = target.Pivot;
			return RaTween.To(target.RObject, DefaultPlugins.Vector2, () => target.Pivot, v => target.Pivot = new Vector2(target.Pivot.X, v.Y),
				prevPivot with {Y = to}, duration);
		}
		
		public RaTweenCore<Vector2> RaScale(Vector2 to, float duration)
		{
			return RaTween.To(target.RObject, DefaultPlugins.Vector2, () => target.Scale, v => target.Scale = v, to, duration);
		}
		
		public RaTweenCore<Vector2> RaScaleX(float to, float duration)
		{
			var prevScale = target.Scale;
			return RaTween.To(target.RObject, DefaultPlugins.Vector2, () => target.Scale, v => target.Scale = new Vector2(v.X, target.Scale.Y),
				prevScale with { X = to }, duration);
		}

		public RaTweenCore<Vector2> RaScaleY(float to, float duration)
		{
			var prevScale = target.Scale;
			return RaTween.To(target.RObject, DefaultPlugins.Vector2, () => target.Scale, v => target.Scale = new Vector2(target.Scale.X, v.Y),
				prevScale with {Y = to}, duration);
		}
	}
}