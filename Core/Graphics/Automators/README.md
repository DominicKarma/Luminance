# Automators
This namespace contains several simple but useful things for programming with graphics/VFX.

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
