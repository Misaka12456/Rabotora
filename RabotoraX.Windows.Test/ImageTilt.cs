using RabotoraX.Core;
using RabotoraX.Core.Scripting;
using RabotoraX.Core.UI;

namespace RabotoraX.Windows.Test.Demo2D;

public sealed class ImageTilt : RManagedScript
{
	private const float FadeIn = 1.5f; // Linear
	private Component2D _image = null!;
	private float _prepareTimer = 0;
	private float _timer = 0;
	
	public override void OnStart()
	{
		if (GetComponent<RawImage>() != null)
		{
			var image = GetComponent<RawImage>()!;
			image.Opacity = 0;
			_image = image;
		}
		else
		{
			var image = GetComponent<Image>()!;
			image.Opacity = 0;
			_image = image;
		}
		_image.IsEnabled = true;
	}

	public override void OnUpdate(float deltaTime)
	{
		if (_prepareTimer < 1.0f)
		{
			_prepareTimer += deltaTime;
			return;
		}
		_timer += deltaTime;
		float alpha = Math.Clamp(_timer / FadeIn, 0, 1);
		if (_image is RawImage rawImage)
		{
			rawImage.Opacity = alpha;
		}
		else if (_image is Image image)
		{
			image.Opacity = alpha;
		}
	}
}