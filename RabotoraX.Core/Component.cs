using System.Collections;
using JetBrains.Annotations;
using RabotoraX.Core.Cinematics;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Scripting;
using RabotoraX.Core.UI;

namespace RabotoraX.Core
{
	public abstract class Component : Object
	{
		public RObject RObject { get; internal set; } = null!;
		public RLayout Layout
		{
			get
			{
				CheckReady();
				return RObject.Layout;
			}
		}

		public bool IsEnabled { get; set; } = true;
		private bool _isAwakened;

		public virtual void OnAwake()
		{
			_isAwakened = true;
		}
		public virtual void OnStart() { }
		public virtual void OnUpdate(float deltaTime) { }
		public virtual void OnRender(INativeCommandList cmd) { }

		public virtual void OnRender2D(INative2DRenderContext context)
		{
			if (RObject.Stage.Type is StageType.Render2D or StageType.Render3DHybrid) return;
			throw new PlatformNotSupportedException("2D rendering is not supported in 3D stages. If you want to use 2D rendering, please create a stage with StageType.Render2D or use a Component2D instead.");
		}
		
		[UsedImplicitly]
		public T? GetComponent<T>() where T : Component
		{
			return RObject.GetComponent<T>();
		}
		
		[UsedImplicitly]
		public T? GetComponentInChildren<T>() where T : Component
		{
			return RObject.GetComponentInChildren<T>();
		}
		
		[UsedImplicitly]
		public RCoroutine StartCoroutine(IEnumerator routine)
		{
			CheckReady();
			return RObject.StartCoroutine(routine);
		}
		
		[UsedImplicitly]
		public void StopCoroutine(RCoroutine coroutine)
		{
			CheckReady();
			RObject.StopCoroutine(coroutine);
		}

		protected void CheckReady()
		{
			if (!_isAwakened)
			{
				throw new RabotoraException("Component is not ready. " +
				                            "Access to RObject-dependent properties/methods may cause unexpected NullReferenceExceptions from constructors or field initializers " +
				                            "as they are called before the component is fully initialized. " +
				                            "Consider moving such code to OnAwake or a later lifecycle method instead.");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this is not RLayout)
			{
				RObject.RemoveComponent(this);
			}
			base.Dispose(disposing);
		}
	}
}