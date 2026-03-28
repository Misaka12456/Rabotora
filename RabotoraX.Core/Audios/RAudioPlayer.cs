using System.Numerics;
using RabotoraX.Core.Cinematics;
using Silk.NET.OpenAL;

namespace RabotoraX.Core.Audios;

public class RAudioPlayer : Component
{
	private uint _sourceId;
	private AudioClip? _clip;

	public AudioClip? Clip
	{
		get => _clip;
		set
		{
			_clip = value;
			if (_sourceId != 0)
			{
				AudioService.AL.SetSourceProperty(_sourceId, SourceInteger.Buffer, _clip != null ? (int)_clip.BufferId : 0);
			}
		}
	}

	public float Volume
	{
		get => _volume;
		set
		{
			_volume = value;
			if (_sourceId != 0) AudioService.AL.SetSourceProperty(_sourceId, SourceFloat.Gain, _volume);
		}
	}
	private float _volume = 1.0f;

	public float Pitch
	{
		get => _pitch;
		set
		{
			_pitch = value;
			if (_sourceId != 0) AudioService.AL.SetSourceProperty(_sourceId, SourceFloat.Pitch, _pitch);
		}
	}
	private float _pitch = 1.0f;

	public bool Loop
	{
		get => _loop;
		set
		{
			_loop = value;
			if (_sourceId != 0) AudioService.AL.SetSourceProperty(_sourceId, SourceBoolean.Looping, _loop);
		}
	}
	private bool _loop;
	
	public bool Is3D { get; set; } = false; // 是否启用3D空间音效，启用后声音会根据RAudioListener的位置和朝向进行空间化处理
	// (对于Render2D Stage，此参数会被自动忽略，按照默认值false处理)

	public override void OnAwake()
	{
		base.OnAwake();
		_sourceId = AudioService.AL.GenSource();
		Volume = _volume;
		Pitch = _pitch;
		Loop = _loop;
	}

	public void Play()
	{
		if (_sourceId != 0 && Clip != null)
		{
			AudioService.AL.SourcePlay(_sourceId);
		}
	}

	public void Stop()
	{
		if (_sourceId != 0)
		{
			AudioService.AL.SourceStop(_sourceId);
		}
	}
	
	public void Pause()
	{
		if (_sourceId != 0)
		{
			AudioService.AL.SourcePause(_sourceId);
		}
	}

	public override void OnUpdate(float deltaTime)
	{
		if (_sourceId == 0) return;
		
		AudioService.AL.GetSourceProperty(_sourceId, GetSourceInteger.SourceState, out int state);
		if (state != (int) SourceState.Playing) return;
		
		bool enforce2D = RObject.Stage.Type == StageType.Render2D;
		if (enforce2D || !Is3D)
		{
			AudioService.AL.SetSourceProperty(_sourceId, SourceBoolean.SourceRelative, true);
			AudioService.AL.SetSourceProperty(_sourceId, SourceVector3.Position, Vector3.Zero);
			AudioService.AL.SetSourceProperty(_sourceId, SourceFloat.RolloffFactor, 0f); // Disable distance attenuation for 2D sounds
		}
		else
		{
			AudioService.AL.SetSourceProperty(_sourceId, SourceBoolean.SourceRelative, false);
			AudioService.AL.SetSourceProperty(_sourceId, SourceVector3.Position, Layout.WorldMatrix.Translation);
			AudioService.AL.SetSourceProperty(_sourceId, SourceFloat.RolloffFactor, 1f); // Use normal distance attenuation for 3D sounds
		}
	}

	protected override void Dispose(bool disposing)
	{
		Stop();
		if (_sourceId != 0)
		{
			AudioService.AL.DeleteSource(_sourceId);
			_sourceId = 0;
		}
		base.Dispose(disposing);
	}
}