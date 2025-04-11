using Rabotora.Graphics;
using NativeWindow = Rabotora.Graphics.NativeWindow;
using Rectangle = Rabotora.Core.Rectangle;

namespace MyFirstGame;

public class MyFirstGame : RWindow
{
	private readonly SplashComponent _splash;
	
	public MyFirstGame() : base(1280, 720, "My First Game - ラボトラ Rabotora DirectX Debug")
	{
		RenderContext = new DirectXRenderContext(this); // 注入渲染上下文
		_splash = new SplashComponent(
			["Data/Unpacked/Sprites/splash_ageWarning.png", "Data/Unpacked/Sprites/splash_devLogo.png"],
			new Rectangle(0, 0, Width, Height)
		);
		AddComponent(_splash);
		Resized += () => _splash.Bounds = new Rectangle(0, 0, Width, Height);
	}

	public override void Run()
	{
		Task.Run(_splash.PlayAnimationAsync);
		base.Run();
	}
}