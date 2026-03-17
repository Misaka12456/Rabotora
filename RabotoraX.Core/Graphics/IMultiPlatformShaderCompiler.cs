namespace RabotoraX.Core.Graphics;

public interface IMultiPlatformShaderCompiler
{
	void CompileInto(INativeShader target, ShaderPlatform platform, string vert, string frag);
}