namespace RabotoraX.Core.Scripting;

public sealed class WaitForSeconds : RYieldData
{
	private float _timeLeft;
	public WaitForSeconds(float seconds)
	{
		_timeLeft = seconds;
	}

	public override bool KeepWaiting(float deltaTime)
	{
		_timeLeft -= deltaTime;
		return _timeLeft > 0;
	}
}