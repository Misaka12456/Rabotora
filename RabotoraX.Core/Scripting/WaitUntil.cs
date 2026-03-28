namespace RabotoraX.Core.Scripting;

public sealed class WaitUntil : RYieldData
{
	private readonly Func<bool> _predicate;
	public WaitUntil(Func<bool> predicate)
	{
		_predicate = predicate;
	}

	public override bool KeepWaiting(float deltaTime)
	{
		return !_predicate();
	}
}