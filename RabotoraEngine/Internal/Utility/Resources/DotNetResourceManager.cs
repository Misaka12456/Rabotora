using System.Diagnostics;
using System.Reflection;
using System.Text;
using RabotoraEngine.Internal.Utility.Common;

namespace RabotoraEngine.Internal.Utility.Resources;

/// <summary>
/// Provides methods to fetch embedded resources from .NET assemblies.
/// </summary>
public static class DotNetResourceManager
{
	private readonly static char[] InvalidPathCharacters = [' ','-','~','!','@','#','$','%','^','&','*','(',')','+','=','{','}','[',']',';',',','\''];
	
	/// <summary>
	/// Fetches the text content of an embedded resource from the calling assembly using UTF-8 encoding.
	/// </summary>
	/// <param name="resPath">The resource path.</param>
	/// <returns>The text content of the resource.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	public static string FetchResourceText(string resPath) => Assembly.GetCallingAssembly().FetchResourceText(resPath, Encoding.UTF8);
	
	/// <summary>
	/// Fetches the text content of an embedded resource from the calling assembly using the specified encoding.
	/// </summary>
	/// <param name="resPath">The resource path.</param>
	/// <param name="encoding">The text encoding to use.</param>
	/// <returns>The text content of the resource.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	public static string FetchResourceText(string resPath, Encoding encoding) => Assembly.GetCallingAssembly().FetchResourceText(resPath, encoding);
	
	/// <summary>
	/// Fetches the text content of an embedded resource from the specified assembly using UTF-8 encoding.
	/// </summary>
	/// <param name="asm">The assembly containing the resource.</param>
	/// <param name="resPath">The resource path.</param>
	/// <returns>The text content of the resource.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	public static string FetchResourceText(this Assembly asm, string resPath) => asm.FetchResourceText(resPath, Encoding.UTF8);
	
	/// <summary>
	/// Fetches the text content of an embedded resource from the specified assembly using the specified encoding.
	/// </summary>
	/// <param name="asm">The assembly containing the resource.</param>
	/// <param name="resPath">The resource path.</param>
	/// <param name="encoding">The text encoding to use.</param>
	/// <returns>The text content of the resource.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	public static string FetchResourceText(this Assembly asm, string resPath, Encoding encoding)
	{
		resPath = asm.CorrectResourcePath(resPath);
		using var rs = asm.GetManifestResourceStream(resPath) ?? throw new InvalidOperationException($"Resource '{resPath}' not found in assembly '{asm.FullName}'");
		using var sr = new StreamReader(rs, encoding);
		return sr.ReadToEnd();
	}
	
	/// <summary>
	/// Fetches the text content of an embedded resource from the calling assembly asynchronously using UTF-8 encoding.
	/// </summary>
	/// <param name="resPath">The resource path.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The text content of the resource.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	public static async ValueTask<string> FetchResourceTextAsync(string resPath, CancellationToken cancellationToken = default) => await Assembly.GetCallingAssembly().FetchResourceTextAsync(resPath, Encoding.UTF8, cancellationToken);
	
	/// <summary>
	/// Fetches the text content of an embedded resource from the calling assembly asynchronously using the specified encoding.
	/// </summary>
	/// <param name="resPath">The resource path.</param>
	/// <param name="encoding">The text encoding to use.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The text content of the resource.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	public static async ValueTask<string> FetchResourceTextAsync(string resPath, Encoding encoding, CancellationToken cancellationToken = default) => await Assembly.GetCallingAssembly().FetchResourceTextAsync(resPath, encoding, cancellationToken);
	
	/// <summary>
	/// Fetches the text content of an embedded resource from the specified assembly asynchronously using UTF-8 encoding.
	/// </summary>
	/// <param name="asm">The assembly containing the resource.</param>
	/// <param name="resPath">The resource path.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The text content of the resource.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	public static async ValueTask<string> FetchResourceTextAsync(this Assembly asm, string resPath, CancellationToken cancellationToken = default) => await asm.FetchResourceTextAsync(resPath, Encoding.UTF8, cancellationToken);
	
