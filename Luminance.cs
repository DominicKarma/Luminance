global using static System.MathF;
global using static Luminance.Assets.MiscTexturesRegistry;
global using static Luminance.Common.Utilities.Utilities;
global using static Microsoft.Xna.Framework.MathHelper;
using Luminance.Core.Graphics;
using Luminance.Core.Hooking;
using Terraria.ModLoader;

namespace Luminance
{
    public class Luminance : Mod
    {
        public override void Unload() => ManagedILEdit.UnloadEdits();

        public override void PostSetupContent()
        {
            // Go through every mod and check for effects to autoload.
            foreach (Mod mod in ModLoader.Mods)
            {
                ShaderManager.LoadShaders(mod);
                AtlasManager.InitializeModAtlases(mod);
                ParticleManager.InitializeManualRenderers(mod);
            }

            // Mark loading operations as finished.
            ShaderManager.HasFinishedLoading = true;
        }
    }
}
