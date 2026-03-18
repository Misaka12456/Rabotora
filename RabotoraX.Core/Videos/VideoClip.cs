namespace RabotoraX.Core.Videos;

public class VideoClip : Object // not System.Object, but a RabotoraX.Core.Object
{
	public Stream Stream { get; }
	public double Duration { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }
	
	public VideoClip(Stream stream)
	{
		Stream = stream ?? throw new ArgumentNullException(nameof(stream));
		if (!Stream.CanRead || !Stream.CanSeek)
		{
			throw new ArgumentException("Stream must be readable and seekable.", nameof(stream));
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Stream.Dispose();
		}
		base.Dispose(disposing);
	}
}