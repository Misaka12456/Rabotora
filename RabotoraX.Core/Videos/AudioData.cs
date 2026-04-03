namespace RabotoraX.Core.Videos;

public struct AudioData
{
	public byte[]? Samples;
	public int SampleLength;
	public int Channels;
	public int SampleRate;
	public int BitDepth;
	public double Pts;
}