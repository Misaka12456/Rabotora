using RabotoraX.Core.Graphics;

namespace RabotoraX.Interop.Direct3D12.Rendering;

public class DX12Shader : INativeShader
{
	public byte[] ByteCode { get; }
	public string? VertSource { get; init; }
	public string? FragSource { get; init; }
	public ShaderType Type { get; }
	public Vortice.Direct3D12.InputElementDescription[]? InputElements { get; }

	public DX12Shader(byte[] byteCode, ShaderType type, Vortice.Direct3D12.InputElementDescription[]? layout)
	{
		ByteCode = byteCode;
		Type = type;
		InputElements = layout;
	}
	public void Dispose() { }
}