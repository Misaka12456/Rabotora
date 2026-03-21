namespace RabotoraX.Core;

public sealed class RabotoraSynchronizationContext : SynchronizationContext
{
	private readonly EventHandler<Exception> _onException;

	public RabotoraSynchronizationContext(EventHandler<Exception> onException)
	{
		_onException = onException;
	}

	public override void Post(SendOrPostCallback d, object? state)
	{
		ThreadPool.QueueUserWorkItem(_ =>
		{
			try
			{
				d(state);
			}
			catch (Exception ex)
			{
				_onException(this, ex);
			}
		});
	}

	public override void Send(SendOrPostCallback d, object? state)
	{
		try
		{
			d(state);
		}
		catch (Exception ex)
		{
			_onException(this, ex);
			throw;
		}
	}
}