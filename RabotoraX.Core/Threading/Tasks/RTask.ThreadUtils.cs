namespace RabotoraX.Core.Threading.Tasks;

public partial struct RTask
{
	/// <summary>
	/// Returns an awaitable that, when awaited, will switch the execution context to the main thread (RabotoraX Render Thread).
	/// </summary>
	/// <returns>An awaitable that switches to the main thread when awaited.</returns>
	/// <seealso cref="SwitchToThreadPool" />
	public static SwitchToMainThreadAwaitable SwitchToMainThread() => new();
	
	/// <summary>
	/// Returns an awaitable that, when awaited, will switch the execution context to a thread pool thread.
	/// </summary>
	/// <returns>An awaitable that switches to a thread pool thread when awaited.</returns>
	/// <seealso cref="SwitchToMainThread" />
	public static SwitchToThreadPoolAwaitable SwitchToThreadPool() => new();
	
	/// <summary>
	/// Returns an awaitable that, when awaited, will switch the execution context to a task pool thread.
	/// </summary>
	/// <returns>An awaitable that switches to a task pool thread when awaited.</returns>
	/// <seealso cref="SwitchToMainThread" />
	/// <seealso cref="SwitchToThreadPool" />
	[Obsolete("Use RTask.SwitchToThreadPool() and SwitchToThreadPoolAwaitable instead for better performance and reliability. This will be removed in a future version.")]
	public static SwitchToTaskPoolAwaitable SwitchToTaskPool() => new();
}