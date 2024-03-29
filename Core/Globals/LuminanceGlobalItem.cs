using Luminance.Core.Balancing;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Globals
{
    internal class LuminanceGlobalItem : GlobalItem
    {
        public override void SetDefaults(Item entity) => InternalBalancingManager.BalanceItem(entity);
    }
}
