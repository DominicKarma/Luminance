sampler baseTexture : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
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

// !!! Screen shader, do not delete the above parameters !!!

// The X/Y values corresponds to the direction of the line, the Z and W position correspond to the origin point of the line relative to the screen.
float lineWidths[10];
float4 lines[10];

float SignedDistanceToLine(float2 p, float2 lineOrigin, float2 lineDirection)
{
    return dot(lineDirection, p - lineOrigin);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float negativeInterpolant = 0;
    float2 coordsOffset = 0;
    for (int i = 0; i < 10; i++)
    {
        float4 l = lines[i];
        float2 lineDirection = l.xy;
        float2 lineOrigin = l.zw;
        float2 orthogonalLineDirection = float2(lineDirection.y, lineDirection.x);
        float distanceFromLine = SignedDistanceToLine(coords, lineOrigin, lineDirection);
        float lineWidth = lineWidths[i];
        
        coordsOffset -= lineDirection * sign(distanceFromLine) * lineWidth * 0.5;
        negativeInterpolant += smoothstep(1, 0.7, abs(distanceFromLine) / lineWidth);
    }
    
    float edgeInterpolant = smoothstep(0, 0.25, negativeInterpolant) * smoothstep(1, 0.9, negativeInterpolant);
    float4 negativeColor = float4(1 - tex2D(baseTexture, coords).rgb, 1);
    return lerp(tex2D(baseTexture, coords + coordsOffset), negativeColor, saturate(negativeInterpolant)) - edgeInterpolant * 0.9;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}