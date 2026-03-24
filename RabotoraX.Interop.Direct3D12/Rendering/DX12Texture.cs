using RabotoraX.Core.Graphics;
using Vortice.Direct3D12;

namespace RabotoraX.Interop.Direct3D12.Rendering;

public class DX12Texture2D : INativeTexture2D
{
	public ID3D12Resource Resource { get; }
	public int Width { get; }
	public int Height { get; }
	
	public DX12Texture2D(ID3D12Resource res, int width, int height)
	{
		Resource = res;
		Width = width;
		Height = height;
	}
	
	public void Dispose()
	{
		Resource.Dispose();
		GC.SuppressFinalize(this);
	}
}

public class DX12RenderTexture : INativeRenderTexture
{
	public ID3D12Resource Resource { get; }
	public CpuDescriptorHandle RtvHandle { get; internal set; }
	public int Width { get; }
	public int Height { get; }
	public DX12RenderTexture(ID3D12Resource res, int width, int height)
	{
		Resource = res;
		Width = width;
		Height = height;
	}
	
	public void Dispose()
	{
		Resource.Dispose();
		GC.SuppressFinalize(this);
	}
}