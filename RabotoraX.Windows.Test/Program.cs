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
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace RabotoraX.Windows.Test;

[SupportedOSPlatform("windows")]
public static class Program
{
	[STAThread]
	[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
	public static int Main(string[] args)
	{
		using var app = new Rabotora("Example Presentation", 1280, 720, new Fractional(16, 9));

		return app.Run(Example2DGoLiveStage2());
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
		text.Color = Vector4.One; // White text
		text.Layout.SetParent(canvasObj.Layout);

		return stage;
	}

	private static RStage Example2DStage()
	{
		var stage = new RStage("Act 1: Hello RabotoraX") {Type = StageType.Render2D, ClearColor = new Vector4(0, 0, 0, 1)};

		var canvasObj = stage.CreateObject("Canvas");
		var canvas = canvasObj.AddComponent<RCanvas>();
		canvas.ReferenceResolution = new Vector2(1280, 720);
		canvas.ScaleMode = CanvasScaleMode.ScaleWithScreenSize;
		canvas.MatchMode = ScreenMatchMode.MatchWidthOrHeight;
		canvas.MatchWidthOrHeight = 0.5f;
		var ui = canvas.GetComponent<RUILayout>()!;
		ui.AnchorMin = Vector2.Zero;
		ui.AnchorMax = Vector2.One;
		ui.OffsetMin = ui.OffsetMax = Vector2.Zero;
		ui.Pivot = new Vector2(0f, 0f);

		var text = stage.CreateObject("HelloText").AddComponent<Text>();
		text.Content = "RabotoraX 2D Stage 示例\nRabotoraX 2D Stage Example";
		text.Color = new Vector4(0, 1, 0, 1);
		SetupScreenLayout(text.RObject, Vector2.Zero);

		var t1 = stage.CreateObject("Text1").AddComponent<Text>();
		t1.Content = "扫码缴费";
		t1.Color = Vector4.One;
		SetupScreenLayout(t1.RObject, new Vector2(150, 300));

		var t2 = stage.CreateObject("Text2").AddComponent<Text>();
		t2.Content = "快速离场";
		t2.Color = Vector4.One;
		SetupScreenLayout(t2.RObject, new Vector2(150, 325));

		var t3 = stage.CreateObject("Text3").AddComponent<Text>();
		t3.Content = "快";
		t3.Color = Vector4.One;
		SetupScreenLayout(t3.RObject, new Vector2(150, 350));

		var t4 = stage.CreateObject("Text4").AddComponent<Text>();
		t4.Content = "快";
		t4.Color = new Vector4(1, 0, 0, 1);
		SetupScreenLayout(t4.RObject, new Vector2(150, 375));

		var t5 = stage.CreateObject("InputTest").AddComponent<Text>();
		t5.Content = "只有按住屏幕才能看到我哦\nHold the screen to see me";
		t5.Color = new Vector4(111 / 255f, 194 / 255f, 118 / 255f, 1);
		SetupScreenLayout(t5.RObject, new Vector2(150, 450));
		t5.RObject.AddComponent<InputTest>();
		t5.IsEnabled = false;

		return stage;

		void SetupScreenLayout(RObject obj, Vector2 screenPos)
		{
			var layout = obj.GetComponent<RUILayout>()!;
			layout.SetParent(canvasObj.Layout);

			layout.AnchorMin = Vector2.Zero;
			layout.AnchorMax = Vector2.Zero;

			layout.Pivot = Vector2.Zero;
			layout.AnchoredPosition = screenPos;
		}
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
		image.Opacity = 0;
		using var pkg = RDataPackage.Open("Resources/Demo2D_Data.pkg");
		using var stream = pkg.OpenEntry("Assets/Sprites/Splash.png") ?? throw new InvalidOperationException("Failed to load image from package.");
		var tex = new Texture2D();
		tex.Load2DImage(stream);
		image.Sprite = Sprite.Create(tex);
		image.IsEnabled = false;
		imageObj.AddComponent<ImageTilt>();

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
		var clip = new VideoClip(new FileStream(@"<YOUR_VIDEO_PATH_HERE>", FileMode.Open, FileAccess.Read)); // no need to set VideoFormatType bec. underlying decoder (e.g. Media Foundation) will auto-detect it
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
		statusText.Color = new Vector4(0, 0, 0, 1); // Black text
		statusText.FontName = "Microsoft YaHei UI";
		statusText.FontSize = 18;
		statusText.Content = "Idle";
		vtp._statusText = statusText; // pass the reference to VideoTexturePlayer for status updates
		
		return stage;
	}
}