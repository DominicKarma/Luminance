# Specific Effect Managers
This namespace contains several useful effects that you can use, such as camera control or screenshake. Each feature is documented below.
> [!Note]
> You should avoid using these systems if the game is running as a server as they will have no effect.

---
## Blocker System
This system allows you to block user inputs and/or UI rendering while a condition is met. To use, call either ``BlockerSystem.Start`` override and pass in whatever is appropriate for your use case.

```c#
public ref float Timer => ref NPC.ai[1];

public const int SpawnAnimationLength = 180;

public override void AI()
{
    if (!Main.dedServ && Timer == 0)
        BlockerSystem.Start(false, true, () => Timer <= SpawnAnimationLength);
    ...
}
```
> An example of a boss blocking UI for the duration of its custom spawn animation.

## Camera Pan System
This system allows you to easily modify the camera with panning to a specific location and zooming. To pan, call ``CameraPanSystem.PanTowards``, to zoom in, call ``CameraPanSystem.ZoomIn`` and to zoom out, call ``CameraPanSystem.ZoomOut``.

## Screen Modifier Manager
This system allows you to do things with the screen target from a ``FilterManager.EndCapture`` detour, while following a priority order from other mods using this system for ordering.<br/>
To use, call ``ScreenModifierManager.RegisterScreenModifier`` in any ``PostSetupContent`` (or anywhere after Luminance has ran its loading). All registered modifiers are ran from lowest priority to highest, and you can check Luminance's modifier's priorities inside ``ScreenModifierManager``.

```c#
private void PerformScreenModifications(RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
{
    // Perform any modifications to the screen target here.
    ...
    // "screenTarget1" should be the active target when leaving this method.
}

public override PostSetupContent() => ScreenModifierManager.RegisterScreenModifier(PerformScreenModifications, 150);
```
> A basic example of registering a screen modifier at a priority above cutscenes, but below screen filters.

## Screen Shake System
This system allows you to create customizable screen shake effects. An example using is below, but for a more in-depth look at each available effect, read the documentation on each exposed method in ``ScreenShakeSystem.cs``
```c#
// Creating a shake at a boss' center in its AI method.
if (!Main.dedServer && Timer == ShakeStartTime)
    ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 4f);
```
> A basic example of creating a screenshake effect.
