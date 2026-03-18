namespace RabotoraX.Core.Videos;

public struct AudioFrame
{
	public byte[] Samples;
	public int SampleRate;
	public int Channels;
	public int BitDepth;
	public double Pts;
}