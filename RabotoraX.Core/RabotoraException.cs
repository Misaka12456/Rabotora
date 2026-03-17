namespace RabotoraX.Core;

public sealed class RabotoraException : Exception
{
	public RabotoraException() { }
	public RabotoraException(string message) : base(message) { }
	public RabotoraException(string message, Exception inner) : base(message, inner) { }
}