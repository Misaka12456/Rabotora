using System.Diagnostics.CodeAnalysis;

namespace RabotoraX.Core.Threading.Tasks;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global")]
internal class RTaskCompletionSource
{
	public short Version { get; private set; } = 1;
	private Action? _continuation;
	private Exception? _exception;
	private int _isCompletedFlag; // use int instead of bool to utilize Interlocked operations

	public bool IsCompleted(short token) => token != Version || _isCompletedFlag == 1;

	public void GetResult(short token)
	{
		if (token != Version)
		{
			throw new InvalidOperationException("RTask reused incorrectly. This usually means that the RTask was awaited multiple times or that the RTask was not awaited at all. " +
			                                    "Please ensure that each RTask is awaited exactly once and that you do not reuse RTask instances to avoid conflict with internal RTask pooling.");
		}

		var ex = _exception;
		
		ResetAndReturnToPool();
		if (ex != null)
		{
			throw ex;
		}
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

	public bool TrySetResult()
	{
		if (Interlocked.Exchange(ref _isCompletedFlag, 1) == 1) return false; // Already completed
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
	
	public void SetResult() => TrySetResult();
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
		_isCompletedFlag = 0;
		unchecked { Version++; } // Increment version to invalidate any existing tokens
		RTaskPool<RTaskCompletionSource>.Recycle(this);
	}
}