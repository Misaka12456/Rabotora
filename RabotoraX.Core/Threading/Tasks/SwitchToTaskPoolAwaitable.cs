using System.Runtime.CompilerServices;

namespace RabotoraX.Core.Threading.Tasks;

/// <summary>
/// Represents an awaitable that, when awaited, will switch the execution context to a task pool thread.
/// </summary>
/// <seealso cref="SwitchToMainThreadAwaitable" />
/// <seealso cref="SwitchToThreadPoolAwaitable" />
[Obsolete("Use RTask.SwitchToThreadPool() and SwitchToThreadPoolAwaitable instead for better performance and reliability. This will be removed in a future version.")]
public struct SwitchToTaskPoolAwaitable
{
	public Awaiter GetAwaiter() => new();
	public struct Awaiter : ICriticalNotifyCompletion
	{
		public bool IsCompleted => !MultiThreadService.IsRenderThread;
		public void GetResult() { }
		public void OnCompleted(Action continuation) => Task.Run(continuation);
		public void UnsafeOnCompleted(Action continuation) => Task.Run(continuation);
	}
}