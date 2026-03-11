using System.Numerics;
using System.Runtime.InteropServices;
using RabotoraX.Core.Cinematics;
using RabotoraX.Core.Graphics;

namespace RabotoraX.Core.Test;

[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionColor
{
    public Vector3 Position;
    public Vector4 Color;
}

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct QuadParams
{
    public Matrix4x4 WorldViewProjection;
    public Vector4 MeshColor;
}

public class QuadRenderer : Component
{
    public Vector4 Color { get; set; } = new Vector4(1, 0, 0, 1); // 默认红色

    private IGpuBuffer? _vertexBuffer;
    private IGpuBuffer? _indexBuffer;
    private IGpuBuffer? _constantBuffer;
    
    // 修复：必须分开保存 VS 和 PS
    private IShader? _vertexShader;
    private IShader? _pixelShader;

    private const string ShaderCode = @"
        cbuffer MVPBuffer : register(b0) {
           matrix WorldViewProj;
           float4 MeshColor;
        };

        struct VS_INPUT {
            float3 Pos : POSITION;
            float4 Color : COLOR;
        };

        struct PS_INPUT {
            float4 Pos : SV_POSITION;
            float4 Color : COLOR;
        };

        PS_INPUT VSMain(VS_INPUT input) {
            PS_INPUT output;
            output.Pos = mul(float4(input.Pos, 1.0f), WorldViewProj);
            output.Color = input.Color;
            return output;
        }

        float4 PSMain(PS_INPUT input, bool isFrontFace : SV_IsFrontFace) : SV_Target {
           float4 green = float4(0.0, 1.0, 0.0, 1.0);

           float4 frontColor = input.Color * MeshColor;
           
           return isFrontFace ? frontColor : green; // front: input (red), back: green
        }
    ";

    public override void OnStart()
    {
        var api = GraphicsService.API;

        var vertices = new VertexPositionColor[]
        {
            new() { Position = new Vector3(-0.5f, -0.5f, 0), Color = Vector4.One },
            new() { Position = new Vector3(-0.5f,  0.5f, 0), Color = Vector4.One },
            new() { Position = new Vector3( 0.5f,  0.5f, 0), Color = Vector4.One },
            new() { Position = new Vector3( 0.5f, -0.5f, 0), Color = Vector4.One }
        };
        _vertexBuffer = api.CreateBuffer(BufferType.VertexBuffer, vertices);

        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };
        // 修复：赋值给正确的变量 _indexBuffer
        _indexBuffer = api.CreateBuffer(BufferType.IndexBuffer, indices);

        _constantBuffer = api.CreateBuffer(BufferType.ConstantBuffer, Marshal.SizeOf<QuadParams>());

        var layout = new InputElementDescription[]
        {
            new("POSITION", 0, 3, 0),
            new("COLOR", 0, 4, 12)
        };
        
        // 修复：同时编译并创建 Vertex Shader 和 Pixel Shader
        _vertexShader = api.CreateShader(ShaderType.VertexShader, ShaderCode, "VSMain", layout);
        _pixelShader = api.CreateShader(ShaderType.FragmentShader, ShaderCode, "PSMain");
    }

    public override void OnRender()
    {
        var api = GraphicsService.API;
        if (Audience.Main == null || _vertexShader == null || _pixelShader == null) return;

        // 1. 计算 MVP 矩阵
        float aspect = (float)GraphicsService.LatestWindowState.Width / GraphicsService.LatestWindowState.Height;
        Matrix4x4 view = Audience.Main.GetViewMatrix();
        Matrix4x4 proj = Audience.Main.GetProjectionMatrix(aspect);
        
        Matrix4x4 world = Matrix4x4.CreateScale(Layout.Scale) * 
                          Matrix4x4.CreateFromQuaternion(Layout.Rotation) * 
                          Matrix4x4.CreateTranslation(Layout.Position);

        Matrix4x4 mvp = world * view * proj;
        var mvpData = new QuadParams { WorldViewProjection = Matrix4x4.Transpose(mvp), MeshColor = Color };

        // 2. 更新常量缓冲
        api.UpdateBuffer(_constantBuffer!, ref mvpData);

        // 3. 绑定状态
        // 修复：必须同时绑定 VS 和 PS
        api.SetShader(_vertexShader);
        api.SetShader(_pixelShader);
        
        api.SetVertexBuffer(_vertexBuffer!, Marshal.SizeOf<VertexPositionColor>());
        api.SetIndexBuffer(_indexBuffer!);
        
        // 绑定常量缓冲到 VS (因为我们在 HLSL 里是在 VS_INPUT 阶段乘的 MVP)
        api.SetConstantBuffer(0, _constantBuffer!, ShaderType.VertexShader);
        api.SetConstantBuffer(0, _constantBuffer!, ShaderType.FragmentShader); 

        // 4. Draw Call
        // ReSharper disable once NotDisposedResource
        api.SetCullMode(CullMode.None);
        api.DrawIndexed(6, 0, 0);
    }

    protected override void Dispose(bool disposing)
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _constantBuffer?.Dispose();
        _vertexShader?.Dispose();
        _pixelShader?.Dispose();
        base.Dispose(disposing);
    }
}