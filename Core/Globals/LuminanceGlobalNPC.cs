using Luminance.Core.Balancing;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Globals
{
    public class LuminanceGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public delegate void HPBalanceDelegate(NPC npc, int maxHP);

        /// <summary>
        /// Called whenever a npc has its hp adjusted for balance.
        /// </summary>
        public static event HPBalanceDelegate OnHPBalance;

        public bool HasBalancedHP
        {
            get;
            private set;
        }

        public override void Unload()
        {
            OnHPBalance = null;
        }

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
                    OnHPBalance?.Invoke(npc, maxHP);
                }
            }
            return true;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) => InternalBalancingManager.ApplyFromProjectile(npc, ref modifiers, projectile);
    }
}
