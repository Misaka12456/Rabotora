using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Versioning;
using RabotoraX.Core;
using RabotoraX.Core.Audios;
using RabotoraX.Core.Cinematics;
using RabotoraX.Core.Graphics;
using RabotoraX.Core.Mathematics;
using RabotoraX.Core.Resources;
using RabotoraX.Core.Test;
using RabotoraX.Core.UI;
using RabotoraX.Core.Videos;
using RabotoraX.Windows.Test.Demo2D;
using RabotoraX.Windows.Test.Demo3D;

namespace RabotoraX.Windows.Test;

[SupportedOSPlatform("windows")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public static class Program
{
	[STAThread]
	public static int Main(string[] args)
	{
		using var app = new Rabotora("Example Presentation", 1280, 720, new Fractional(16, 9));

		return app.Run(Example3DHybridStage());
	}

	private static RStage Example3DStage()
	{
		var stage = new RStage("Act 1: Hello RabotoraX");
		var audObj = stage.CreateObject("MainAudience");
		audObj.Layout.Position = new Vector3(0, 0, 5);
		var aud = audObj.AddComponent<Audience>();
		aud.ClearConfig.ClearColor = new Vector4(0.1f, 0.1f, 0.1f, 1); // dark gray background

		var cubeObj = stage.CreateObject("RotatingCube");
		cubeObj.Layout.Position = Vector3.Zero;
		cubeObj.AddComponent<QuadRenderer>();
		cubeObj.AddComponent<ColorTilt>();
		cubeObj.AddComponent<RotatingCube>();

		var cubeObj2 = stage.CreateObject("StaticCube");
		cubeObj2.Layout.Position = Vector3.Zero;
		cubeObj2.Layout.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.5f); // rotate 90 degrees around Y axis
		var r2 = cubeObj2.AddComponent<QuadRenderer>();
		r2.Color = new Vector4(0, 0, 1, 1);
		return stage;
	}

	private static RStage Example3DHybridStage()
	{
		var stage = new RStage("Act 1: Hello RabotoraX") {Type = StageType.Render3DHybrid}; // no need to do 
		var audObj = stage.CreateObject("MainAudience");
		audObj.Layout.Position = new Vector3(0, 0, 5);
		audObj.AddComponent<Audience>();
		audObj.AddComponent<RAudioListener>();

		var cubeObj = stage.CreateObject("RotatingCube");
		cubeObj.Layout.Position = Vector3.Zero;
		cubeObj.AddComponent<QuadRenderer>();
		cubeObj.AddComponent<ColorTilt>();
		cubeObj.AddComponent<RotatingCube>();
		var player = cubeObj.AddComponent<RAudioPlayer>();
		using var pkg = RDataPackage.Open("Resources/Demo3D_Data.pkg");
		using (var stream = pkg.OpenEntry("Assets/Audios/SneakySnitch.ogg") ?? throw new InvalidOperationException("Failed to load audio from package."))
		{
			player.Clip = AudioClip.LoadFromStream(stream, AudioFormatType.OggVorbis);
		}

		player.Is3D = true;
		player.Loop = true;
		player.Play();

		var cubeObj2 = stage.CreateObject("StaticCube");
		cubeObj2.Layout.Position = Vector3.Zero;
		cubeObj2.Layout.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.5f); // rotate 90 degrees around Y axis
		var r2 = cubeObj2.AddComponent<QuadRenderer>();
		r2.Color = new Vector4(0, 0, 1, 1);

		var canvasObj = stage.CreateObject("Canvas");
		var canvas = canvasObj.AddComponent<RCanvas>();
		canvas.ReferenceResolution = new Vector2(1280, 720);
		canvas.ScaleMode = CanvasScaleMode.ScaleWithScreenSize;
		canvas.MatchMode = ScreenMatchMode.MatchWidthOrHeight;
		canvas.MatchWidthOrHeight = 0.5f;

		var text = stage.CreateObject("HelloText").AddComponent<Text>();
		text.Content = "RabotoraX 3D Hybrid (3D + 2D) Stage 示例\nRabotoraX 3D Hybrid (3D + 2D) Stage Example";
		text.Color = Color.White;
		text.Layout.SetParent(canvasObj.Layout);
		((RUILayout)text.Layout).AnchorMin = ((RUILayout) text.Layout).AnchorMax = new Vector2(0.5f, 0.5f);
		((RUILayout)text.Layout).Pivot = new Vector2(0.5f, 0.5f);
		((RUILayout)text.Layout).Size = new Vector2(600, 200);
		((RUILayout)text.Layout).AnchoredPosition = Vector2.Zero;
		text.RObject.AddComponent<MovingText>();

		return stage;
	}

	private static RStage Example2DStage()
	{
	    var stage = new RStage("Act 1: Hello RabotoraX") { Type = StageType.Render2D, ClearColor = new Vector4(0, 0, 0, 1) };

	    // Canvas
	    var canvasObj = stage.CreateObject("Canvas");
	    var canvas = canvasObj.AddComponent<RCanvas>();
	    canvas.ReferenceResolution = new Vector2(1280, 720);
	    canvas.ScaleMode = CanvasScaleMode.ScaleWithScreenSize;
	    canvas.MatchMode = ScreenMatchMode.MatchWidthOrHeight;
	    canvas.MatchWidthOrHeight = 0.5f;

	    // Vertical Layout Container
	    var containerObj = stage.CreateObject("VerticalContainer");
	    containerObj.Layout.SetParent(canvas.Layout);
	    var containerLayout = containerObj.AddComponent<RUILayout>(); // cast to RUILayout after being a child of canvas to integrate auto-RUILayout behavior
	    containerLayout.AnchorMin = containerLayout.AnchorMax = new Vector2(0, 0);
	    containerLayout.Pivot = new Vector2(0, 0);
	    containerLayout.AnchoredPosition = new Vector2(150, 250);
	    containerLayout.Size = new Vector2(300, 200);

	    // Config Vertical Layout Group
	    var vGroup = containerObj.AddComponent<UIVerticalLayoutGroup>();
	    vGroup.PaddingLeft = 10;
	    vGroup.PaddingTop = 10;
	    vGroup.Spacing = new Vector2(0, 0);
	    vGroup.ChildAlignment = UIAlignment.UpperLeft;
	    vGroup.ControlChildWidth = true;
	    vGroup.ControlChildHeight = true;
	    
	    var o1 = stage.CreateObject("Text1");
	    var t1 = o1.AddComponent<Text>();
	    o1.Layout.SetParent(containerLayout);
	    t1.Color = Color.White;
	    t1.FontSize = 24;
	    t1.FontName = "Microsoft YaHei UI";
	    t1.Content = "扫码缴费";

	    var o2 = stage.CreateObject("Text2");
	    var t2 = o2.AddComponent<Text>();
	    o2.Layout.SetParent(containerLayout);
	    t2.Color = Color.White;
	    t2.FontSize = 24;
	    t2.FontName = "Microsoft YaHei UI";
	    t2.Content = "快速离场";

	    return stage;
	}

	private static RStage Example2DGoLiveStage()
	{
		var stage = new RStage("Splash") {Type = StageType.Render2D, ClearColor = new(0, 0, 0, 1)};

		var canvasObj = stage.CreateObject("Canvas");
		var canvas = canvasObj.AddComponent<RCanvas>();
		canvas.ReferenceResolution = new Vector2(1280, 720);
		canvas.ScaleMode = CanvasScaleMode.ScaleWithScreenSize;
		canvas.MatchMode = ScreenMatchMode.MatchWidthOrHeight;
		canvas.MatchWidthOrHeight = 0.5f;

		var imageObj = stage.CreateObject("SplashImage");
		var image = imageObj.AddComponent<Image>();
		var imgLayout = (RUILayout) image.Layout;
		imgLayout.SetParent(canvas.Layout);

		imgLayout.AnchorMin = Vector2.Zero;
		imgLayout.AnchorMax = Vector2.One;
		imgLayout.OffsetMin = imgLayout.OffsetMax = Vector2.Zero;
		image.Opacity = 1;
		using var pkg = RDataPackage.Open("Resources/Demo2D_Data.pkg");
		using var stream = pkg.OpenEntry("Assets/Sprites/Splash.png") ?? throw new InvalidOperationException("Failed to load image from package.");
		var tex = new Texture2D();
		tex.Load2DImage(stream);
		image.Sprite = Sprite.Create(tex);
		image.IsEnabled = true;
		var button = imageObj.AddComponent<Button>();
		button.TargetRenderable = image;
		button.Transition = SelectableTransition.Opacity;
		button.StateOpacities = new ButtonStateOpacities()
		{
			NormalOpacity = 1.0f,
			PressedOpacity = 0.75f,
		};
		button.EventOnClick += () =>
		{
			Console.WriteLine("Clicked!");
		};
		button.IsEnabled = true;

		return stage;
	}

	private static RStage Example2DGoLiveStage2()
	{
		var stage = new RStage("Splash") {Type = StageType.Render2D, ClearColor = new(0, 0, 0, 1)};

		var canvasObj = stage.CreateObject("Canvas");
		var canvas = canvasObj.AddComponent<RCanvas>();
		canvas.ReferenceResolution = new Vector2(1280, 720);
		canvas.ScaleMode = CanvasScaleMode.ScaleWithScreenSize;
		canvas.MatchMode = ScreenMatchMode.MatchWidthOrHeight;
		canvas.MatchWidthOrHeight = 0.5f;
		canvasObj.AddComponent<RAudioListener>();

		var imageObj = stage.CreateObject("SplashVideo");
		var image = imageObj.AddComponent<RawImage>();
		var imgLayout = (RUILayout) image.Layout;
		imgLayout.SetParent(canvas.Layout);
		
		imgLayout.AnchorMin = Vector2.Zero;
		imgLayout.AnchorMax = Vector2.One;
		imgLayout.OffsetMin = imgLayout.OffsetMax = Vector2.Zero;
		image.Opacity = 1;
		var clip = new VideoClip(new FileStream("<YOUR_VIDEO_PATH_HERE>", FileMode.Open, FileAccess.Read));
		var player = imageObj.AddComponent<RVideoPlayer>();
		player.Clip = clip;
		player.Prepare();
		image.Texture = player.Texture; // assign the player's texture to the RawImage
		var vtp = imageObj.AddComponent<VideoTexturePlayer>();
		
		var statusTextObj = stage.CreateObject("StatusText");
		var statusText = statusTextObj.AddComponent<Text>();
		var statusLayout = (RUILayout) statusText.Layout;
		statusLayout.SetParent(canvas.Layout);
		
		statusLayout.AnchorMin = statusLayout.AnchorMax = new Vector2(0, 0); // Top-left corner
		statusLayout.Pivot = new Vector2(0, 0); // Set pivot to top-left for easier positioning
		statusLayout.AnchoredPosition = new Vector2(10, 10); // 10 pixels from the top-left corner
		statusLayout.Size = new Vector2(400, 50); // Set a fixed size for the status text
		statusText.Color = Color.Black; // Black text
		statusText.FontName = "Microsoft YaHei UI";
		statusText.FontSize = 18;
		statusText.Content = "Idle";
		vtp._statusText = statusText; // pass the reference to VideoTexturePlayer for status updates
		
		return stage;
	}
}