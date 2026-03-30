using System.Diagnostics.CodeAnalysis;

namespace RabotoraX.Core.Threading.Tasks;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global")]
internal class RTaskCompletionSource<T>
{
	public short Version { get; private set; } = 1;
	private Action? _continuation;
	private Exception? _exception;
	private T? _result;
	private int _isCompletedFlag; // use int instead of bool to utilize Interlocked operations

	public bool IsCompleted(short token) => token != Version || _isCompletedFlag == 1;

	public T GetResult(short token)
	{
		if (token != Version)
		{
			throw new InvalidOperationException("RTask reused incorrectly. This usually means that the RTask was awaited multiple times or that the RTask was not awaited at all. " +
			                                    "Please ensure that each RTask is awaited exactly once and that you do not reuse RTask instances to avoid conflict with internal RTask pooling.");
		}

		var ex = _exception;
		var res = _result;
		
		ResetAndReturnToPool();
		if (ex != null)
		{
			throw ex;
		}
		return res!;
	}

	public void OnCompleted(Action continuation, short token)
	{
		if (token != Version) return;
		if (_isCompletedFlag == 1)
		{
			continuation();
		}
		else
		{
			_continuation = continuation; // Store continuation now for future invocation when it completes
		}
	}

	public bool TrySetResult(T result)
	{
		if (Interlocked.Exchange(ref _isCompletedFlag, 1) == 1) return false; // Already completed
		_result = result;
		InvokeContinuation();
		return true;
	}
	
	public bool TrySetException(Exception ex)
	{
		if (Interlocked.Exchange(ref _isCompletedFlag, 1) == 1) return false; // Already completed
		_exception = ex;
		InvokeContinuation();
		return true;
	}

	public void SetResult(T result) => TrySetResult(result);
	public void SetException(Exception ex) => TrySetException(ex);
	
	private void InvokeContinuation()
	{
		var action = _continuation;
		_continuation = null;

		if (action != null)
		{
			if (MultiThreadService.IsRenderThread)
			{
				action();
			}
			else
			{
				MultiThreadService.Invoke(action);
			}
		}
	}

	private void ResetAndReturnToPool()
	{
		_continuation = null;
		_exception = null;
		_result = default;
		_isCompletedFlag = 0;
		unchecked
		{
			Version++;
		} // Increment version to invalidate any existing tokens
		RTaskPool<RTaskCompletionSource<T>>.Recycle(this);
	}
}