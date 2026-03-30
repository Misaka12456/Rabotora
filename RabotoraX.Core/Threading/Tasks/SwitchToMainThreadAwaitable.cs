using System.Runtime.CompilerServices;

namespace RabotoraX.Core.Threading.Tasks;

/// <summary>
/// Represents an awaitable that, when awaited, will switch the execution context to the main thread (RabotoraX Render Thread).
/// </summary>
/// <seealso cref="SwitchToThreadPoolAwaitable" />
public struct SwitchToMainThreadAwaitable
{
	public Awaiter GetAwaiter() => new();
	public struct Awaiter : ICriticalNotifyCompletion
	{
		public bool IsCompleted => MultiThreadService.IsRenderThread;
		public void GetResult() { }
		public void OnCompleted(Action continuation) => MultiThreadService.Invoke(continuation);
		public void UnsafeOnCompleted(Action continuation) => MultiThreadService.Invoke(continuation);
	}
}