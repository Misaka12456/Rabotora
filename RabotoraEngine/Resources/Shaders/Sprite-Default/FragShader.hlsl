Texture2D uTexture : register(t0);
SamplerState uSampler : register(s0);

struct PSInput
{
    float4 position : SV_POSITION;
    float2 texCoord : TEXCOORD0;
    float4 color : COLOR0;
};

float4 main(PSInput input) : SV_TARGET
{
    // Sample texture
    float4 tex = uTexture.Sample(uSampler, input.texCoord);

    // Multiply by vertex color
    float4 outColor = tex * input.color;

    // Premultiply alpha
    outColor.rgb *= outColor.a;

    return outColor;
}