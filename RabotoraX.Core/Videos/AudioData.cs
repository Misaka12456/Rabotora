namespace RabotoraX.Core.Videos;

public struct AudioData
{
	public ReadOnlyMemory<byte> Samples;
	public int Channels;
	public int SampleRate;
	public int BitDepth;
	public double Pts;
}