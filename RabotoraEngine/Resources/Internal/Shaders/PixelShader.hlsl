/**
 *  Rabotora Engine - Pixel Shader (Fragment Shader, HLSL version, for DirectX 11+)
 *  (C)Copyright 2018-2025 Misaka Castle Group, Rabotora Tower Project and all Rabotora contributors
 *  Licensed under 123 Open-Source Organization MIT Public License 2.0
 */

Texture2D shaderTexture : register(t0);
SamplerState textureSampler : register(s0);

struct PS_INPUT {
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR;
};

float4 main(PS_INPUT input) : SV_TARGET {
    float4 texColor = shaderTexture.Sample(textureSampler, input.TexCoord);
    
    float4 finalColor = texColor * input.Color;
    
    return finalColor;
}