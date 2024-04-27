# Mod Calls
Luminance provides a system for easily handling mod calls, and allowing for them to be more organised and safe. To create a call, override ``ModCall`` and follow the below example to set it up properly.

```c#
public sealed class SetExampleBossDownedCall : ModCall
{
    // These are all of the commands (first argument) that mods can use to select this mod call.
    public override IEnumerable<string> GetCallCommands()
    {
        yield return "ExampleBoss";
        yield return "ExampleBossCoolNickname";
    }

    // These are all of the types that the input arguments are required to be.
    // This does not include the first argument (the command name).
    public override IEnumerable<Type> GetInputTypes()
    {
        yield return typeof(bool);
    }

    // Process the arguments and perform the mod call's action here.
    protected override object SafeProcess(params object[] argsWithoutCommand)
    {
        // For this to run, the submitted arguments must successfully match your input types,
        // so it is safe to directly cast the objects.
        WorldSaveSystem.ExampleBossDowned = (bool)argsWithoutCommand[0];
        // If the mod call doesn't need to return anything, avoid returning null for good practice.
        return ModCallManager.DefaultObject;
    }
}
```

> [!Warning]
> In order for your mod calls to be processed, you must call ``Luminance.Core.ModCalls.ModCallManager.ProcessAllModCalls(Mod mod, params object[] args)`` from your ``Mod.Call(params object[] args)`` override.
