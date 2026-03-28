namespace RabotoraX.Core.Audios;

public class RAudioListener : Component
{
	public override void OnAwake()
	{
		base.OnAwake();
		AudioService.MainListener ??= this;
	}

	protected override void Dispose(bool disposing)
	{
		if (AudioService.MainListener == this)
		{
			AudioService.MainListener = null;
		}
		base.Dispose(disposing);
	}
}