using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Balancing
{
    /// <summary>
    /// This class contains several default balancing rules that you can use. You can also make your own by creating a class/record that implements <see cref="INPCHitBalancingRule"/>.
    /// </summary>
    public static class DefaultNPCBalancingRules
    {
        /// <summary>
        /// Reduces all incoming damage from projectiles of specified types.
        /// </summary>
        public record ProjectileResistBalancingRule(float DamageMultiplier, BalancePriority Priority, params int[] ProjectileTypes) : INPCHitBalancingRule
        {
            BalancePriority INPCHitBalancingRule.Priority => Priority;

            string INPCHitBalancingRule.UniqueRuleName => "LuminanceProjectileResistBalancingRule";

            bool INPCHitBalancingRule.AppliesTo(NPC npc, NPCHitContext hitContext) => ProjectileTypes.Contains(hitContext.ProjectileType ?? -1);

            void INPCHitBalancingRule.ApplyBalancingChange(NPC npc, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= DamageMultiplier;
        }

        /// <summary>
        /// Appies to all incoming damage caused by projectiles with remaining pierce.
        /// </summary>
        public record PierceResistBalancingRule(float DamageMultiplier, BalancePriority Priority) : INPCHitBalancingRule
        {
            BalancePriority INPCHitBalancingRule.Priority => Priority;

            string INPCHitBalancingRule.UniqueRuleName => "LuminancePierceResistBalancingRule";

            bool INPCHitBalancingRule.AppliesTo(NPC npc, NPCHitContext hitContext) => hitContext.Pierce is > 1 or -1;

            void INPCHitBalancingRule.ApplyBalancingChange(NPC npc, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= DamageMultiplier;
        }

        /// <summary>
        /// Applies to all incoming damage from projectiles of specified damage classes.
        /// </summary>
        public record ClassResistBalancingRule(float DamageMultiplier, BalancePriority Priority, DamageClass Class) : INPCHitBalancingRule
        {
            BalancePriority INPCHitBalancingRule.Priority => Priority;

            string INPCHitBalancingRule.UniqueRuleName => "LuminanceClassResistBalancingRule";

            bool INPCHitBalancingRule.AppliesTo(NPC npc, NPCHitContext hitContext) => hitContext.ClassType.Equals(Class);

            void INPCHitBalancingRule.ApplyBalancingChange(NPC npc, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= DamageMultiplier;
        }
    }
}
