namespace MyFirstGame;

public static class Program
{
	[STAThread]
	public static int Main(string[] args)
	{
		using var game = new MyFirstGame();
		game.Run();
		
		Console.WriteLine("Window closed. Exiting...");
		return 0;
	}
}	