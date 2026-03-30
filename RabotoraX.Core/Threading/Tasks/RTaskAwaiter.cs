using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RabotoraX.Core.Threading.Tasks;

/// <summary>
/// Provides an awaiter for <see cref="RTask"/> that allows it to be awaited using the async/await syntax.
/// </summary>
/// <seealso cref="RTaskAwaiter{T}"/>
[SuppressMessage("ReSharper", "StructCanBeMadeReadOnly")] // to compatible with .NET's integrated async/await system, this should not be read-only
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public struct RTaskAwaiter : ICriticalNotifyCompletion
{
	public bool IsCompleted => _task.IsCompleted;
	private readonly RTask _task;
	
	public RTaskAwaiter(RTask task)
	{
		_task = task;
	}
	
	public void GetResult() => _task.GetResult();
	public void OnCompleted(Action continuation) => _task.OnCompleted(continuation);
	public void UnsafeOnCompleted(Action continuation) => _task.OnCompleted(continuation);
}