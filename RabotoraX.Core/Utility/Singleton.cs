using System.Diagnostics.CodeAnalysis;
using RabotoraX.Core.Scripting;

namespace RabotoraX.Core.Utility;

[SuppressMessage("Usage", "RATSG002: Generic type 'Singleton<T>' is marked as serializable but lacks generic [RBinarySerializable<T>] attributes. Native AOT Source Generation requires concrete type information for generic types.")]
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