Texture2D tex : register(t0);
SamplerState samp : register(s0);

struct VS_INPUT
{
    float2 pos : POSITION;
    float2 uv : TEXCOORD;
};

struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

PS_INPUT VSMain(VS_INPUT input)
{
    PS_INPUT output;
    output.pos = float4(input.pos, 0.0f, 1.0f);
    output.uv = input.uv;
    return output;
}

float4 PSMain(PS_INPUT input) : SV_Target
{
    float2 size;
    tex.GetDimensions(size.x, size.y);
    float2 pixel = 1.0 / size;
    
    float4 center = tex.Sample(samp, input.uv);
    
    float4 neighbor = tex.Sample(samp, input.uv + float2(0, pixel.y)) +
                      tex.Sample(samp, input.uv - float2(0, pixel.y)) +
                      tex.Sample(samp, input.uv + float2(pixel.x, 0)) +
                      tex.Sample(samp, input.uv - float2(pixel.x, 0));
    
    return center + (center - (neighbor * 0.25)) * 0.4;
}