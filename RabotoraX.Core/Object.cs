namespace RabotoraX.Core;

public abstract class Object : IDisposable
{
	public string InstanceId { get; }
	private bool _isDisposed;

	protected Object()
	{
		InstanceId = Guid.NewGuid().ToString("N");
	}

	public void Dispose()
	{
		if (_isDisposed) return;
		Dispose(true);
		GC.SuppressFinalize(this);
		_isDisposed = true;
	}

	protected virtual void Dispose(bool disposing)
	{
	}
}