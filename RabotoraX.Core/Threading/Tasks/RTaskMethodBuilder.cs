using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RabotoraX.Core.Threading.Tasks;

/// <summary>
/// Represents the builder for asynchronous methods that return <see cref="RTask"/>.
/// </summary>
/// <seealso cref="RTaskMethodBuilder{T}"/>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public struct RTaskMethodBuilder
{
	public RTask Task => GetCurrentTask();
	private RTaskCompletionSource? _tcs;

	public static RTaskMethodBuilder Create() => new();

	public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
	{
		stateMachine.MoveNext();
	}

	public void SetStateMachine(IAsyncStateMachine stateMachine)
	{
		// No need to store the state machine reference since we don't use it for anything
	}

	public void SetResult() => _tcs?.SetResult();
	public void SetException(Exception ex) => _tcs?.SetException(ex);

	public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
		where TAwaiter : INotifyCompletion
		where TStateMachine : IAsyncStateMachine
	{
		_tcs ??= RTaskPool<RTaskCompletionSource>.Pick();
		awaiter.OnCompleted(stateMachine.MoveNext);
	}

	public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
		where TAwaiter : ICriticalNotifyCompletion
		where TStateMachine : IAsyncStateMachine
	{
		_tcs ??= RTaskPool<RTaskCompletionSource>.Pick();
		awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
	}

	private RTask GetCurrentTask()
	{
		if (_tcs == null)
		{
			return RTask.CompletedTask;
		}

		return new RTask(_tcs, _tcs.Version);
	}
}