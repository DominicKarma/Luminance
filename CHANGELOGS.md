# Luminance Changelogs
This file will contain each version of the mod published, along with all of the accompanying changes. Use this as reference for updating your mod if required (read the [API breakage policy](https://github.com/DominicKarma/Luminance/blob/main/README.md#api-breakage-policy)).

---

## V1.0.5
- Made the DrawInstances method for MetaballType instances virtual.
- Made the RenderQuad method use a dynamic clipping range for its underlying OrthographicOffCenter matrix, to prevent larger textures from being clipped incorrectly.
- Shader recompilations now display any detailed errors that occurred.

---

## V1.0.4
- Fixed problems regarding .fxc file loading.
- HookHelpers now internally uses RuntimeHelpers instead of FormatterServices.

---

## V1.0.3
- Added more shapes to the Primitive Renderer.
- Added a system for adding icons to the player and world selection UI. This also has an accompanying mod call.
- Added a system for Verlet Intergration.
- Added a new event to the state machine, ``OnStackEmpty``, which is fired upon the transition check encountering an empty stack. Use this to optionally refill the stack.
- Added a lot of missing documentation files.
- Changed the shader compiler to use FXC instead of EasyXNB.
- Changed the priority of the CutsceneLayer and FilterLayer from 125 and 255 to 100 and 200 respectively.

### Breaking Changes:
- Removed IDrawAdditive. This has skipped the obsolete stage due to its documentation advising against usage and warning about future removal, and sourcing issues.

### Upcoming Breaking Changes:
- ScreenShakeSystem's ``void SetUniversalRumble(float, float, Vector2?)`` has been marked as obsolete, you should swap all usages to ``ShakeInfo SetUniversalRumble(float, float, Vector2?, float)``.

---

## V1.0.2
- Improved the design of the mod's icon.
- Improved the Spanish mod localization.
- Shaders without a compiled xnb now get automatically compiled when entering a world.
- Fixed files being left behind in the shader compiler directory.
- Fixed a potential index error in the primitive renderer.

---

## V1.0.1
- Fixed a bug where atlases could include the .rawimg file extension in their registered texture paths.

---

## V1.0.0
Initial mod release! Find a list of the features in the base README.md and check out the (currently limited) documentation alongside the implementations of them.
