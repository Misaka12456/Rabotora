namespace RabotoraX.Core.Videos;

public interface IVideoDecoder : IDisposable
{
	bool HasAudio { get; }
	
	bool IsReady { get; }
	
	void Initialize(VideoClip clip);
	
	bool TryReadNextVideoFrame(out ReadOnlySpan<byte> frameData, out double timestamp);
	bool TryReadNextAudioBlock(out AudioData audioData);
	void Seek(double time);

	public static IVideoDecoder PlatformCreate()
	{
		Type? implType;
		try
		{
			implType = OperatingSystem.IsWindows() ? Type.GetType("RabotoraX.Interop.Win32.RenderImpl.WindowsMediaFoundationDecoder, RabotoraX.Interop.Win32") :
				OperatingSystem.IsLinux() ? Type.GetType("RabotoraX.Interop.Linux.RenderImpl.FfmpegDecoder, RabotoraX.Interop.Linux") :
				OperatingSystem.IsMacOS() ? Type.GetType("RabotoraX.Interop.MacOS.RenderImpl.DarwinVideoToolboxDecoder, RabotoraX.Interop.MacOS") :
				OperatingSystem.IsAndroid() ? Type.GetType("RabotoraX.Interop.Android.RenderImpl.AndroidExoPlayerDecoder, RabotoraX.Interop.Android") :
				OperatingSystem.IsIOS() ? Type.GetType("RabotoraX.Interop.IOS.RenderImpl.DarwinVideoToolboxDecoder, RabotoraX.Interop.IOS") :
				throw new PlatformNotSupportedException("Unsupported platform");
		}
		catch
		{
			implType = null;
		}
		
		if (implType == null)
		{
			throw new NotImplementedException("This functionality is not implemented in the portable version of this assembly. " +
			                                  "You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
		}
		return (IVideoDecoder)Activator.CreateInstance(implType)!;
	}
}