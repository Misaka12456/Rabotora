using System.Numerics;

namespace RabotoraX.Core.UI;

public interface INativeTextLayout : IDisposable
{
	Vector2 Size { get; }
}