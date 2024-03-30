using Luminance.Core.Hooking;
using Terraria.ModLoader;

namespace Luminance.Core
{
    internal class LuminanceSystem : ModSystem
    {
        public override void PreUpdateEntities()
        {
            UpdateBossCache();
        }

        public override void Unload()
        {
            HookHelper.UnloadHooks();
        }
    }
}
