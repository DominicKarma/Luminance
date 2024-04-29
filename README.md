# Luminance
Luminance is a library mod that provides a range of useful features to mods.

---
## How to navigate the repository
Each folder contains (or will contain) a txt file providing documentation for each feature contained in the folder. If something is unclear, or could be worded better, feel free to create an issue or PR that improves it (only do this for documentation NOT in TODO.md please). A full list of features can be found at the bottom of this file.

## How to use Luminance
To make your mod use Luminance, add ``modReferences = Luminance`` in your mod's ``build.txt``. In order to be able to reference the mod when programming, download ``Luminance.dll``, ``Luminance.pdb`` and ``Luminance.xml`` from the latest github release, or extract the mod in-game and take the files from there. Add the dll as a project reference, or directly add it into your mod's .csproj. Ensure you reference the latest mod version to avoid things breaking. The .pdb and .xml files ensure you can view the documentation from your mod directly. It's recommended to add these 3 files to the ``buildIgnore`` in ``build.txt`` to avoid unnecessarily packaging them with your mod.

## API breakage policy
To prevent mod breakage from updates to Luminance as much as possible, changes to exsting methods etc will attempt to obsolete the old versions while also adding the new versions for a wait period of 2 weeks, to allow mods to update safely to the newer version without suddenly breaking. After the wait time has passed, the old versions will be removed.
You can keep track of any current or upcoming breakage by checking [here](https://github.com/DominicKarma/Luminance/blob/main/CHANGELOGS.md) each time Luminance updates.

## Feature list
Navigate to the folder to find out more about each feature listed.<br/>

### Common:
- Data Structures: Useful generic interfaces or structs with varying use cases.
- Easings: Features for creating easing animations, or simply smoothening a value based on a range of easing curves.
- State Machines: A system for handling complex behavior(AI) states for NPCs, Projectiles or anything really.
- Utilities: Contains a wide range of utilities, ranging from mathematics to drawcode etc.
- Verlet Intergration: Contains verlet simulations for creating realistic ropes etc.

### Core:
- Balancing: Contains a centeralized balancing system that supports NPC HP, NPC projectile resistances, Item tweaking with conflict handling between different mods.
- Cutscenes: Contains a cutscene system that allows for mods to play out cutscene events with hooks for common aspects, but giving freedom in what happens during it.
- Graphics: Provides a wide range of graphics features, such as particles, primitives, shaders, screenshake etc.
- Hooking: Provides several interfaces for using detours, a wrapper class for managed IL edits and a helper class.
- Menu Info UI: Contains a system that allows mods to add small icons to the player and world UI that show specific information based on the state of said player or world.
- Mod Calls: Provides a system to split up and organise your mod calls while having automatic type safety and argument checks.
- Sounds: Provides a system to properly loops sound instances.

## Contributors
Thanks to anyone and everyone who has contributed to Luminance in any form!
- <b>Dominic Karma:</b> Initial mod features, feature contributor.
- <b>Toasty:</b> Feature contributor.
- <b>moonburn:</b> Proper mod icon.
- <b>JavyzTaken:</b> Various optimizations and minor additions.
- <b>madamamada, riz30n014, definatly_a_human, ritsukicat, myawk, lgl_fish:</b> Translations.
- <b>ScalarVector:</b> Independent bases for a handful of features within the library.
