using RabotoraX.Core.Graphics;
using Vortice.Direct3D11;

namespace RabotoraX.Interop.Direct3D11;

internal class DX11Shader : IShader
{
	public ID3D11DeviceChild NativeShader { get; } // VertexShader or PixelShader
	public ID3D11InputLayout? InputLayout { get; } // 仅 VertexShader 需要
	public byte[]? ByteCode { get; }
	public ShaderType Type { get; }
	
	public DX11Shader(ID3D11DeviceChild shader, ShaderType type, ID3D11InputLayout? inputLayout = null, byte[]? byteCode = null)
	{
		NativeShader = shader;
		Type = type;
		InputLayout = inputLayout;
		ByteCode = byteCode;
	}

	public void Dispose()
	{
		InputLayout?.Dispose();
		NativeShader.Dispose();
	}
}