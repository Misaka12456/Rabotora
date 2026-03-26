using System.Diagnostics.CodeAnalysis;
using RabotoraX.Core.Diagnostics;

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
	IEnumerable<ShaderPlatform> GetSupportedShaderPlatforms(); // 获取当前图形API支持的着色器平台列表 (比如DX11/DX12/Vulkan/SPIR-V/Metal等)，用于在运行时选择合适的着色器编译目标
	
	// 帧控制
	void BeginFrame();
	void EndFrame();
	void Present(bool vsync = true); // 分开EndFrame和Present是因为某些图形后端 (比如Vulkan/DX12) 是拆开处理的，而对于那些不区分的后端 (比如OpenGL) 则EndFrame里直接调用Present即可
	void WaitNextFrameReady();

	INativeCommandList CreateCommandList();
	void Submit(INativeCommandList commandList);
	
	#region 资源创建
	// 创建缓冲区 (顶点/索引/常量)
	IGpuBuffer CreateBuffer<T>(BufferType type, T[] data) where T : unmanaged;
	// 创建空缓冲区 (用于动态更新)
	IGpuBuffer CreateBuffer(BufferType type, int sizeInBytes);
	// 更新缓冲区数据
	void UpdateBuffer<T>(IGpuBuffer buffer, T[] data) where T : unmanaged;
	void UpdateBuffer<T>(IGpuBuffer buffer, ref T data) where T : unmanaged;
	
	// 创建着色器(Shader) (包含输入布局，对DX12/Vulkan等需要显式输入布局的后端；OpenGL此值可选)
	INativeShader CreateNativeShader(ShaderType type, string sourceCode, string entryPoint = "main", InputElementDescription[]? inputLayout = null);
	INativeShader CreateNativeShaderProgram(string vertSource, string fragSource, InputElementDescription[] inputLayout, string vertEntryPoint = "main", string fragEntryPoint = "main");
	#endregion
	
	#region 纹理与渲染目标

	INativeTexture2D CreateTexture2D(int width, int height, ReadOnlySpan<byte> pixelData);
	INativeRenderTexture CreateRenderTexture(int width, int height);
	void UpdateTexture2D(INativeTexture2D texture, ReadOnlySpan<byte> pixelData, int stride = 0);
	INative2DRenderContext? Get2DContext();
	#endregion
	
	// 纹理与渲染目标在RabotoraX v0.5.3+已移动至INativeCommandList
	
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.Direct3D11.DirectX11", "RabotoraX.Interop.Direct3D11")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.Direct3D12.DirectX12", "RabotoraX.Interop.Direct3D12")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.Vulkan.Vulkan", "RabotoraX.Interop.Vulkan")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.Metal.Metal", "RabotoraX.Interop.Metal")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "RabotoraX.Interop.OpenGL.OpenGL", "RabotoraX.Interop.OpenGL")]
	public static INativeGraphicsAPI Create()
	{
#if DEBUG
		var dialog = INativeGraphicsAPISelectDialog.PlatformCreate();
		if (dialog != null && dialog.GetIsUserRequestedToSelect())
		{
			var type = dialog.ShowDialog();
			if (type != null)
			{
				return (INativeGraphicsAPI)Activator.CreateInstance(type)!;
			}
			Environment.Exit(0);
		}
#endif
		return PlatformDefaultCreate();
	}

	private static INativeGraphicsAPI PlatformDefaultCreate()
	{
		Type? implType;
		try
		{
			implType = OperatingSystem.IsWindows() ? Type.GetType("RabotoraX.Interop.Direct3D12.DirectX12, RabotoraX.Interop.Direct3D12") :
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