using System;
using System.Collections.Generic;
using Luminance.Core.MenuInfoUI;
using Terraria.IO;

namespace Luminance.Core.ModCalls.LuminanceCalls
{
    internal sealed class RegisterWorldInfoIconCall : ModCall
    {
        private sealed class ModCallInfoUIManager : InfoUIManager
        {
            private static readonly List<WorldInfoIcon> worldInfoIcons = [];

            public override IEnumerable<WorldInfoIcon> GetWorldInfoIcons() => worldInfoIcons;

            public static void AddWorldInfoIcon(WorldInfoIcon icon) => worldInfoIcons.Add(icon);
        }
        
        public override IEnumerable<string> GetCallCommands()
        {
            yield return "RegisterWorldInfoIcon";
        }

        public override IEnumerable<Type> GetInputTypes()
        {
            yield return typeof(string);
            yield return typeof(string);
            yield return typeof(Func<WorldFileData, bool>);
            yield return typeof(byte);
        }
        
        protected override object SafeProcess(params object[] argsWithoutCommand)
        {
            string texturePath = (string)argsWithoutCommand[0];
            string hoverTextKey = (string)argsWithoutCommand[1];
            Func<WorldFileData, bool> shouldAppear = (Func<WorldFileData, bool>)argsWithoutCommand[2];
            byte priority = (byte)argsWithoutCommand[3];
            
            ModCallInfoUIManager.AddWorldInfoIcon(new WorldInfoIcon(texturePath, hoverTextKey, shouldAppear, priority));
            return ModCallManager.DefaultObject;
        }
    }
}
