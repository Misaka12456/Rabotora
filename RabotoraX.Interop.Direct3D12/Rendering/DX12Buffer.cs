using RabotoraX.Core.Graphics;
using Vortice.Direct3D12;

namespace RabotoraX.Interop.Direct3D12.Rendering;

public sealed class DX12Buffer : IGpuBuffer
{
	public ID3D12Resource Resource { get; }
	public int SizeInBytes { get; }

	public DX12Buffer(ID3D12Resource res, int size)
	{
		Resource = res;
		SizeInBytes = size;
	}

	public void Dispose()
	{
		Resource.Dispose();
	}
}