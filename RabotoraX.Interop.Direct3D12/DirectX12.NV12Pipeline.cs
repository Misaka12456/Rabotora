using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using RabotoraX.Core.Videos;
using RabotoraX.Interop.Direct3D11.Rendering;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using MapFlags = Vortice.Direct3D11.MapFlags;

namespace RabotoraX.Interop.Direct3D12;

public partial class DirectX12
{
    private ID3D11VertexShader? _nv12VertShader;
    private ID3D11PixelShader? _nv12FragShader;
    private ID3D11SamplerState? _nv12Sampler;
    private ID3D11RasterizerState? _nv12Rasterizer;
    private bool _isNv12Ready;

    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
    private void EnsureNV12Pipeline()
    {
        if (_isNv12Ready) return;

        var coreAsm = typeof(IVideoDecoder).Assembly;
        using var stream = coreAsm.GetManifestResourceStream("RabotoraX.Core.Assets.Shaders.VideoDefault.hlsl")!;
        string hlsl = new StreamReader(stream, new UTF8Encoding(false)).ReadToEnd();

        Compiler.Compile(hlsl, "VSMain", "VideoDefault.hlsl", "vs_5_0", out var vsBlob, out var errBlob).CheckError();
        _nv12VertShader = _d3d11Device!.CreateVertexShader(vsBlob.AsBytes());
        vsBlob.Dispose();
        errBlob?.Dispose();

        Compiler.Compile(hlsl, "PSMain", "VideoDefault.hlsl", "ps_5_0", out var psBlob, out errBlob).CheckError();
        _nv12FragShader = _d3d11Device.CreatePixelShader(psBlob.AsBytes());
        psBlob.Dispose();
        errBlob?.Dispose();

        _nv12Sampler = _d3d11Device.CreateSamplerState(new SamplerDescription()
        {
            Filter = Filter.MinMagMipLinear,
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            ComparisonFunc = ComparisonFunction.Never,
            MaxLOD = float.MaxValue
        });

        _nv12Rasterizer = _d3d11Device.CreateRasterizerState(new RasterizerDescription(CullMode.None, FillMode.Solid)
        {
            DepthClipEnable = false
        });

        _isNv12Ready = true;
    }

    internal DX11NV12VideoTexture CreateNV12VideoTexture(int width, int height)
    {
        EnsureNV12Pipeline();

        var inY = _d3d11Device!.CreateTexture2D(new Texture2DDescription()
        {
            Width = (uint)width, Height = (uint)height,
            MipLevels = 1, ArraySize = 1,
            Format = Format.R8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Dynamic,
            BindFlags = BindFlags.ShaderResource,
            CPUAccessFlags = CpuAccessFlags.Write
        });
        var inYSrv = _d3d11Device.CreateShaderResourceView(inY);

        var inUV = _d3d11Device.CreateTexture2D(new Texture2DDescription()
        {
            Width = (uint)(width / 2), Height = (uint)(height / 2),
            MipLevels = 1, ArraySize = 1,
            Format = Format.R8G8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Dynamic,
            BindFlags = BindFlags.ShaderResource,
            CPUAccessFlags = CpuAccessFlags.Write
        });
        var inUVSrv = _d3d11Device.CreateShaderResourceView(inUV);

        var outRgba = _d3d11Device.CreateTexture2D(new Texture2DDescription()
        {
            Width = (uint)width, Height = (uint)height,
            MipLevels = 1, ArraySize = 1,
            Format = Format.R8G8B8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
            CPUAccessFlags = CpuAccessFlags.None
        });
        var outRgbaRtv = _d3d11Device.CreateRenderTargetView(outRgba);

        return new DX11NV12VideoTexture(inY, inYSrv, inUV, inUVSrv, outRgba, outRgbaRtv, width, height);
    }

    internal unsafe void UpdateNV12Texture(DX11NV12VideoTexture nv12, ReadOnlySpan<byte> data, int stride)
    {
        int width = nv12.Width;
        int height = nv12.Height;
        int srcStride = stride > 0 ? stride : width;

        fixed (byte* pData = data)
        {
            var mapY = _d3d11Context!.Map(nv12.InputY, 0, MapMode.WriteDiscard);
            for (int row = 0; row < height; row++)
            {
                Unsafe.CopyBlockUnaligned((byte*) mapY.DataPointer + row * mapY.RowPitch, pData + (long) row * srcStride, (uint) width);
            }
            _d3d11Context.Unmap(nv12.InputY, 0);

            var mapUV = _d3d11Context.Map(nv12.InputUV, 0, MapMode.WriteDiscard);
            byte* uvSrc = pData + (long)srcStride * height;
            int uvRows = height / 2;
            for (int row = 0; row < uvRows; row++)
            {
                Unsafe.CopyBlockUnaligned((byte*)mapUV.DataPointer + row * mapUV.RowPitch, uvSrc + (long)row * srcStride, (uint) width);
            }
            _d3d11Context.Unmap(nv12.InputUV, 0);
        }

        _d3d11Context!.OMSetRenderTargets(nv12.OutputRgbaRtv);
        _d3d11Context.RSSetViewport(new Viewport(0, 0, width, height, 0, 1));
        _d3d11Context.RSSetState(_nv12Rasterizer);
        _d3d11Context.OMSetBlendState(null, null, 0xFFFFFFFF);
        _d3d11Context.IASetInputLayout(null);
        _d3d11Context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        _d3d11Context.VSSetShader(_nv12VertShader);
        _d3d11Context.PSSetShader(_nv12FragShader);
        _d3d11Context.PSSetShaderResources(0, [nv12.InputYSrv, nv12.InputUVSrv]);
        _d3d11Context.PSSetSampler(0, _nv12Sampler);
        _d3d11Context.Draw(3, 0);

        _d3d11Context.OMSetRenderTargets((ID3D11RenderTargetView)null!);
        _d3d11Context.PSSetShaderResources(0, [null!, null!]);

        _d3d11Context.Flush();
    }

    internal void DisposeNV12Pipeline()
    {
        _nv12VertShader?.Dispose();
        _nv12FragShader?.Dispose();
        _nv12Sampler?.Dispose();
        _nv12Rasterizer?.Dispose();
        _nv12VertShader = null;
        _nv12FragShader = null;
        _nv12Sampler    = null;
        _nv12Rasterizer = null;
        _isNv12Ready = false;
    }
}