	/// <summary>
	/// Fetches the text content of an embedded resource from the specified assembly asynchronously using the specified encoding.
	/// </summary>
	/// <param name="asm">The assembly containing the resource.</param>
	/// <param name="resPath">The resource path.</param>
	/// <param name="encoding">The text encoding to use.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The text content of the resource.</returns>
	/// <exception cref="InvalidOperationException">>Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	public static async ValueTask<string> FetchResourceTextAsync(this Assembly asm, string resPath, Encoding encoding, CancellationToken cancellationToken = default)
	{
		resPath = asm.CorrectResourcePath(resPath);
		await using var rs = asm.GetManifestResourceStream(resPath) ?? throw new InvalidOperationException($"Resource '{resPath}' not found in assembly '{asm.FullName}'");
		using var sr = new StreamReader(rs, encoding);
		return await sr.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
	}
	
	/// <summary>
	/// Fetches the binary content of an embedded resource from the calling assembly.
	/// </summary>
	/// <param name="resPath">The resource path.</param>
	/// <returns>The binary content of the resource.</returns>
	/// <exception cref="InvalidOperationException">>Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	public static byte[] FetchResourceBytes(string resPath) => Assembly.GetCallingAssembly().FetchResourceBytes(resPath);
	
	/// <summary>
	/// Fetches the binary content of an embedded resource from the specified assembly.
	/// </summary>
	/// <param name="asm">The assembly containing the resource.</param>
	/// <param name="resPath">>The resource path.</param>
	/// <returns>The binary content of the resource.</returns>
	/// <exception cref="InvalidOperationException">>Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	public static byte[] FetchResourceBytes(this Assembly asm, string resPath)
	{
		resPath = asm.CorrectResourcePath(resPath);
		using var rs = asm.GetManifestResourceStream(resPath) ?? throw new InvalidOperationException($"Resource '{resPath}' not found in assembly '{asm.FullName}'");
		using var ms = new MemoryStream();
		rs.CopyTo(ms);
		return ms.ToArray();
	}
	
	/// <summary>
	/// Fetches the binary content of an embedded resource from the calling assembly asynchronously.
	/// </summary>
	/// <param name="resPath">The resource path.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The binary content of the resource.</returns>
	/// <exception cref="InvalidOperationException">>Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	public static async ValueTask<byte[]> FetchResourceBytesAsync(string resPath, CancellationToken cancellationToken = default) => await Assembly.GetCallingAssembly().FetchResourceBytesAsync(resPath, cancellationToken);
	
	/// <summary>
	/// Fetches the binary content of an embedded resource from the specified assembly asynchronously.
	/// </summary>
	/// <param name="asm">The assembly containing the resource.</param>
	/// <param name="resPath">The resource path.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The binary content of the resource.</returns>
	/// <exception cref="InvalidOperationException">>Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	public static async ValueTask<byte[]> FetchResourceBytesAsync(this Assembly asm, string resPath, CancellationToken cancellationToken = default)
	{
		resPath = asm.CorrectResourcePath(resPath);
		await using var rs = asm.GetManifestResourceStream(resPath) ?? throw new InvalidOperationException($"Resource '{resPath}' not found in assembly '{asm.FullName}'");
		await using var ms = new MemoryStream();
		await rs.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
		return ms.ToArray();
	}
	
	/// <summary>
	/// Fetches the binary content of an embedded resource from the calling assembly as <see cref="ReadOnlyMemory{byte}"/>.
	/// </summary>
	/// <param name="resPath">The resource path.</param>
	/// <returns>The binary content of the resource.</returns>
	/// <exception cref="InvalidOperationException">>Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	public static ReadOnlyMemory<byte> FetchResourceBytesMemory(string resPath) => Assembly.GetCallingAssembly().FetchResourceBytesMemory(resPath);
	
	/// <summary>
	/// Fetches the binary content of an embedded resource from the specified assembly as <see cref="ReadOnlyMemory{byte}"/>.
	/// </summary>
	/// <param name="asm">The assembly containing the resource.</param>
	/// <param name="resPath">The resource path.</param>
	/// <returns>The binary content of the resource.</returns>
	/// <exception cref="InvalidOperationException">>Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	public static ReadOnlyMemory<byte> FetchResourceBytesMemory(this Assembly asm, string resPath)
	{
		resPath = asm.CorrectResourcePath(resPath);
		using var rs = asm.GetManifestResourceStream(resPath) ?? throw new InvalidOperationException($"Resource '{resPath}' not found in assembly '{asm.FullName}'");
		var memory = new Memory<byte>(new byte[rs.Length]);
		rs.ReadExactly(memory.Span);
		return memory;
	}
	
