sampler baseTexture : register(s0);
sampler distortionMap : register(s1);
sampler distortionExclusionMap : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;
float4 uShaderSpecificData;

float2 GetDistortionDirection(float2 coords)
{
    float3 distortionMapValue = tex2D(distortionMap, coords).xyz;
    
    float x = (distortionMapValue.x - 0.5) * 2;
    float y = (distortionMapValue.y - 0.5) * 2;
    
    return float2(x, y) * distortionMapValue.z;
}

float2 GetDistortionDirectionBlurred(float2 coords)
{
    float2 distortion = GetDistortionDirection(coords);
    for (int i = -3; i <= 3; i++)
        distortion += GetDistortionDirection(coords + float2(i * 0.003, 0)) / (abs(i) * 0.7 + 1);
    for (i = -3; i <= 3; i++)
        distortion += GetDistortionDirection(coords + float2(0, i * 0.003)) / (abs(i) * 0.7 + 1);
    
    return distortion;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 distortionOffset = GetDistortionDirectionBlurred(coords) * (1.025 - pow(tex2D(distortionExclusionMap, coords).a, 0.17));
    return tex2D(baseTexture, coords + distortionOffset * 0.05);
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}