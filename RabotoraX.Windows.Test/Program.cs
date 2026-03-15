using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using RabotoraX.Core;
using RabotoraX.Core.Audios;
using RabotoraX.Core.Cinematics;
using RabotoraX.Core.Inputs;
using RabotoraX.Core.Scripting;
using RabotoraX.Core.Test;
using RabotoraX.Core.UI;
using RabotoraX.Interop.Direct3D11;
using RabotoraX.Interop.Win32.RenderImpl;

namespace RabotoraX.Windows.Test;

public sealed class ColorTilt : RManagedScript
{
	private QuadRenderer _quadRenderer = null!;
	private Stopwatch _timer = null!;
	
	public override void OnStart()
	{
		_quadRenderer = GetComponent<QuadRenderer>()!;
		_timer = Stopwatch.StartNew();
	}

	public override void OnUpdate(float deltaTime)
	{
		var t = (float)_timer.Elapsed.TotalSeconds;
		var r = (float)(Math.Sin(t) * 0.5 + 0.5);
		var g = (float)(Math.Sin(t + Math.PI * 2 / 3) * 0.5 + 0.5);
		var b = (float)(Math.Sin(t + Math.PI * 4 / 3) * 0.5 + 0.5);
		_quadRenderer.Color = new Vector4(r, g, b, 1);
	}
}

public sealed class RotatingCube : RManagedScript
{
	private Stopwatch _timer = null!;
	
	public override void OnStart()
	{
		_timer = Stopwatch.StartNew();
	}

	public override void OnUpdate(float deltaTime)
	{
		var t = (float)_timer.Elapsed.TotalSeconds;
		var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, t * 0.5f);
		Layout.Rotation = rot;
	}
}

public sealed class InputTest : RManagedScript
{
	private Text _text = null!;

	public override void OnAwake()
	{
		_text = GetComponent<Text>()!;
	}

	public override void OnUpdate(float deltaTime)
	{
		_text.IsEnabled = Input.GetMouseButton(0);
	}
}

public static class Program
{
	[STAThread]
	[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
	public static int Main(string[] args)
	{
		// Assembly.Load("RabotoraX.Interop.Direct3D11").GetTypes();
		// Assembly.Load("RabotoraX.Interop.Win32").GetTypes();
		_ = typeof(DirectX11);
		_ = typeof(Win32NativeWindow);
		using var app = new Rabotora("Example Presentation", 1280, 720);
		
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
	
	private static RStage Example2DStage()
	{
		var stage = new RStage("Act 1: Hello RabotoraX") {Type = StageType.Render2D, ClearColor = new(0,0,0,1)};
		// A 2D stage doesn't need a camera (an audience), so we can directly create objects and render them.
		
		var canvasObj = stage.CreateObject("Canvas");
		var canvas = canvasObj.AddComponent<RCanvas>();
		canvas.ReferenceResolution = new Vector2(1280, 720);
		canvas.ScaleMode = CanvasScaleMode.ScaleWithScreenSize;
		canvas.MatchMode = ScreenMatchMode.MatchWidthOrHeight;
		canvas.MatchWidthOrHeight = 0.5f;
		
		var text = stage.CreateObject("HelloText").AddComponent<Text>();
		text.Content = "RabotoraX 2D Stage 示例\nRabotoraX 2D Stage Example";
		text.Color = new Vector4(0, 1, 0, 1); // Green text
		text.Layout.SetParent(canvasObj.Layout);

		var t1 = stage.CreateObject("Text1").AddComponent<Text>();
		t1.Content = "扫码缴费";
		t1.Color = Vector4.One;
		t1.Layout.SetParent(canvasObj.Layout);
		t1.GetComponent<RUILayout>()!.AnchoredPosition = new Vector2(150, 300);
		var t2 = stage.CreateObject("Text2").AddComponent<Text>();
		t2.Content = "快速离场";
		t2.Color = Vector4.One;
		t2.Layout.SetParent(canvasObj.Layout);
		t2.GetComponent<RUILayout>()!.AnchoredPosition = new Vector2(150, 325);
		var t3 = stage.CreateObject("Text3").AddComponent<Text>();
		t3.Content = "快";
		t3.Color = Vector4.One;
		t3.Layout.SetParent(canvasObj.Layout);
		t3.GetComponent<RUILayout>()!.AnchoredPosition = new Vector2(150, 350);
		var t4 = stage.CreateObject("Text4").AddComponent<Text>();
		t4.Content = "快";
		t4.Color = new Vector4(1, 0, 0, 1); // Red text
		t4.Layout.SetParent(canvasObj.Layout);
		t4.GetComponent<RUILayout>()!.AnchoredPosition = new Vector2(150, 375);
		
		var t5 = stage.CreateObject("InputTest").AddComponent<Text>();
		t5.Content = "只有按住屏幕才能看到我哦\nHold the screen to see me";
		t5.Color = new Vector4(111 / 255f, 194 / 255f, 118 / 255f, 1); // Soft green text
		t5.Layout.SetParent(canvasObj.Layout);
		t5.GetComponent<RUILayout>()!.AnchoredPosition = new Vector2(450, 300);
		t5.RObject.AddComponent<InputTest>();
		t5.IsEnabled = false;
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
		using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RabotoraX.Windows.Test.Resources.SneakySnitch.ogg") // from Kevin MacLeod (see LICENSE-Appendix.txt for details)
		       ?? throw new InvalidOperationException("Failed to load embedded audio resource."))
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
}