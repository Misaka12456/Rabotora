using System.Text;
using Rabotora.Graphics;

namespace Rabotora.Internal;

public static class DotNetResourceManager
{
	public static byte[] FetchResourceBytes(string path)
	{
		path = path.Replace("/", ".").Replace("\\", ".");
		path = $"RabotoraEngine.Resources.{path}";
		var assembly = typeof(RWindow).Assembly;
		using var rs = assembly.GetManifestResourceStream(path); // rs: resource stream
		if (rs == null)
		{
			throw new FileNotFoundException($"Resource '{path}' not found in assembly '{assembly.FullName}'.");
		}
		using var ms = new MemoryStream();
		rs.CopyTo(ms);
		return ms.ToArray();
	}
	
	public static string FetchResourceText(string path, Encoding encoding)
	{
		path = path.Replace("/", ".").Replace("\\", ".");
		path = $"RabotoraEngine.Resources.{path}";
		var assembly = typeof(RWindow).Assembly;
		using var rs = assembly.GetManifestResourceStream(path); // rs: resource stream
		if (rs == null)
		{
			throw new FileNotFoundException($"Resource '{path}' not found in assembly '{assembly.FullName}'.");
		}
		using var sr = new StreamReader(rs, encoding);
		return sr.ReadToEnd();
	}
	
	public static string FetchResourceText(string path) => FetchResourceText(path, Encoding.UTF8);
}