using RabotoraX.Core.Inputs;
using RabotoraX.Core.Scripting;
using RabotoraX.Core.UI;

namespace RabotoraX.Windows.Test.Demo2D;

public sealed class InputTest : RManagedScript
{
	private Text _text = null!;

	public override void OnAwake()
	{
		base.OnAwake();
		_text = GetComponent<Text>()!;
	}

	public override void OnUpdate(float deltaTime)
	{
		_text.IsEnabled = Input.GetMouseButton(0);
	}
}