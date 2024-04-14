# Primitives

Luminance provides a simple and centralized class for handling rendering primitives directly, for things such as trails etc. You do not need to worry about the behind the scenes goings on to use them.

## How to use
Drawing primitives using it is done via a single method call, ``PrimitiveRenderer.RenderTrail``.

```c#
private void DrawTrail()
{
    Vector2 positionToCenterOffset = NPC.Size * 0.5f;
    PrimitiveRenderer.RenderTrail(NPC.oldPos, new(_ => 20f, _ => Color.Aqua, _ => positionToCenterOffset), 20);
}
```
>  An example of using this to draw a very simple centered trail behind an NPC using its oldpos array.

## Pixelated Primitives
Luminance provides an interface to draw primitives in for NPCS/Projectiles with pixelation applied to them. To use, make your ModNPC/ModProjectile implement ``IPixelatedPrimitiveRenderer`` and use the same render trail call as above, setting ``Pixelate`` to true in the ``PrimitiveSettings`` constructor.

> [!Warning]
> It is essential to correctly pair the interface and the Pixelate setting exclusively together, it will throw if done incorrectly.
> <br/>

> [!Note]
> If you wish to render pixelated primitives on something that cannot implement the interface, you can use ``PrimitivePixelationSystem.RenderToPrimsNextFrame`` to queue an action to be performed at given layer. Be aware of where you call this, as it may not take effect until the next frame.
> Due to this, usage of this method is advised against.

## Creating a primitive shader
Primitives are constrained by a couple of differences compared to "traditional" shaders that are commonly applied to ordinary textures.
In the ``Assets/AutoloadedEffects/Shaders/Examples`` directory of this repository you will find an example for primitive shaders. This documentation will go through its requirements and purposes.

### Parameters
Contained within the example shader are the following three parameters:
```hlsl
sampler overlayTexture : register(s1);

float globalTime;
matrix uWorldViewProjection;
```

The first two, overlayTexture and globalTime, are unused, serving as examples for what a proper implementation could use. The ``overlayTexture`` will correspond to textures supplied via ``ManagedShader.SetTexture(texture, index)`` on the C# end of things.
``globalTime``, however, is automatically supplied. Any shader, including primitive shaders, will search for this parameter and supply it with ``Main.GlobalTimeWrappedHourly`` if it's found, allowing for dynamic time behaviors within the shader.
For primitives these could correspond with a simple texture scroll, making a chosen texture move forward or backward based on time.

The ``uWorldViewProjection`` parameter, on the other hand, is required for primitive shaders used by luminance to work, being responsible for vertex transformations. If it is misconfigured or given an incorrect name, the primitives will not render properly.

### Vertex data structures and the vertex shader
When working with primitives, you must manually ensure that your position coordinates are properly configured to be relative to screen UVs through linear transformations. Luckily, Luminance's internals automatically handle the math of this step when applying primitive shaders.
However, the shader must independently handle the processing of this matrix in the vertex shader. This is all done in the vertex shader.



One caveat to keep in mind is that Luminance uses a trick involving the Z position in the texture input coordinates to automatically address primitive distortion artifacts when the width function makes sharp changes. The ideas behind this are a bit complicated, but for the purposes of simply using this system all you have to remember is that you `` TextureCoordinates`` parameter should be of type ``float3`` for the input structure and ``float2`` for the output structure, and that you must include the ``output.TextureCoordinates.y = ...`` line in your vertex shaders. Misconfiguring this will result in rendering bugs.

> [!Warning]
> Be careful when moving shaders over from other mods to Luminance. You must ensure that the matrix parameter is named correctly, that your vertex data structures *exactly* match how they're set up in the example shader. Don't accidentally misread ``float2`` as ``float3``, and don't forget to account for the Z value in the input texture coordinates.
> <br/>
