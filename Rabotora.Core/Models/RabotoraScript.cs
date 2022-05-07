using Rabotora.Core.Rsf;
using System.Text;

namespace Rabotora.Core
{
	public struct RabotoraScript
	{
		public string ScriptName { get; }

		public string ScriptPath { get; }

		public RawRsfScript RawScript { get; }

		private RabotoraScript(string scriptName, string scriptPath, RawRsfScript script)
		{
			ScriptName = scriptName;
			ScriptPath = scriptPath;
			RawScript = script;
		}

		public static RabotoraScript LoadStandaloneScript(string scriptPath)
		{
			return new RabotoraScript(new DirectoryInfo(scriptPath).Name, scriptPath, RabotoraScriptFormat.ParseScript(File.ReadAllText(scriptPath, Encoding.UTF8)));
		}

		public static RabotoraScript LoadScript(string scriptPath, string scriptName, Stream scriptStream, bool closeStreamAfterFinish = false)
		{
			byte[] scriptData = new byte[scriptStream.Length];
			scriptStream.Read(scriptData, 0, scriptData.Length);
			if (closeStreamAfterFinish)
			{
				scriptStream.Close();
			}
			return new RabotoraScript(scriptName, scriptPath, RabotoraScriptFormat.ParseScript(Encoding.UTF8.GetString(scriptData)));
		}

		public static RabotoraScript LoadScript(string scriptPath, string scriptName, byte[] scriptRawData)
		{
			return new RabotoraScript(scriptName, scriptPath, RabotoraScriptFormat.ParseScript(Encoding.UTF8.GetString(scriptRawData)));
		}
	}
}
