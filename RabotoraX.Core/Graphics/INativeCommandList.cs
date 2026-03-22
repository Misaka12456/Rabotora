using JetBrains.Annotations;

namespace RabotoraX.Core.Graphics;

public interface INativeCommandList : IDisposable
{
	void Begin();
	void End();
	
	void BeginRenderPass(INativeRenderTexture? renderTexture);
	void EndRenderPass();
	void Clear(float r, float g, float b, float a);
	
	void SetViewport(float x, float y, float width, float height, float minDepth = 0, float maxDepth = 1);
	
	void SetShader(INativeShader shader);
	void SetVertexBuffer(IGpuBuffer buffer, int stride, int offset = 0);
	void SetIndexBuffer(IGpuBuffer buffer);
	void SetConstantBuffer(int slot, IGpuBuffer buffer, ShaderType stage);
	
	[MustDisposeResource] IDisposable SetCullMode(CullMode mode); // 只作用于到下次调用SetCullMode/ResumeCullMode为止/ICommandList结束的Draw Call
	[MustDisposeResource] IDisposable SetBlendState(BlendState state); // 只作用于到下次调用SetBlendState/ResumeBlendState为止/ICommandList结束的Draw Call
	void ResumeCullMode(); // 恢复到上次调用SetCullMode之前的状态
	void ResumeBlendState(); // 恢复到上次调用SetBlendState之前的状态
	void SetDepthEnabled(bool enabled, bool writeEnabled = true);
	
	void Draw(int vertexCount, int startVertexLocation, PrimitiveTopology topology = PrimitiveTopology.TriangleList);
	void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation, PrimitiveTopology topology = PrimitiveTopology.TriangleList);
}