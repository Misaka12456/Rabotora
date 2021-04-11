using Microsoft.CodeAnalysis.CSharp.Scripting;
using Rabotora.Compiler.Models;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Rabotora.Compiler
{
	/// <summary>
	/// Rabotora 启动器重编译类
	/// </summary>
	public static class Launcher
	{
		public static string ReCompile(CompilingGameProject cgproj,string outputPath)
		{
			string name = Path.Combine(outputPath, cgproj.ProjectName + ".exe");
			var p = new CompilerParameters
			{
				GenerateExecutable = true,
				CompilerOptions = "/target:winexe /optimize",
				GenerateInMemory = false,
				OutputAssembly = name
			};
			p.ReferencedAssemblies.Add("GameLauncher.exe");
			// TODO
			return name;
		}
	}
}
