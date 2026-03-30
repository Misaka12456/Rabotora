using System.Runtime.CompilerServices;

namespace RabotoraX.Core.Threading.Tasks;

/// <summary>
/// Represents a light-weighted, zero-allocation, RabotoraX engine-oriented asynchronous operation that can be awaited. 
/// </summary>
/// <seealso cref="RTask{T}"/>
[AsyncMethodBuilder(typeof(RTaskMethodBuilder))]
public readonly partial struct RTask
{
	public readonly static RTask CompletedTask = new(null, 0);
	
	internal bool IsCompleted => _tcs == null || _tcs.IsCompleted(_token);
	
	private readonly RTaskCompletionSource? _tcs;
	private readonly short _token;
	
	internal RTask(RTaskCompletionSource? tcs, short token)
	{
		_tcs = tcs;
		_token = token;
	}
	
	public RTaskAwaiter GetAwaiter()
	{
		return new RTaskAwaiter(this);
	}

	internal void GetResult()
	{
		_tcs?.GetResult(_token);
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

