using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Luminance.Core.ModCalls
{
    public class ModCallManager
    {
        public static object DefaultObject => new();

        public static readonly Dictionary<string, List<ModCall>> ModCallsByMods = new();

        /// <summary>
        /// Call this from YourMod.Call(params object[] args).
        /// </summary>
        public static object ProcessAllModCalls(Mod mod, params object[] args)
        {
            if (args is null || args.Length <= 0)
                return new ArgumentException("ERROR: No function name specified. First argument must be a function name.");
            if (args[0] is not string command)
                return new ArgumentException("ERROR: First argument must be a string function name.");

            var collection = GetCorrectModCalls(mod.Name);
            foreach (var modCall in collection)
            {
                if (modCall.CallCommands.Any(callCommand => callCommand.Equals(command, StringComparison.OrdinalIgnoreCase)))
                    return modCall.ProcessInternal(args.Skip(1).ToArray());
            }

            return DefaultObject;
        }

        private static List<ModCall> GetCorrectModCalls(string modName)
        {
            if (!ModCallsByMods.ContainsKey(modName))
                ModCallsByMods[modName] = new();
            return ModCallsByMods[modName];
        }

        internal static void RegisterModCall(Mod mod, ModCall call) => GetCorrectModCalls(mod.Name).Add(call);

        internal static void RemoveModCall(Mod mod, ModCall call) => GetCorrectModCalls(mod.Name).Remove(call);
    }
}
