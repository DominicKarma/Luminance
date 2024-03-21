# Automators
This namespace contains several simple but useful things for programming with graphics/VFX.

## IDrawAdditive
This is an interface designed for use by projectiles for drawing additively without restarting the spritebatch. To use, make your ModProjectile implement the interface and use the drawing method as you would predraw.

> [!Note]
> This feature is likely to be removed/reworked in the future due to only working with projectiles, and the performance benefits not really being relevant. Therefore, use of this is not recommended.

## ManagedRenderTarget
This is a RenderTarget wrapper that automatically handles resizes, unloads and disposes of if not in use. To use it is simple, assign to an instance of it in a loading method using its constructor, and then use it for drawing like you would a normal render target.
```c#
public ManagedRenderTarget MyTarget;

public override void Load()
{
    Main.QueueMainThreadAction(() =>
    {
        MyTarget = new(true, ManagedRenderTarget.CreateScreenSizedTarget);
    });
}
```
> Snippet showing an example of preparing a ManagedRenderTarget in a ModSystem.
