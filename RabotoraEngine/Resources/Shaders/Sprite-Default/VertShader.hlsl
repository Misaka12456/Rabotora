cbuffer TransformCB : register(b0)
{
    float4x4 uTransform; // model-view-proj or simple screen transform. If unused, set to identity.
}

struct VSInput
{
    float3 POSITION : POSITION;
    float2 TEXCOORD : TEXCOORD;
    float4 COLOR : COLOR;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD;
    float4 Color : COLOR;
};

VSOutput main(VSInput input)
{
    VSOutput o;

    // Transform position by uTransform (expecting position.w = 1)
    float4 pos = float4(input.POSITION, 1.0f);
    o.Position = mul(pos, uTransform); // note: HLSL mul(row, column) depends on matrix layout; this assumes row-major or adjusts on CPU accordingly.

    // Prefer no transform: ensure uTransform -- identity on CPU.
    o.TexCoord = input.TEXCOORD;
    o.Color = input.COLOR;

    return o;
}