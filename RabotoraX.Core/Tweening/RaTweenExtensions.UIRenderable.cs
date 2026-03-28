using System.Diagnostics.CodeAnalysis;
using RabotoraX.Core.UI;

namespace RabotoraX.Core.Tweening;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static partial class RaTweenExtensions
{
	extension(UIRenderable target)
	{
		public RaTweenCore<float> RaOpacity(float to, float duration)
		{
			return RaTween.To(target.RObject, DefaultPlugins.Float, () => target.Opacity, v => target.Opacity = v, to, duration);
		}
	}
}