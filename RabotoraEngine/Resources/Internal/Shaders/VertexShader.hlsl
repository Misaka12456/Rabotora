/**
 *  Rabotora Engine - Vertex Shader (HLSL version, for DirectX 11+)
 *  (C)Copyright 2018-2025 Misaka Castle Group, Rabotora Tower Project and all Rabotora contributors
 *  Licensed under 123 Open-Source Organization MIT Public License 2.0
 */

struct VS_INPUT {
    float3 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR;
};

struct VS_OUTPUT {
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR;
};

VS_OUTPUT main(VS_INPUT input) {
    VS_OUTPUT output;
    output.Position = float4(input.Position, 1.0f);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    return output;
}