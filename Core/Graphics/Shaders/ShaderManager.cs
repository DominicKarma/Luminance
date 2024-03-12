using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics.Shaders
{
    public class ShaderManager : ModSystem
    {
        /// <summary>
        ///     The set of all shaders handled by this manager class.
        /// </summary>
        private static Dictionary<string, ManagedShader> shaders;

        /// <summary>
        ///     Whether this manager class has finished loading all shaders yet or not.
        /// </summary>
        /// <remarks>
        ///     This primarily exists for cases where shaders may be used at mod loading times, such as on the game title screen.
        /// </remarks>
        public static bool HasFinishedLoading
        {
            get;
            private set;
        }

        /// <summary>
        ///     The folder directory in which shaders to autoload are searched for.
        /// </summary>
        public const string AutoloadDirectory = "AutoloadedEffects";

        public override void OnModLoad()
        {
            // Don't attempt to load shaders on servers.
            if (Main.netMode == NetmodeID.Server)
                return;

            shaders = [];
        }

        public override void PostSetupContent()
        {
            // Go through every mod and check for effects to autoload.
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
                    if (!path.Contains($"{AutoloadDirectory}/"))
                        continue;

                    string shaderName = Path.GetFileNameWithoutExtension(path);
                    string clearedPath = Path.Combine(Path.GetDirectoryName(path), shaderName).Replace(@"\", @"/");
                    Ref<Effect> shader = new(mod.Assets.Request<Effect>(clearedPath, AssetRequestMode.ImmediateLoad).Value);
                    SetShader(shaderName, shader);
                }
            }

            // Mark loading operations as finished.
            HasFinishedLoading = true;
        }

        /// <summary>
        ///     Retrieves a managed shader of a given name.
        /// </summary>
        /// <remarks>
        ///     In this context, the "name" must correspond with the file name of the shader, not including the path extension.
        /// </remarks>
        /// <param name="name">The name of the shader.</param>
        public static ManagedShader GetShader(string name) => shaders[name];

        /// <summary>
        ///     Sets a shader with a given name in the registry manually.
        /// </summary>
        /// <param name="name">The name of the shader.</param>
        /// <param name="newShaderData">The shader data reference to save.</param>
        public static void SetShader(string name, Ref<Effect> newShaderData) => shaders[name] = new(name, newShaderData);
    }
}
