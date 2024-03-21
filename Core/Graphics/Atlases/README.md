# Texture Atlases

These are a way to optimise texture rendering, by making the GPU swap the active texture less often. This is achieved by having all the textures on a single larger texture, and using it with frames to render the specific texture you want from it.
The process for using them is simple, but to make one yourself you will need to download and use some external software. They are primarily used in Luminance for its particle system, and it is encouraged to use them for any similar things you implement in your mod.

## Getting a texture from an atlas in code
This is very simple to do. Calling ``AtlasManager.GetTexture(string textureName)`` will return an ``AtlasTexture``, which can be passed into a ``SpriteBatch.Draw`` overload located in the ``Luminance.Common.Utilities`` namespace.

> [!NOTE]
> The name in your files does <b>not</b> need to be prefixed with the name of your mod, but accessing the atlas texture via code <b>must</b> prefix the name with ``YourModsName.``. This is to prevent conflicts between mods with otherwise identical texture names.
> For example, MyCoolMod would access the atlas texture "MyCoolTexture" by calling:<br/>
>  ``var texture = AtlasManager.GetTexture("MyCoolMod.MyCoolTexture");``

The atlas texture contains all the information about a single subtexture texture on the atlas. They are listed below:
- ``Name`` the name of the subtexture. This is unique, and trying to add any atlases with subtexture names that are already registered from your mod will throw.
- ``Atlas`` the atlas that this subtexture belongs to.
- ``Frame`` the frame rectangle of this subtexture on the main atlas.
  - ``Frame.X`` and ``Frame.Y`` are the coordinates of the top left of the subtexture.
  - ``Frame.Width`` and ``Frame.Height`` are the width and height of the subtexture.

## How to create an atlas:
Download [this program](https://free-tex-packer.com) and boot it up. You can add images, or folders from the buttons over here.

![image](https://github.com/toasty599/Luminance/assets/74939552/4bbfdb2d-85fc-4f2a-8444-62854289bc6c)

Here, I have added a folder, and it along with its contents are shown on the left now.

![image](https://github.com/toasty599/Luminance/assets/74939552/2ba19578-ec10-4b73-a9c2-462fa92e64f2)

### Configuring the atlas:
You will notice that they've also been placed into the canvas in the center. They will likely not be packed very well by default, and there are some settings you need to change.
- You must name it something unique, and relevant to its contents.
- You must remove the file extension.
- You shouldn't prepend the folder name in most cases, unless there are duplicate subtexture names which that solves.
- Rotation must not be allowed.
- Trimming with a mode of trim is fine and advisable.
- Detect identical should be enabled, it will remove duplicate textures and make the duplicate ones all point to the single one left, saving texture and file size.
- It can be packed however you like, but I recommend either Optimal Packer (works best in most cases) or Max Rects Packer with Smart method.

![image](https://github.com/toasty599/Luminance/assets/74939552/7bb61319-2a0e-4186-9c1e-8a9f9d3ec00a)

### Saving the atlas:
- Select the save path, and pick somewhere in ``YourModDirectory/Assets/Atlases/``.
- Then, click export. It won't give any confirmation, but checking the folder you saved too should show them there.
- It will generate the texture from the preview, and also a json file. Opening it up, you will see that it contains information about every subtexture on the atlas texture. This is read by Luminance when the atlas is loaded, and sets up all of its subtextures.
- Ensure that the subtexture name looks correct, but do not change any other data, as it will mess up reading the subtextures in Luminance.

![image](https://github.com/toasty599/Luminance/assets/74939552/bd5f23e6-a4c7-4f20-85a7-9124dabe580a)

- ``frames`` encapsulates each subtexture.
- ``BasicEnemy`` is the name of this subtexture, and encapsulates the information about it.
- ``frame`` is the position, width, and height of the subtexture on the atlas texture.

> [!NOTE]
> Frame is the only thing read for each subtexture entry. You should be able to remove the other ones without issue if file size is a concern to you, but it is not required.

The others aren't read or needed, but I will describe them here anyway:
- ``sourceSize`` is the dimensions of the texture, its width and height.
- ``rotated`` whether the sprite has been rotated. This should always be false. If it is not, you must remake the atlas texture with it disabled, simply changing it to false won't fix it.
- ``trimmed`` whether the subtexture has been trimmed. This is not relevant for these cases, as the texture should fill its space entirely.
- ``spriteSourceSize`` is sourceSize, but as a rectangle including its position on the texture. This is uneeded with frame and sourceSize existing.
- ``pivot`` is the pivot point of the texture. This is not relevant as that is determined automatically or manually when drawing.

Any atlases in the ``YourModDirectory/Assets/Atlases/`` directory will be automatically loaded and unloaded by Luminance when your mod is. If something doesn't seem to be working correctly, check the log file for any messages.
