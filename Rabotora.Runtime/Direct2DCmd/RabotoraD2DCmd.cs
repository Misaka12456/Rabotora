using SharpDX.Direct2D1;

namespace Rabotora.Runtime
{
	/// <summary>
	/// Represents a <see cref="RabotoraD2DCmd"/> which can be stopped when running.
	/// </summary>
	public interface IStoppableCmd : IDisposable
	{
		/// <summary>
		/// Should we stop running the command now?
		/// </summary>
		public bool IsStopDoing { get; set; }

		/// <summary>
		/// Stop the running command instantly.
		/// <para>
		/// Call this method on a command which is not running will <b>do nothing</b>.
		/// </para>
		/// </summary>
		public void StopCommand()
		{
			IsStopDoing = true;
		}

		/// <summary>
		/// Stop the running command instantly.
		/// <para>
		/// Call this method on a command which is not running will <b>do nothing</b>.
		/// </para>
		/// </summary>
		void IDisposable.Dispose()
		{
			GC.SuppressFinalize(this);
			StopCommand();
		}
	}

	/// <summary>
	/// Represents an abstract Rabotora Direct2D Command.
	/// </summary>
	public abstract class RabotoraD2DCmd
	{
		/// <summary>
		/// Execute the command on the destination render target.
		/// </summary>
		/// <param name="renderTarget">Destination render target.</param>
		public abstract void DoCommand(RenderTarget renderTarget);
	}
}
