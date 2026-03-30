using System.Collections.Concurrent;

namespace RabotoraX.Core.Threading.Tasks;

internal static class RTaskPool<T> where T : class, new()
{
	private readonly static ConcurrentStack<T> _pool = new();

	public static T Pick()
	{
		return _pool.TryPop(out var item) ? item : new T();
	}
	
	public static void Recycle(T item)
	{
		_pool.Push(item);
	}
}