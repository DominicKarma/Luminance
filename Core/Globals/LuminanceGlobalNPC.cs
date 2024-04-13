using Luminance.Core.Balancing;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Globals
{
    public class LuminanceGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) => InternalBalancingManager.ApplyFromProjectile(npc, ref modifiers, projectile);
    }
}
