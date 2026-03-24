struct VSOut { float4 pos : SV_Position; float2 uv : TEXCOORD; };

Texture2D tex : register(t0);
SamplerState sam : register(s0);

float4 PSMain(VSOut i) : SV_Target
{
    float4 col = tex.Sample(sam, i.uv);
    
    float3 corrected = saturate(col.rgb - 0.062745f) * 1.16438f;
    
    return float4(corrected, col.a);
}