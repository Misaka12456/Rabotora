using Vortice.Direct3D11;

namespace RabotoraEngine.Graphics;

public class DXTexture2D : ITexture2D
{
	public int Width { get; }
	public int Height { get; }
	public object NativeData => _texture ?? throw new InvalidOperationException("Texture is not initialized or already disposed.");

	private readonly ID3D11ShaderResourceView? _resourceView;
	private readonly ID3D11Texture2D? _texture;
	private bool _disposed;

	public DXTexture2D(ID3D11Texture2D nativeTex, ID3D11ShaderResourceView resView)
	{
		_texture = nativeTex;
		_resourceView = resView;
		
		var desc = nativeTex.Description;
		Width = (int)desc.Width;
		Height = (int)desc.Height;
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_resourceView?.Dispose();
			_texture?.Dispose();
			_disposed = true;
		}
		GC.SuppressFinalize(this);
	}

	~DXTexture2D()
	{
		Dispose();
	}
}