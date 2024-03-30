using Luminance.Core.Balancing;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Globals
{
    internal class LuminanceGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        private bool HasBalancedHP;

        public override bool PreAI(NPC npc)
        {
            if (npc.TryGetGlobalNPC<LuminanceGlobalNPC>(out var globalNPC) && !globalNPC.HasBalancedHP)
            {
                int maxHP = InternalBalancingManager.GetBalancedHP(npc);

                if (maxHP != npc.lifeMax)
                {
                    npc.life = npc.lifeMax = maxHP;
                    globalNPC.HasBalancedHP = true;
                    npc.netUpdate = true;
                }
            }
            return true;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) => InternalBalancingManager.ApplyFromProjectile(npc, ref modifiers, projectile);
    }
}
