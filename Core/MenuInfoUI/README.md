# Menu Info UI
Luminance provides an easy way to have custom icons appear on the player and world selection UI, providing information about them. To use, create a class inheriting from ``InfoUIManager`` and override ``GetPlayerInfoIcons()`` and/or ``GetWorldInfoIcons()``.
```csharp
public sealed class ExampleInfoUIManager : InfoUIManager
{
    public override IEnumerable<PlayerInfoIcon> GetPlayerInfoIcons()
    {
        yield return new PlayerInfoIcon("MyModName/Assets/ExtraTextures/CustomPlayerIcon1",
        "Mods.MyModName.InfoIcons.CustomPlayerIcon1",
        player =>
        {
            if (!player.TryGetModPlayer<MyModPlayer>(out var myModPlayer))
                return false;

            return myModPlayer.MyCustomStateBool;
        },
        100);
    }

    public override IEnumerable<WorldInfoIcon> GetWorldInfoIcons()
    {
        yield return new WorldInfoIcon("MyModName/Assets/ExtraTextures/CustomWorldIcon1",
        "Mods.MyModName.InfoIcons.CustomWorldIcon1",
        worldData =>
        {
            if (!worldData.TryGetHeaderData<MyModSystem>(out var tagData))
                return false;

            return tagData.ContainsKey("MyCustomState") && tag.GetBool("MyCustomState");
        },
        100);
    }
}
```
> A basic example showcasing how to add icons.

The priority of the icons (the last parameter in the constructor) is responsible for ordering (ascending order). If the order of your icon does not matter, place it around the 100 mark, else try different values until it works well with other mod's icons.
