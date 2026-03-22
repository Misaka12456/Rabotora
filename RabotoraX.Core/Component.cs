using RabotoraX.Core.Cinematics;
using RabotoraX.Core.Graphics;

namespace RabotoraX.Core
{
	public abstract class Component : Object
	{
		public RObject RObject { get; internal set; } = null!;
		public RLayout Layout => RObject.Layout;
	
		public bool IsEnabled { get; set; } = true;
	
		public virtual void OnAwake() { }
		public virtual void OnStart() { }
		public virtual void OnUpdate(float deltaTime) { }
		public virtual void OnRender(INativeCommandList cmd) { }

		public virtual void OnRender2D(INative2DRenderContext context)
		{
			if (RObject.Stage.Type is StageType.Render2D or StageType.Render3DHybrid) return;
			throw new PlatformNotSupportedException("2D rendering is not supported in 3D stages. If you want to use 2D rendering, please create a stage with StageType.Render2D or use a Component2D instead.");
		}
		
		public T? GetComponent<T>() where T : Component
		{
			return RObject.GetComponent<T>();
		}
		
		public T? GetComponentInChildren<T>() where T : Component
		{
			return RObject.GetComponentInChildren<T>();
		}
	}
}