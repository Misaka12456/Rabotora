using System.Collections;
using RabotoraX.Core.Scripting;

namespace RabotoraX.Core.Threading.Tasks;

public partial struct RTask
{
	/// <summary>
	/// Instantiate a new <see cref="RTask"/> using the provided factory delegate.
	/// </summary>
	/// <param name="factory">A delegate that creates and returns a new <see cref="RTask"/> instance.</param>
	/// <returns>The <see cref="RTask"/> instance created by the factory delegate.</returns>
	public static RTask Create(Func<RTask> factory)
	{
		return factory();
	}

	/// <summary>
	/// Instantiates a new <see cref="RTask"/> that completes after a specified delay in seconds.
	/// </summary>
	/// <param name="delay">The delay duration in seconds before the task completes.</param>
	/// <returns>A <see cref="RTask"/> that completes after the specified delay.</returns>
	/// <seealso cref="Delay(int)"/>
	public static RTask Delay(TimeSpan delay)
	{
		return Delay((float)delay.TotalSeconds);
	}

	/// <summary>
	/// Instantiates a new <see cref="RTask"/> that completes after a specified delay in seconds.
	/// </summary>
	/// <param name="milliseconds">The delay duration in milliseconds before the task completes.</param>
	/// <returns>A <see cref="RTask"/> that completes after the specified delay.</returns>
	/// <seealso cref="Delay(TimeSpan)"/>
	public static RTask Delay(int milliseconds)
	{
		return Delay(milliseconds / 1000f);
	}

	private static RTask Delay(float seconds)
	{
		var tcs = RTaskPool<RTaskCompletionSource>.Pick();
		float elapsed = 0f;
		Action<float> updateAction = null!;

		updateAction = dt =>
		{
			elapsed += dt;
			if (elapsed >= seconds)
			{
				RTaskService.RemoveAction(updateAction);
				tcs.TrySetResult();
			}
		};
		
		RTaskService.RegisterUpdateAction(updateAction);
		return new RTask(tcs, tcs.Version);
	}
	
	/// <summary>
	/// Instantiates a new <see cref="RTask"/> that completes when the provided predicate returns true. The predicate is evaluated every frame.
	/// </summary>
	/// <param name="predicate">A delegate that returns a boolean value. The task will complete when this delegate returns <see langword="true"/>.</param>
	/// <returns>A <see cref="RTask"/> that completes when the provided predicate returns <see langword="true"/>.</returns>
	public static RTask WaitUntil(Func<bool> predicate)
	{
		var tcs = RTaskPool<RTaskCompletionSource>.Pick();
		Action<float> updateAction = null!;

		updateAction = _ =>
		{
			if (predicate())
			{
				RTaskService.RemoveAction(updateAction);
				tcs.TrySetResult();
			}
		};
		
		RTaskService.RegisterUpdateAction(updateAction);
		return new RTask(tcs, tcs.Version);
	}

	/// <summary>
	/// Instantiates a new <see cref="RTask"/> that completes at the next iteration of the specified script loop timing. By default, it completes at the next Update loop.
	/// </summary>
	/// <param name="timing">The script loop timing at which the task should complete.</param>
	/// <returns>A <see cref="RTask"/> that completes at the next iteration of the specified script loop timing.</returns>
	public static RTask Yield(RScriptLoopTiming timing = RScriptLoopTiming.Update)
	{
		var tcs = RTaskPool<RTaskCompletionSource>.Pick();
		Action<float> updateAction = null!;
		updateAction = _ =>
		{
			RTaskService.RemoveAction(updateAction);
			tcs.TrySetResult();
		};
		
		RTaskService.RegisterUpdateAction(updateAction);
		return new RTask(tcs, tcs.Version);
	}

	internal static RTask FromIEnumerator(IEnumerator routine)
	{
		var tcs = RTaskPool<RTaskCompletionSource>.Pick();
		Action<float> updateAction = null!;

		updateAction = dt =>
		{
			bool shouldMoveNext = true;
			if (routine.Current is RYieldData yieldData)
			{
				shouldMoveNext = !yieldData.KeepWaiting(dt);
			}

			if (shouldMoveNext && !routine.MoveNext())
			{
				RTaskService.RemoveAction(updateAction);
				tcs.TrySetResult();
			}
		};
		
		RTaskService.RegisterUpdateAction(updateAction);
		return new RTask(tcs, tcs.Version);
	}
}