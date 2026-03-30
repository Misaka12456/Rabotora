using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RabotoraX.Core.Threading.Tasks;

/// <summary>
/// Represents the builder for asynchronous methods that return a <see cref="RTask{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the result produced by the asynchronous method.</typeparam>
/// <seealso cref="RTaskMethodBuilder"/>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public struct RTaskMethodBuilder<T>
{
	public RTask<T> Task => GetCurrentTask();
	private RTaskCompletionSource<T>? _tcs;
	private T? _syncResult;
	
	public static RTaskMethodBuilder<T> Create() => new();
	
	public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
	{
		stateMachine.MoveNext();
	}
	
	public void SetStateMachine(IAsyncStateMachine stateMachine)
	{
		// No need to store the state machine reference since we don't use it for anything
	}
	
	public void SetResult(T result)
	{
		if (_tcs == null)
		{
			_syncResult = result;
		}
		else
		{
			_tcs.SetResult(result);
		}
	}

	public void SetException(Exception ex)
	{
		_tcs ??= RTaskPool<RTaskCompletionSource<T>>.Pick();
		_tcs.SetException(ex);
	}
	
	public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
		where TAwaiter : INotifyCompletion
		where TStateMachine : IAsyncStateMachine
	{
		_tcs ??= RTaskPool<RTaskCompletionSource<T>>.Pick();
		awaiter.OnCompleted(stateMachine.MoveNext);
	}
	
	public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
		where TAwaiter : ICriticalNotifyCompletion
		where TStateMachine : IAsyncStateMachine
	{
		_tcs ??= RTaskPool<RTaskCompletionSource<T>>.Pick();
		awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
	}
	
	private RTask<T> GetCurrentTask()
	{
		if (_tcs == null)
		{
			return new RTask<T>(_syncResult!);
		}
		return new RTask<T>(_tcs, _tcs.Version);
	}
}