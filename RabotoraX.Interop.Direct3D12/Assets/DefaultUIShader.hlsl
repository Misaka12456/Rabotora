struct VSOut { float4 pos : SV_Position; float2 uv : TEXCOORD; };
VSOut VSMain(uint id : SV_VertexID)
{
    VSOut o;
    o.uv = float2((id << 1) & 2, id & 2);
    o.pos = float4(o.uv * float2(2, -2) + float2(-1, 1), 0, 1);
    return o;
}
Texture2D tex : register(t0);
SamplerState sam : register(s0);
float4 PSMain(VSOut i) : SV_Target
{
    float4 col = tex.Sample(sam, i.uv);
    return col;
}