using System.Collections.Generic;
using System.IO;
using KarmaLibrary.Core.Graphics.Shaders.Screen;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace KarmaLibrary.Core.Graphics.Shaders
{
    public class ShaderManager : ModSystem
    {
        private static Dictionary<string, ManagedShader> shaders;

        public static bool HasFinishedLoading
        {
            get;
            private set;
        }

        public override void OnModLoad()
        {
            // Don't attempt to load shaders on servers.
            if (Main.netMode == NetmodeID.Server)
                return;

            shaders = [];

            // This is kind of hideous but I'm not sure how to best handle these screen shaders. Perhaps some marker in the file name or a dedicated folder?
            Ref<Effect> s = new(Mod.Assets.Request<Effect>("Assets/AutoloadedEffects/ScreenDistortions/RadialScreenShoveShader", AssetRequestMode.ImmediateLoad).Value);
            Filters.Scene[RadialScreenShoveShaderData.ShaderKey] = new Filter(new RadialScreenShoveShaderData(s, ManagedShader.DefaultPassName), EffectPriority.VeryHigh);

            Ref<Effect> s2 = new(Mod.Assets.Request<Effect>("Assets/AutoloadedEffects/ScreenDistortions/LocalScreenDistortionShader", AssetRequestMode.ImmediateLoad).Value);
            Filters.Scene[ScreenDistortShaderData.ShaderKey] = new Filter(new ScreenDistortShaderData(s2, ManagedShader.DefaultPassName), EffectPriority.VeryHigh);

            HasFinishedLoading = true;
        }

        public override void PostSetupContent()
        {
            foreach (Mod mod in ModLoader.Mods)
            {
                List<string> fileNames = mod.GetFileNames();
                if (fileNames is null)
                    continue;

                foreach (var path in fileNames)
                {
                    // Ignore paths inside of the compiler directory.
                    if (path?.Contains("Compiler/") ?? true)
                        continue;
                    if (!path.Contains("AutoloadedEffects/"))
                        continue;

                    string shaderName = Path.GetFileNameWithoutExtension(path);
                    string clearedPath = Path.Combine(Path.GetDirectoryName(path), shaderName).Replace(@"\", @"/");
                    Ref<Effect> shader = new(mod.Assets.Request<Effect>(clearedPath, AssetRequestMode.ImmediateLoad).Value);
                    SetShader(shaderName, shader);
                }
            }
        }

        public static ManagedShader GetShader(string name) => shaders[name];

        public static void SetShader(string name, Ref<Effect> newShaderData) => shaders[name] = new(name, newShaderData);

        public override void PostUpdateEverything()
        {
            RadialScreenShoveShaderData.ToggleActivityIfNecessary();
            ScreenDistortShaderData.ToggleActivityIfNecessary();
        }
    }
}
