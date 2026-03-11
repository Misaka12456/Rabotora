using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RabotoraX.Core.Threading;

public static class MultiThreadService
{
	public static bool IsRenderThread => Thread.CurrentThread == _tRender;
	private static Thread? _tRender;
	private readonly static ConcurrentQueue<RenderTask> _qTasks = new();
	private readonly static Stopwatch _qRenderTimeout = new();

	private struct RenderTask
	{
		public Action Action;
		public TaskCompletionSource? Tcs;
	}
	
	public static void Initialize(Thread tRender)
	{
		_tRender = tRender;
	}
	
	public static void Invoke(Action action)
	{
		if (IsRenderThread)
		{
			action();
		}
		else
		{
			_qTasks.Enqueue(new RenderTask() { Action = action });
		}
	}
	
	public static async Task InvokeAsync(Action action)
	{
		if (IsRenderThread)
		{
			try
			{
				action();
				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Task.FromException(ex);
			}
		}
		else
		{
			var tcs = new TaskCompletionSource();
			_qTasks.Enqueue(new RenderTask() { Action = action, Tcs = tcs });
			await tcs.Task;
		}
	}

	public static void ExecutePendingTasks(long maxTicks = 20000)
	{
		if (!IsRenderThread) throw new InvalidOperationException("ExecutePendingTasks must be called from the render thread.");
		_qRenderTimeout.Restart();
		while (_qTasks.TryDequeue(out var task))
		{
			try
			{
				task.Action();
				task.Tcs?.SetResult();
			}
			catch (Exception ex)
			{
				if (task.Tcs != null) task.Tcs.SetException(ex);
				else throw; // If it's a fire-and-forget task, rethrow the exception to avoid silently swallowing it
			}

			if (_qRenderTimeout.ElapsedTicks > maxTicks) break; // Move remaining tasks to the next frame to avoid frame drops.
		}
		_qRenderTimeout.Stop();
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfNotRenderThread(string methodName)
	{
		if (_tRender == null) return; // If the service is not initialized, we can't determine the render thread, so we won't throw. This allows some flexibility in initialization order.
		if (!IsRenderThread) throw new InvalidOperationException($"{methodName} must be called from the render thread.");
	}
}