	/// <summary>
	/// Fetches the binary content of an embedded resource from the calling assembly asynchronously as <see cref="ReadOnlyMemory{byte}"/>.
	/// </summary>
	/// <param name="resPath">The resource path.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The binary content of the resource.</returns>
	/// <exception cref="InvalidOperationException">>Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	public static async ValueTask<ReadOnlyMemory<byte>> FetchResourceBytesMemoryAsync(string resPath, CancellationToken cancellationToken = default) => await Assembly.GetCallingAssembly().FetchResourceBytesMemoryAsync(resPath, cancellationToken);
	
	/// <summary>
	/// Fetches the binary content of an embedded resource from the specified assembly asynchronously as <see cref="ReadOnlyMemory{byte}"/>.
	/// </summary>
	/// <param name="asm">The assembly containing the resource.</param>
	/// <param name="resPath">The resource path.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The binary content of the resource.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the resource is not found in the assembly.</exception>
	/// <seealso cref="FetchResourceBytesMemory(string)"/>
	/// <seealso cref="FetchResourceBytesMemory(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesMemoryAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytes(string)"/>
	/// <seealso cref="FetchResourceBytes(Assembly, string)"/>
	/// <seealso cref="FetchResourceBytesAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceBytesAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceText(string)"/>
	/// <seealso cref="FetchResourceText(string, Encoding)"/>
	/// <seealso cref="FetchResourceText(Assembly, string)"/>
	/// <seealso cref="FetchResourceText(Assembly, string, Encoding)"/>
	/// <seealso cref="FetchResourceTextAsync(string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(string, Encoding, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, CancellationToken)"/>
	/// <seealso cref="FetchResourceTextAsync(Assembly, string, Encoding, CancellationToken)"/>
	public static async ValueTask<ReadOnlyMemory<byte>> FetchResourceBytesMemoryAsync(this Assembly asm, string resPath, CancellationToken cancellationToken = default)
	{
		resPath = asm.CorrectResourcePath(resPath);
		await using var rs = asm.GetManifestResourceStream(resPath) ?? throw new InvalidOperationException($"Resource '{resPath}' not found in assembly '{asm.FullName}'");
		var memory = new Memory<byte>(new byte[rs.Length]);
		await rs.ReadExactlyAsync(memory, cancellationToken).ConfigureAwait(false);
		return memory;
	}
	
	/// <summary>
	/// Attempts to correct the resource path to match the assembly's manifest resource naming convention.
	/// </summary>
	/// <param name="asm">The assembly containing the resource.</param>
	/// <param name="resPath">The original resource path.</param>
	/// <returns>The corrected resource path, formatted as <c>AssemblyName.Resource.Path</c> to match the manifest resource naming convention.</returns>
	/// <exception cref="InvalidOperationException">Thrown if there is no <see cref="AssemblyName"/> or if the resource is not found in the assembly.</exception>
	public static string CorrectResourcePath(this Assembly asm, string resPath)
	{
		if (asm.GetManifestResourceNames().Contains(resPath))
		{
			return resPath;
		}
    
		var asmName = asm.GetName().Name;
		if (asmName is null)
		{
			throw new InvalidOperationException("Assembly has no name.");
		}
    
		var normalizedPath = resPath.Replace('/', '.').Replace('\\', '.').ReplaceAll(InvalidPathCharacters, '_');
		
		if (normalizedPath.StartsWith(asmName + "."))
		{
			normalizedPath = normalizedPath[(asmName.Length + 1)..];
		}
    
		var correctedPath = $"{asmName}.{normalizedPath}";
    
		if (asm.GetManifestResourceNames().Contains(correctedPath))
		{
			return correctedPath;
		}
    
#if DEBUG
		Debug.WriteLine("Available resources:");
		foreach (var name in asm.GetManifestResourceNames())
		{
			Debug.WriteLine($"  {name}");
		}
#endif
		
		throw new InvalidOperationException($"Resource '{resPath}' not found in assembly '{asm.FullName}'. Corrected path: '{correctedPath}'");
	}
}