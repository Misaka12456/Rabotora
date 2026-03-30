using System.Runtime.CompilerServices;

namespace RabotoraX.Core.Threading.Tasks;

/// <summary>
/// Represents an awaitable that, when awaited, will switch the execution context to a thread pool thread.
/// </summary>
/// <seealso cref="SwitchToMainThreadAwaitable" />
public struct SwitchToThreadPoolAwaitable
{
	public Awaiter GetAwaiter() => new();
	public struct Awaiter : ICriticalNotifyCompletion
	{
		public bool IsCompleted => !MultiThreadService.IsRenderThread;
		public void GetResult() { }
		public void OnCompleted(Action continuation) => ThreadPool.QueueUserWorkItem(_ => continuation());
		public void UnsafeOnCompleted(Action continuation) => ThreadPool.QueueUserWorkItem(_ => continuation());
	}
}