namespace RabotoraX.Core.Graphics;

public class RenderTexture : Object, INativeRenderTexture
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
	
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			NativeTexture.Dispose();
		}
		base.Dispose(disposing);
	}
}