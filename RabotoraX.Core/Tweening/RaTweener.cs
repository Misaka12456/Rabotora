using System.Diagnostics.CodeAnalysis;
using RabotoraX.Core.Scripting;

namespace RabotoraX.Core.Tweening;

/// <summary>
/// Represents the base class for all RaTween instances.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public abstract class RaTweener : RYieldData
{
	public float Duration { get; protected set; }
	public float Elapsed { get; protected set; }
	public bool IsActive { get; protected set; } = true;

	protected Ease _easeType = Ease.Linear;
	protected EaseFunction? _customEase;

	protected Action? _onComplete;
	protected Action<float>? _onUpdate; // "float" parameter is the eased progress (0 to 1)

	public void Kill()
	{
		IsActive = false;
	}
}