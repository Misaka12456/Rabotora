namespace RabotoraX.Core.Graphics;

public class RenderTexture : Object, INativeTexture2D
{
	public int Width => NativeTexture.Width;
	public int Height => NativeTexture.Height;
	
	public INativeRenderTexture NativeTexture { get; }

	public RenderTexture(int width, int height)
	{
		if (width <= 0 || height <= 0)
			throw new ArgumentException("Width and Height must be greater than zero.");
		
		NativeTexture = GraphicsService.API.CreateRenderTexture(width, height);
	}

	public void Bind()
	{
		GraphicsService.API.SetRenderTarget(NativeTexture);
	}
	
	public void Unbind()
	{
		GraphicsService.API.SetRenderTarget(null);
	}
	
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			NativeTexture.Dispose();
		}
		base.Dispose(disposing);
	}
}