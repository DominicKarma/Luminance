global using static System.MathF;
global using static Luminance.Assets.MiscTexturesRegistry;
global using static Luminance.Common.Utilities.Utilities;
global using static Microsoft.Xna.Framework.MathHelper;
using Luminance.Core.Graphics;
using Luminance.Core.Hooking;
using Luminance.Core.ModCalls;
using Terraria.ModLoader;

namespace Luminance
{
    /// <summary>
    /// The central mod type for the Luminance library.
    /// </summary>
    public sealed class Luminance : Mod
    {
        /// <summary>
        /// Handles all necessary manual unloading effects for the library.
        /// </summary>
        public override void Unload() => ManagedILEdit.UnloadEdits();

        /// <summary>
        /// Handles all necessary loading effects for the library, after all mods have loaded and all dependencies have been established.
        /// </summary>
        public override void PostSetupContent()
        {
            ShaderManager.HasFinishedLoading = false;

            foreach (Mod mod in ModLoader.Mods)
            {
                HookHelper.LoadHookInterfaces(mod);
                ShaderRecompilationMonitor.LoadForMod(mod);
                ShaderManager.LoadShaders(mod);
                AtlasManager.InitializeModAtlases(mod);
                ParticleManager.InitializeManualRenderers(mod);
            }

            ShaderManager.HasFinishedLoading = true;
        }

        public override object Call(params object[] args) => ModCallManager.ProcessAllModCalls(this, args);
    }
}
