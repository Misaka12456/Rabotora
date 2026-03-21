using System.Reflection;
using System.Runtime.CompilerServices;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Scripting;
using RabotoraX.Core.UI;
using RabotoraX.Core.Videos;

namespace RabotoraX.Windows.Test.Demo2D;

public sealed class VideoTexturePlayer : RManagedScript
{
	private const float WaitTime = 1.5f;
	private RVideoPlayer _videoPlayer = null!;
	private RawImage _rawImage = null!;
	public Text _statusText = null!;
	private float _prepareTimer = 0;
	private string _ver = string.Empty;
	private string _length = string.Empty;
	
	public override void OnStart()
	{
		_videoPlayer = GetComponent<RVideoPlayer>()!;
		_rawImage = GetComponent<RawImage>()!;
		string graphApiName = GraphicsService.API.ApiName switch
		{
			{ } p when p.StartsWith("Direct") => "DirectX",
			{ } p when p.StartsWith("Vulkan") => "Vulkan",
			{ } p when p.StartsWith("OpenGL") => "OpenGL",
			{ } p when p.StartsWith("Metal") => "Metal",
			_ => "Unknown"
		};
		_ver = $"RabotoraX {graphApiName} Release {Assembly.GetAssembly(typeof(GraphicsService))?.GetName().Version?.ToString(3) ?? string.Empty}";
		_length = TimeSpan.FromSeconds(_videoPlayer.Length.TotalSeconds).ToString(@"mm\:ss");
	}

	public override void OnUpdate(float deltaTime)
	{
		if (_prepareTimer < WaitTime)
		{
			_prepareTimer += deltaTime;
			return;
		}
		UpdatePlayer();
		UpdateStatus();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdatePlayer()
	{
		if (_videoPlayer.IsPlaying) return;
		
		Console.WriteLine("Preparing video...");
		_videoPlayer.Play();
	}

	private void UpdateStatus()
	{
		var currentTime = _videoPlayer.Time;
		var colorRange = _videoPlayer.ColorType switch
		{
			VideoRenderColorType.FollowSystem => "Follow System",
			VideoRenderColorType.Full => "Full",
			VideoRenderColorType.Limited => "Limited",
			_ => "Unknown"
		};
		_statusText.Content = $"{_ver}\nColor Range: {colorRange}\nTime: {currentTime:mm\\:ss} / {_length}";
	}
}