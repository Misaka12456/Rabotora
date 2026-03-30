using System.Collections;
using System.Diagnostics.CodeAnalysis;
using RabotoraX.Core.Scripting;
using RabotoraX.Core.Tweening;

namespace RabotoraX.Core.Threading.Tasks;

/// <summary>
/// Provides extension methods for <see cref="RTask"/> and <see cref="RTask{T}"/> to enhance their usability and integration with other parts of the framework, such as coroutines and tweening.
/// </summary>
public static class RTaskExtensions
{
	/// <summary>
	/// Fires and forgets the given <see cref="RTask"/>, whatever the result is.
	/// </summary>
	/// <param name="task">The <see cref="RTask"/> to be fired and forgotten.</param>
	/// <seealso cref="Forget{T}(RTask{T})"/>
	public static async void Forget(this RTask task)
	{
		try
		{
			await task;
		}
		catch (Exception ex)
		{
#if DEBUG && RABOTORA_STRICT
			Console.WriteLine("[RTask] Unobserved exception in Forget(): " + ex);
#endif
		}
	}

	/// <summary>
	/// Fires and forgets the given <see cref="RTask{T}"/>, whatever the result is.
	/// </summary>
	/// <param name="task">The <see cref="RTask{T}"/> to be fired and forgotten.</param>
	/// <typeparam name="T">The type of the result produced by the <see cref="RTask{T}"/>.</typeparam>
	/// <seealso cref="Forget(RTask)"/>
	public static async void Forget<T>(this RTask<T> task)
	{
		try
		{
			await task;
		}
		catch (Exception ex)
		{
#if DEBUG && RABOTORA_STRICT
			Console.WriteLine("[RTask] Unobserved exception in Forget<T>(): " + ex);
#endif
		}
	}

	/// <summary>
	/// Fires and forgets the given <see cref="System.Threading.Tasks.Task"/>, whatever the result is.
	/// </summary>
	/// <param name="clrTask">The <see cref="System.Threading.Tasks.Task"/> to be fired and forgotten.</param>
	public static void Forget(this Task clrTask)
	{
		clrTask.ContinueWith(t =>
		{
			if (t is {IsFaulted: true, Exception: not null})
			{
#if DEBUG && RABOTORA_STRICT
				Console.WriteLine("[RTask] Unobserved exception in Forget(System.Threading.Tasks.Task): " + t.Exception);
#endif
			}
		}, TaskContinuationOptions.OnlyOnFaulted);
	}

	/// <summary>
	/// Attaches an external cancellation token to the given <see cref="RTask"/>, allowing it to be canceled from outside the task's own cancellation mechanism.
	/// </summary>
	/// <param name="task">The <see cref="RTask"/> to which the external cancellation token will be attached.</param>
	/// <param name="token">The external <see cref="CancellationToken"/> that will be used to cancel the task.</param>
	public static async RTask AttachExternalToken(this RTask task, CancellationToken token)
	{
		var tcs = RTaskPool<RTaskCompletionSource>.Pick();

		await using var registration = token.Register(() =>
		{
			tcs.TrySetException(new OperationCanceledException(token));
		});

		// ReSharper disable once ConvertToLocalFunction
		var runner = async () =>
		{
			try
			{
				await task;
				if (!token.IsCancellationRequested)
				{
					tcs.TrySetResult();
				}
			}
			catch (Exception ex)
			{
				if (!token.IsCancellationRequested)
				{
					tcs.TrySetException(ex);
				}
			}
		};
		
		runner().Forget();
		await new RTask(tcs, tcs.Version);
	}

	/// <summary>
	/// Gets an awaiter for the given <see cref="IEnumerator"/> routine, allowing it to be awaited using the async/await syntax.
	/// </summary>
	/// <param name="routine">The <see cref="IEnumerator"/> routine for which to get an awaiter.</param>
	/// <returns>>An <see cref="RTaskAwaiter"/> that can be used to await the completion of the routine.</returns>
	/// <seealso cref="GetAwaiter(RYieldData)"/>
	public static RTaskAwaiter GetAwaiter(this IEnumerator routine)
	{
		return RTask.FromIEnumerator(routine).GetAwaiter();
	}

	/// <summary>
	/// Gets an awaiter for the given <see cref="RYieldData"/>, allowing it to be awaited using the async/await syntax.
	/// </summary>
	/// <param name="yieldData">The <see cref="RYieldData"/> for which to get an awaiter.</param>
	/// <returns>>An <see cref="RTaskAwaiter"/> that can be used to await the completion of the yield data.</returns>
	/// <seealso cref="GetAwaiter(IEnumerator)"/>
	public static RTaskAwaiter GetAwaiter(this RYieldData yieldData)
	{
		return RTask.FromIEnumerator(SingleYield(yieldData)).GetAwaiter();
	}

	/// <summary>
	/// Asynchronously waits for the completion of the given <see cref="RaTweener"/>.
	/// </summary>
	/// <param name="tweener">The <see cref="RaTweener"/> to wait for.</param>
	[SuppressMessage("Readability", "RAT0003:Directly awaiting a RaTweener lacks semantic clarity")]
	public static async RTask AsyncWaitForCompletion(this RaTweener tweener)
	{
		await tweener; // RaTweener inherits from RYieldData, so this will work seamlessly with the existing GetAwaiter extension method for RYieldData
	}
	
	private static IEnumerator SingleYield(RYieldData yieldData)
	{
		yield return yieldData;
	}
}