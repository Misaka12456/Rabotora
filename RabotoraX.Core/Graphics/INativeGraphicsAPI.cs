using JetBrains.Annotations;

namespace RabotoraX.Core.Graphics;

public interface INativeGraphicsAPI : IDisposable
{
	string ApiName { get; }
	string DeviceName { get; }
	bool IsInitialized { get; }
	(int Width, int Height) FramebufferSize { get; }
	Lock RenderLock { get; } // 用于在多线程环境下同步渲染操作的锁对象，确保同一时间只有一个线程在执行渲染相关的操作
	bool IgnoreAllPresents { get; set; }

	// 初始化与生命周期
	void Initialize(INativeWindow window);
	void Resize(int width, int height);
	void WaitIdle();
	
	// 帧控制
	void BeginFrame();
	void Clear(float r, float g, float b, float a);
	void EndFrame();
	void Present(bool vsync = true); // 分开EndFrame和Present是因为某些图形后端 (比如Vulkan/DX12) 是拆开处理的，而对于那些不区分的后端 (比如OpenGL) 则EndFrame里直接调用Present即可
	
	#region 资源创建
	// 创建缓冲区 (顶点/索引/常量)
	IGpuBuffer CreateBuffer<T>(BufferType type, T[] data) where T : unmanaged;
	// 创建空缓冲区 (用于动态更新)
	IGpuBuffer CreateBuffer(BufferType type, int sizeInBytes);
	// 更新缓冲区数据
	void UpdateBuffer<T>(IGpuBuffer buffer, T[] data) where T : unmanaged;
	void UpdateBuffer<T>(IGpuBuffer buffer, ref T data) where T : unmanaged;
	
	// 创建着色器(Shader) (包含输入布局，对DX12/Vulkan等需要显式输入布局的后端；OpenGL此值可选)
	IShader CreateShader(ShaderType type, string sourceCode, string entryPoint = "main", InputElementDescription[]? inputLayout = null);
	IShader CreateShaderProgram(string vertSource, string fragSource, InputElementDescription[] inputLayout, string vertEntryPoint = "main", string fragEntryPoint = "main");
	#endregion
	
	#region 管线命令 (Draw Loop)
	// 设置视口 (Viewport)
	void SetViewport(float x, float y, float width, float height, float minDepth = 0, float maxDepth = 1);

	// 绑定状态
	void SetShader(IShader shader);
	void SetVertexBuffer(IGpuBuffer buffer, int stride, int offset = 0);
	void SetIndexBuffer(IGpuBuffer buffer);
	void SetConstantBuffer(int slot, IGpuBuffer buffer, ShaderType stage);
	
	[MustDisposeResource] IDisposable SetCullMode(CullMode mode); // 只作用于到下次调用SetCullMode/ResumeCullMode为止的Draw Call
	void ResumeCullMode(); // 恢复到上次调用SetCullMode之前的状态
	void SetDepthEnabled(bool enabled, bool writeEnabled = true);
	
	// 绘制
	void Draw(int vertexCount, int startVertexLocation, PrimitiveTopology topology = PrimitiveTopology.TriangleList);
	void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation, PrimitiveTopology topology = PrimitiveTopology.TriangleList);
	
	INative2DRenderContext? Get2DContext(); // 获取2D渲染上下文，如果当前图形API不支持则返回null
	#endregion
	
	
	public static INativeGraphicsAPI PlatformDefaultCreate()
	{
		Type? implType;
		try
		{
			implType = OperatingSystem.IsWindows() ? Type.GetType("RabotoraX.Interop.Direct3D11.DirectX11, RabotoraX.Interop.Direct3D11") :
				OperatingSystem.IsLinux() ? Type.GetType("RabotoraX.Interop.Vulkan.Vulkan, RabotoraX.Interop.Vulkan") :
				OperatingSystem.IsMacOS() ? Type.GetType("RabotoraX.Interop.Metal.Metal, RabotoraX.Interop.Metal") :
				OperatingSystem.IsAndroid() ? Type.GetType("RabotoraX.Interop.Vulkan.Vulkan, RabotoraX.Interop.Vulkan") 
				                              ?? Type.GetType("RabotoraX.Interop.OpenGL.OpenGLEs3, RabotoraX.Interop.OpenGL") :
				OperatingSystem.IsIOS() ? Type.GetType("RabotoraX.Interop.Metal.Metal, RabotoraX.Interop.Metal") :
				throw new PlatformNotSupportedException("Unsupported platform");
		}
		catch
		{
			implType = null;
		}
		if (implType == null)
		{
			throw new NotImplementedException("This functionality is not implemented in the portable version of this assembly. " +
			                                  "You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
		}
		return (INativeGraphicsAPI)Activator.CreateInstance(implType)!;
	}
}