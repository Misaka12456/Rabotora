using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace RabotoraX.Core.Tweening;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static partial class RaTweenExtensions
{
	extension(RObject target)
	{
		public RaTweenCore<Vector3> RaMove(Vector3 to, float duration)
		{
			return RaTween.To(target, DefaultPlugins.Vector3, () => target.Layout.Position, v => target.Layout.Position = v, to, duration);
		}

		public RaTweenCore<Vector3> RaMoveX(float to, float duration)
		{
			var prevPos = target.Layout.Position;
			return RaTween.To(target, DefaultPlugins.Vector3, () => target.Layout.Position, v => target.Layout.Position = new Vector3(v.X, target.Layout.Position.Y, target.Layout.Position.Z),
				prevPos with { X = to }, duration);
		}

		public RaTweenCore<Vector3> RaMoveY(float to, float duration)
		{
			var prevPos = target.Layout.Position;
			return RaTween.To(target, DefaultPlugins.Vector3, () => target.Layout.Position, v => target.Layout.Position = new Vector3(target.Layout.Position.X, v.Y, target.Layout.Position.Z),
				prevPos with { Y = to }, duration);
		}

		public RaTweenCore<Vector3> RaMoveZ(float to, float duration)
		{
			var prevPos = target.Layout.Position;
			return RaTween.To(target, DefaultPlugins.Vector3, () => target.Layout.Position, v => target.Layout.Position = new Vector3(target.Layout.Position.X, target.Layout.Position.Y, v.Z),
				prevPos with { Z = to }, duration);
		}

		public RaTweenCore<Vector3> RaScale(Vector3 to, float duration)
		{
			return RaTween.To(target, DefaultPlugins.Vector3, () => target.Layout.Scale, v => target.Layout.Scale = v, to, duration);
		}

		public RaTweenCore<Vector3> RaScaleX(float to, float duration)
		{
			var prevScale = target.Layout.Scale;
			return RaTween.To(target, DefaultPlugins.Vector3, () => target.Layout.Scale, v => target.Layout.Scale = new Vector3(v.X, target.Layout.Scale.Y, target.Layout.Scale.Z),
				prevScale with { X = to }, duration);
		}

		public RaTweenCore<Vector3> RaScaleY(float to, float duration)
		{
			var prevScale = target.Layout.Scale;
			return RaTween.To(target, DefaultPlugins.Vector3, () => target.Layout.Scale, v => target.Layout.Scale = new Vector3(target.Layout.Scale.X, v.Y, target.Layout.Scale.Z),
				prevScale with { Y = to }, duration);
		}

		public RaTweenCore<Vector3> RaScaleZ(float to, float duration)
		{
			var prevScale = target.Layout.Scale;
			return RaTween.To(target, DefaultPlugins.Vector3, () => target.Layout.Scale, v => target.Layout.Scale = new Vector3(target.Layout.Scale.X, target.Layout.Scale.Y, v.Z),
				prevScale with { Z = to }, duration);
		}
	}
}