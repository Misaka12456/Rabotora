using RabotoraX.Core.Graphics;
using Vortice.Direct3D11;

namespace RabotoraX.Interop.Direct3D11;

internal class DX11Buffer : IGpuBuffer
{
	public ID3D11Buffer NativeBuffer { get; }
	public int SizeInBytes { get; }
	
	public DX11Buffer(ID3D11Buffer buffer, int size)
	{
		NativeBuffer = buffer;
		SizeInBytes = size;
	}

	public void Dispose()
	{
		NativeBuffer.Dispose();
	}
}