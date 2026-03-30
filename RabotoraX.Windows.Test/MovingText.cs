using System.Collections;
using RabotoraX.Core.Scripting;
using RabotoraX.Core.Tweening;
using RabotoraX.Core.UI;
using RabotoraX.Core.Threading.Tasks;

namespace RabotoraX.Windows.Test.Demo2D;

public sealed class MovingText : RManagedScript
{
	private Text _text = null!;

	public override void OnAwake()
	{
		base.OnAwake();
		_text = GetComponent<Text>()!;
	}

	// public override void OnStart()
	// {
	// 	base.OnStart();
	// 	StartCoroutine(AnimateCoroutine());
	// }
	//
	// private IEnumerator AnimateCoroutine()
	// {
	// 	yield return new WaitForSeconds(1.5f);
	// 	yield return _text.UILayout.RaMoveX(350, 5f).SetEase(Ease.InOutSine);
	// }

	public override void OnStart()
	{
		base.OnStart();
		RTask.Create(AnimateAsync).Forget();
	}

	private async RTask AnimateAsync()
	{
		await RTask.SwitchToMainThread();
		await RTask.Delay(1500);
		await _text.UILayout.RaMoveX(350, 5f).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
		Console.WriteLine("Move done!");
	}
}