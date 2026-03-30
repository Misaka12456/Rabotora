using RabotoraX.Core.Scripting;

namespace RabotoraX.Core.Utility;

public abstract class Singleton<T> : RManagedScript where T : Singleton<T>
{
	public bool AllowRepeatInit { get; protected set; } = false;
	public static T Instance { get; private set; } = null!;
	
	public override void OnAwake()
	{
		SingletonAwake();
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (!AllowRepeatInit && Instance != null)
		{
			Dispose();
			return;
		}
		Instance = (T)this;
	}
	
	protected virtual void SingletonAwake()
	{
		
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			IsEnabled = false;
		}

		if (Instance == this)
		{
			Instance = null!;
		}
		base.Dispose(disposing);
	}
}