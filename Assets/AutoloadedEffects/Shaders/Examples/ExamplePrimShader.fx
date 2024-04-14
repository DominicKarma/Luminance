// Documentation for this file exists as the following location: https://github.com/DominicKarma/Luminance/tree/main/Core/Graphics/Primitives
// If you copypaste this .fx file for use as a base for a custom shader, don't forget to delete the residual comments in here.
sampler overlayTexture : register(s1);
float globalTime;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    output.Position = mul(input.Position, uWorldViewProjection);
    output.Position.z = 0;
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    
    output.TextureCoordinates.y = (output.TextureCoordinates.y - 0.5) / input.TextureCoordinates.z + 0.5;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // For most purposes, these are the two primitive data constructs you'll want to work with.
    // In this context, The X component of the UV vector corresponds to the length along the primitive (The start = 0, the end = 1, and everything in between), and the
    // Y component corresponds to the horizontal position on the primitives.
    float4 color = input.Color;
    float2 uv = input.TextureCoordinates;
    
    // As a simple example, interpolate from red to blue the futher along the primitives this fragment is.
    // At the start it will return red, at the end it will return blue.
    return lerp(float4(1, 0, 0, 1), float4(0, 0, 1, 1), uv.x);
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
