using RabotoraX.Core.Graphics;

namespace RabotoraX.Interop.Direct3D12.Rendering;

public class DX12ShaderProgram : INativeShader
{
	public byte[] VSByteCode { get; }
	public byte[] PSByteCode { get; }
	public string? VertSource { get; init; }
	public string? FragSource { get; init; }
	public Vortice.Direct3D12.InputElementDescription[] InputElements { get; }

	public ShaderType Type => ShaderType.VertexFragment;
	
	public DX12ShaderProgram(byte[] vsByteCode, byte[] psByteCode, Vortice.Direct3D12.InputElementDescription[] inputElements)
	{
		VSByteCode = vsByteCode;
		PSByteCode = psByteCode;
		InputElements = inputElements;
	}

	public void Dispose()
	{
		// No unmanaged resources to dispose in this class, but we implement IDisposable for consistency and future-proofing.
		GC.SuppressFinalize(this);
	}
}