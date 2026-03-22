using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.Scripting;

public abstract class RScript : Component
{
	public abstract void OnDestroy();
}

public abstract class RManagedScript : RScript
{
	public override void OnAwake()
	{
		
	}
	
	public override void OnStart()
	{
		
	}
	
	public override void OnUpdate(float deltaTime)
	{
		
	}
	
	public override void OnRender(INativeCommandList cmd)
	{
		
	}
	
	public override void OnDestroy()
	{
		
	}
}