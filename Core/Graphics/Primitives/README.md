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
