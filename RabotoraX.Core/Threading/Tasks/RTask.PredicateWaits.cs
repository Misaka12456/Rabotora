namespace RabotoraX.Core.Threading.Tasks;

public partial struct RTask
{
	/// <summary>
	/// Awaits all provided tasks to complete.
	/// If any task throws an exception, the first encountered exception will be re-thrown after all tasks have completed.
	/// </summary>
	/// <param name="tasks">An array of <see cref="RTask"/> instances to await.</param>
	/// <exception cref="Exception">Thrown if any of the provided tasks throw an exception. The first encountered exception will be re-thrown.</exception>
	/// <seealso cref="WhenAll{T}(params RTask{T}[])"/>
	/// <seealso cref="WhenAny(params RTask[])"/>
	/// <seealso cref="WhenAny{T}(params RTask{T}[])"/>
	public static async RTask WhenAll(params RTask[] tasks)
	{
		Exception? lastEx = null;
		foreach (var task in tasks)
		{
			try
			{
				await task;
			}
			catch (Exception ex)
			{
				lastEx = ex; // Capture the last exception
			}

			if (lastEx != null)
			{
				throw lastEx;
			}
		}
	}
	
	/// <summary>
	/// Awaits all provided tasks to complete and returns their results as an array.
	/// If any task throws an exception, the first encountered exception will be re-thrown after all tasks have completed.
	/// </summary>
	/// <param name="tasks">An array of <see cref="RTask{T}"/> instances to await.</param>
	/// <typeparam name="T">The type of the results produced by the tasks.</typeparam>
	/// <returns>An array containing the results of the completed tasks.</returns>
	/// <exception cref="Exception">Thrown if any of the provided tasks throw an exception. The first encountered exception will be re-thrown.</exception>
	/// <seealso cref="WhenAll(params RTask[])"/>
	/// <seealso cref="WhenAny(params RTask[])"/>
	/// <seealso cref="WhenAny{T}(params RTask{T}[])"/>
	public static async RTask<T[]> WhenAll<T>(params RTask<T>[] tasks)
	{
		var results = new T[tasks.Length];
		Exception? lastEx = null;
		for (int i = 0; i < tasks.Length; i++)
		{
			try
			{
				results[i] = await tasks[i];
			}
			catch (Exception ex)
			{
				lastEx = ex; // Capture the last exception
			}
		}

		if (lastEx != null)
		{
			throw lastEx;
		}
		
		return results;
	}

	/// <summary>
	/// Awaits any of the provided tasks to complete and returns the index of the first completed task.
	/// If any task throws an exception, the exception will be re-thrown immediately without waiting for other tasks to complete.
	/// </summary>
	/// <param name="tasks">An array of <see cref="RTask"/> instances to await.</param>
	/// <returns>The index of the first completed task, or -1 if no tasks were provided.</returns>
	/// <seealso cref="WhenAny{T}(params RTask{T}[])"/>
	/// <seealso cref="WhenAll(params RTask[])"/>
	/// <seealso cref="WhenAll{T}(params RTask{T}[])"/>
	public static RTask<int> WhenAny(params RTask[] tasks)
	{
		if (tasks.Length == 0)
		{
			return new RTask<int>(-1); // Return -1 if no tasks provided
		}
		var tcs = RTaskPool<RTaskCompletionSource<int>>.Pick();

		for (int i = 0; i < tasks.Length; i++)
		{
			RunWhenAnyWaiter(tasks[i], i, tcs).Forget();
		}
		return new RTask<int>(tcs, tcs.Version);
	}
	
	/// <summary>
	/// Awaits any of the provided tasks to complete and returns a tuple containing the index of the first completed task and its result.
	/// If any task throws an exception, the exception will be re-thrown immediately without waiting for other tasks to complete.
	/// </summary>
	/// <param name="tasks">An array of <see cref="RTask{T}"/> instances to await.</param>
	/// <typeparam name="T">The type of the results produced by the tasks.</typeparam>
	/// <returns>A tuple containing the index of the first completed task and its result, or (-1, default(T)) if no tasks were provided.</returns>
	/// <exception cref="ArgumentException">Thrown if the provided tasks array is empty.</exception>
	/// <seealso cref="WhenAny(params RTask[])"/>
	/// <seealso cref="WhenAll(params RTask[])"/>
	/// <seealso cref="WhenAll{T}(params RTask{T}[])"/>
	public static RTask<(int Index, T Result)> WhenAny<T>(params RTask<T>[] tasks)
	{
		if (tasks.Length == 0) throw new ArgumentException("Tasks array cannot be empty");
		var tcs = RTaskPool<RTaskCompletionSource<(int, T)>>.Pick();
		
		for (int i = 0; i < tasks.Length; i++)
		{
			RunWhenAnyWaiter(tasks[i], i, tcs).Forget();
		}
		return new RTask<(int, T)>(tcs, tcs.Version);
	}

	private static async RTask RunWhenAnyWaiter(RTask task, int index, RTaskCompletionSource<int> promise)
	{
		try
		{
			await task;
			promise.TrySetResult(index);
		}
		catch (Exception ex)
		{
			promise.TrySetException(ex);
		}
	}
	
	private static async RTask RunWhenAnyWaiter<T>(RTask<T> task, int index, RTaskCompletionSource<(int, T)> promise)
	{
		try
		{
			var result = await task;
			promise.TrySetResult((index, result));
		}
		catch (Exception ex)
		{
			promise.TrySetException(ex);
		}
	}
}