using System.Numerics;
using Silk.NET.OpenAL;

namespace RabotoraX.Core.Audios;

public static class AudioService
{
	public static AL AL { get; private set; } = null!;
	public static ALContext ALC { get; private set; } = null!;

	private static unsafe Device* _device;
	private static unsafe Context* _context;

	public static RAudioListener? MainListener { get; internal set; } // ONLY allowed one listener in every RStage (multiple listeners will use the first registered one as the main listener and ignore the others)
	// RStages that don't need to play any audio can simply not register any listener, this will leave null by default and won't cause any problem

	public static unsafe void Initialize()
	{
		ALC = ALContext.GetApi();
		AL = AL.GetApi();
		
		_device = ALC.OpenDevice(string.Empty);
		if (_device == null)
		{
			throw new InvalidOperationException("Failed to open audio device because there is no audio device available.");
		}
		
		_context = ALC.CreateContext(_device, null);
		ALC.MakeContextCurrent(_context);

		AL.DistanceModel(DistanceModel.InverseDistanceClamped);
	}

	public static void Update()
	{
		if (MainListener == null || !MainListener.IsEnabled || !MainListener.RObject.IsActive)
		{
			return; // No valid listener, skip audio update
		}
		
		var layout = MainListener.RObject.Layout;
		var pos = layout.Position;
		
		var forward = Vector3.Transform(-Vector3.UnitZ, layout.Rotation);
		var up = Vector3.Transform(Vector3.UnitY, layout.Rotation);

		AL.SetListenerProperty(ListenerVector3.Position, pos);

		unsafe
		{
			float* orientation = stackalloc float[6]
			{
				forward.X, forward.Y, forward.Z,
				up.X, up.Y, up.Z
			};
			AL.SetListenerProperty(ListenerFloatArray.Orientation, orientation);
		}
	}

	public static unsafe void Dispose()
	{
		if (_context != null)
		{
			ALC.MakeContextCurrent(null);
			ALC.DestroyContext(_context);
			_context = null;
		}
		if (_device != null)
		{
			ALC.CloseDevice(_device);
			_device = null;
		}
		AL?.Dispose();
		ALC?.Dispose();
	}
}