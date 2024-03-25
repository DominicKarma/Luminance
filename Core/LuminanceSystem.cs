using Terraria.ModLoader;

namespace Luminance.Core
{
    internal class LuminanceSystem : ModSystem
    {
        public override void PreUpdateEntities()
        {
            UpdateBossCache();
        }
    }
}
