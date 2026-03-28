using System.Collections;
using RabotoraX.Core.Scripting;
using RabotoraX.Core.Tweening;
using RabotoraX.Core.UI;

namespace RabotoraX.Windows.Test.Demo2D;

public sealed class MovingText : RManagedScript
{
	private Text _text = null!;

	public override void OnAwake()
	{
		base.OnAwake();
		_text = GetComponent<Text>()!;
	}

	public override void OnStart()
	{
		base.OnStart();
		StartCoroutine(AnimateCoroutine());
	}

	private IEnumerator AnimateCoroutine()
	{
		yield return new WaitForSeconds(1.5f);
		yield return _text.UILayout.RaMoveX(350, 5f).SetEase(Ease.InOutSine);
	}
}