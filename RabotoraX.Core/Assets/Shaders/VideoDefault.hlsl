Texture2D<float> YPlane : register(t0);
Texture2D<float2> UVPlane : register(t1);
SamplerState Samp : register(s0);

struct VSOut
{
    float4 Pos : SV_POSITION;
    float2 UV : TEXCOORD0;
};

VSOut VSMain(uint id : SV_VertexID)
{
    VSOut o;
    o.UV = float2((id == 1) ? 2.0f : 0.0f, (id == 2) ? 2.0f : 0.0f);
    o.Pos = float4(o.UV.x * 2.0f - 1.0f, 1.0f - o.UV.y * 2.0f, 0.0f, 1.0f);
    return o;
}

float4 PSMain(VSOut i) : SV_Target
{
    float y = YPlane.Sample(Samp, i.UV).r; // Y component
    float2 cbcr = UVPlane.Sample(Samp, i.UV).rg; // Cb and Cr components (U and V respectively)
    
    y = (y - 16.0f / 255.0f) * (255.0f / 219.0f); // Normalize Y to [0, 1]
    float cb = cbcr.r - 128.0f / 255.0f; // Normalize Cb to [-0.5, 0.5]
    float cr = cbcr.g - 128.0f / 255.0f; // Normalize Cr to [-0.5, 0.5]
    
    float r = saturate(y + 1.574800f * cr); // R = Y + 1.5748 * Cr
    float g = saturate(y - 0.187324f * cb - 0.468124f * cr); // G = Y - 0.187324 * Cb - 0.468124 * Cr
    float b = saturate(y + 1.855600f * cb); // B = Y + 1.8556 * Cb
    return float4(r, g, b, 1.0f);
}