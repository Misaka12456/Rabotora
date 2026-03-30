using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RabotoraX.Core.Threading.Tasks;

/// <summary>
/// Provides an awaiter for <see cref="RTask{T}"/> that allows it to be awaited using the async/await syntax.
/// </summary>
/// <typeparam name="T">The type of the result produced by the asynchronous operation.</typeparam>
/// <seealso cref="RTaskAwaiter"/>
[SuppressMessage("ReSharper", "StructCanBeMadeReadOnly")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public struct RTaskAwaiter<T> : ICriticalNotifyCompletion
{
	public bool IsCompleted => _task.IsCompleted;
	private readonly RTask<T> _task;
	
	public RTaskAwaiter(RTask<T> task)
	{
		_task = task;
	}
	
	public T GetResult() => _task.GetResult();
	public void OnCompleted(Action continuation) => _task.OnCompleted(continuation);
	public void UnsafeOnCompleted(Action continuation) => _task.OnCompleted(continuation);
}