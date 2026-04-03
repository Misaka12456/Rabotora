namespace RabotoraX.Core;

/// <summary>
/// Represents the base class for all objects in the RabotoraX framework.
/// </summary>
public abstract class Object : IDisposable
{
	public string InstanceId { get; internal set; } // Add setter for RabotoraX Stage Serialization support
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
	
	public static bool operator ==(Object? left, Object? right)
	{
		if (ReferenceEquals(left, right)) return true;
		if (left is null || right is null) return false;
		return left.InstanceId == right.InstanceId;
	}
	
	public static bool operator !=(Object? left, Object? right)
	{
		return !(left == right);
	}
	
	public bool Equals(Object? other)
	{
		if (other is null) return false;
		return InstanceId == other.InstanceId;
	}
	
	public override bool Equals(object? obj)
	{
		if (obj is Object other)
		{
			return Equals(other);
		}
		return false;
	}
	
	public override int GetHashCode()
	{
		return InstanceId.GetHashCode();
	}
}