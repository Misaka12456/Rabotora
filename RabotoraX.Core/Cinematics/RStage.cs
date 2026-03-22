using System.Numerics;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.UI;
using ZLinq;

namespace RabotoraX.Core.Cinematics;

public class RStage : Object
{
	public string Name { get; set; }
	public StageType Type { get; set; } = StageType.Render3D;
	
	/// <summary>
	/// Represents the color used to clear the stage before rendering each frame.<br />
	/// This value will not be used if the active stage is <see cref="StageType.Render3D"/> stage; instead, they will be cleared by the <see cref="Audience" />.
	/// </summary>
	public Vector4 ClearColor { get; set; } = new Vector4(0, 0, 0, 1); // black
	public IReadOnlyList<RObject> RootObjects => _rootObjects;
	private readonly List<RObject> _rootObjects = [];

	public RStage(string name)
	{
		Name = name;
	}

	public RObject CreateObject(string name)
	{
		var ro = new RObject(name) { Stage = this };
		_rootObjects.Add(ro);
		return ro;
	}
		
	public void DestroyObject(RObject ro)
	{
		if (_rootObjects.Remove(ro))
		{
			ro.Dispose();
		}
	}

	public void Update(float deltaTime)
	{
		foreach (var ro in _rootObjects.AsValueEnumerable().Where(ro => ro.IsActive)) // it's already a copy after where'd, so we can safely call Update on them without worrying about modifications to the list during iteration.
		{
			ro.Update(deltaTime);
		}
	}

	public void Render()
	{
		var cmd = GraphicsService.API.CreateCommandList();
		cmd.Begin();
		try
		{
			if (Type is StageType.Render3D or StageType.Render3DHybrid)
			{
				try
				{
					cmd.BeginRenderPass(null);
					Audience.Main?.Clear(cmd);
					// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
					foreach (var ro in _rootObjects)
					{
						if (ro.IsActive) ro.Render(cmd);
					}
				}
				finally
				{
					cmd.EndRenderPass();
				}
			}

			if (Type is StageType.Render2D or StageType.Render3DHybrid)
			{
				var _2d = GraphicsService.API.Get2DContext();
				if (_2d != null)
				{
					if (Type == StageType.Render2D)
					{
						cmd.Clear(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);
					}

					_2d.BeginDraw();

					foreach (var ro in _rootObjects.AsValueEnumerable().Where(ro => ro.IsActive))
					{
						ro.Render2D(_2d);
					}

					_2d.EndDraw();
				}
			}
		}
		finally
		{
			cmd.End();
		}
	}

	public void GetRenderGroups(out List<RObject> worldObjects, out List<RObject> uiObjects)
	{
		worldObjects = new List<RObject>();
		uiObjects = new List<RObject>();

		foreach (var obj in _rootObjects) // 遍历根节点
		{
			// 如果物体本身是 Canvas 或者含有 RUILayout，归类为 UI
			if (obj.GetComponent<RCanvas>() != null || obj.GetComponent<RUILayout>() != null)
			{
				uiObjects.Add(obj);
			}
			else
			{
				worldObjects.Add(obj);
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (var ro in _rootObjects)
			{
				ro.Dispose();
			}
			_rootObjects.Clear();
		}
		base.Dispose(disposing);
	}
}