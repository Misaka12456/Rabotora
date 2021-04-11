using System;
using System.Globalization;
using System.Reflection;
using static Rabotora.CLI.MultiLang;

namespace Rabotora.CLI
{
	class Program
	{
		static void Main(string[] args)
		{
			switch (args.Length)
			{
				case 0:
					Console.WriteLine(GetRes<string>("CLI.BaseHelp"));
					break;
				case 1:
					if (args[0].Trim().StartsWith('-'))
					{
						string option = args[0].Trim().ToLower()[1..];
						switch (option)
						{
							case "ver":
							case "-version":
								Console.WriteLine(string.Format(GetRes<string>("CLI.Version")!,
									Assembly.GetExecutingAssembly()!.GetName().Version!.ToString(),
									Assembly.GetEntryAssembly()!.GetName().Version!.ToString()));
								break;
							case "?":
								Console.WriteLine(GetRes<string>("CLI.Help"));
								break;
							default:
								Console.WriteLine(GetRes<string>("CLI.Help"));
								break;
						}
					}
					else if (args[0].Trim().ToLower().EndsWith(".rexe"))
					{
						Console.WriteLine("Rabotora Runtime is still developing, please wait for patience...");
					}
					break;
				default:
					break;
			}
		}
	}
}
