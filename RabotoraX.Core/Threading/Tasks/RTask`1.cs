using System.Runtime.CompilerServices;

namespace RabotoraX.Core.Threading.Tasks;

/// <summary>
/// Represents a light-weighted, zero-allocation, RabotoraX engine-oriented, result-bearing asynchronous operation that can be awaited.
/// </summary>
/// <typeparam name="T">The type of the result produced by the asynchronous operation.</typeparam>
/// <seealso cref="RTask"/>
[AsyncMethodBuilder(typeof(RTaskMethodBuilder<>))]
public readonly struct RTask<T>
{
	internal bool IsCompleted => _tcs == null || _tcs.IsCompleted(_token);
	private readonly RTaskCompletionSource<T>? _tcs;
	private readonly short _token;
	private readonly T? _syncResult;
	
	internal RTask(RTaskCompletionSource<T>? tcs, short token)
	{
		_tcs = tcs;
		_token = token;
		_syncResult = default;
	}
	
	internal RTask(T syncResult)
	{
		_tcs = null;
		_token = 0;
		_syncResult = syncResult;
	}
	
	public RTaskAwaiter<T> GetAwaiter()
	{
		return new RTaskAwaiter<T>(this);
	}
	
	internal T GetResult()
	{
		if (_tcs == null)
		{
			return _syncResult!;
		}
		return _tcs.GetResult(_token);
	}
	
	internal void OnCompleted(Action continuation)
	{
		if (_tcs == null)
		{
			continuation();
		}
		else
		{
			_tcs.OnCompleted(continuation, _token);
		}
	}
}
