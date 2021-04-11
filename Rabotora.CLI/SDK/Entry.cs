using Rabotora.CLI.SDK.Globals;
using static Rabotora.CLI.MultiLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabotora.CLI.SDK
{
	public class Entry
	{
		public static void HandleMain(string[] args)
		{
			try
			{
				switch (args[0].ToLower().Trim())
				{
					case "create":
						if (args.Length > 1)
						{
							var name = new StringBuilder();
							for (int i = 1; i < args.Length; i++)
							{
								name.Append(args[i]);
							}
							name.Replace("\"", string.Empty);
							ProjectManager.CreateProjectFromCLI(name.ToString(), Environment.CurrentDirectory);
						}
						else
						{
							Console.WriteLine(string.Format(GetRes<string>("CLI.MissingArguments")!, "Rabotora create",1));
						}
						break;
				}
			}
			catch(Exception)
			{

			}
		}
	}
}
