namespace RabotoraX.Core.Graphics;

public interface IGpuBuffer : IDisposable
{
	int SizeInBytes { get; }
}