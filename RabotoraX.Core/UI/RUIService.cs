namespace RabotoraX.Core.UI;

public static class RUIService
{
	public static INativeTextFactory TextFactory { get; internal set; } = null!;
	
	public static void Initialize(INativeTextFactory textFactory)
	{
		TextFactory = textFactory;
	}

	public static void Dispose()
	{
		TextFactory = null!; // Do not dispose the factory here, as it may implemented by the Native Graphics API and should be disposed by it.
	}
}