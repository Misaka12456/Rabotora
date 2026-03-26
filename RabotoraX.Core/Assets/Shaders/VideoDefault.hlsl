struct VSOut { float4 pos : SV_Position; float2 uv : TEXCOORD; };

Texture2D tex : register(t0);
SamplerState sam : register(s0);

float4 PSMain(VSOut i) : SV_Target
{
    return tex.Sample(sam, i.uv);
}