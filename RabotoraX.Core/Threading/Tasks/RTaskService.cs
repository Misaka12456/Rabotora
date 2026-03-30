namespace RabotoraX.Core.Threading.Tasks;

/// <summary>
/// Provides a service for managing and executing update actions related to all <see cref="RTask"/> and <see cref="RTask{T}"/> instances on the main thread (RabotoraX Render Thread).
/// </summary>
public static class RTaskService
{
	private readonly static List<Action<float>> _updateActions = [];
	private readonly static List<Action<float>> _actionsToAdd = [];
	
	internal static void RegisterUpdateAction(Action<float> action)
	{
		lock (_actionsToAdd)
		{
			_actionsToAdd.Add(action);
		}
	}

	internal static void RemoveAction(Action<float> action)
	{
		_updateActions.Remove(action);
	}

	public static void Update(float deltaTime)
	{
		lock (_actionsToAdd)
		{
			if (_actionsToAdd.Count > 0)
			{
				_updateActions.AddRange(_actionsToAdd);
				_actionsToAdd.Clear();
			}
		}

		for (int i = _updateActions.Count - 1; i >= 0; i--)
		{
			try
			{
				// ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
				_updateActions[i]?.Invoke(deltaTime);
			}
			catch
			{
				_updateActions.RemoveAt(i);
				throw;
			}
		}
	}
}