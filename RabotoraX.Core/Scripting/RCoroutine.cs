using System.Collections;

namespace RabotoraX.Core.Scripting;

public sealed class RCoroutine : Object
{
	internal readonly IEnumerator Routine;
	internal RObject? Owner;
	internal bool IsRunning;
	
	internal RCoroutine(IEnumerator routine, RObject? owner)
	{
		Routine = routine;
		Owner = owner;
		IsRunning = true;
	}
	
	public void Stop()
	{
		IsRunning = false;
	}
}