// Optional parameter - Used for storing arbitrary textures. Can be supplied via the SetTexture method on the C# end.
// For primitives these typically correspond to noise or specially prepared scrolling textures, but really anything can be used.
sampler overlayTexture : register(s1);

// Optional parameter - Used for making the results of this shader vary based on time. Could be used for something like a texture scroll along the primitives, for example.
float globalTime;

// Required parameter - This specifies the matrix used to transform input positions to screen UVs.
// If you import shaders from somewhere else, make sure that this parameter name matches EXACTLY, or it will inevitably fail.
matrix uWorldViewProjection;

// Define vertex data structures.
// If you import shaders from somewhere else, make sure that the TextureCoordinates varible is specified as float3, not float2.
// This constraint is necessary for an upcoming calculation in the vertex shader function.
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
    // Simply pass along data from the VertexShaderInput struct into its corresponding output.
    // The uWorldViewProjection matrix is applied during this step to ensure that positions are correctly mapped onto the screen.
    VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(input.Position, uWorldViewProjection);
    output.Position.z = 0;
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    
    // This is a necessary step for all primitive shaders that use Luminance's primitive renderer. Not using it will result in very strange visual bugs that look like an extreme zoom-in effect.
    // The exact mechanics of what this is for are a bit complex and have to do with taking advantage of the automatic
    // interpolation between vertex data in order to alleviate noticeable distortions on primitives that have sudden width function changes.
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
