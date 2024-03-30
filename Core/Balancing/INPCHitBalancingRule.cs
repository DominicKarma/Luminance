using Terraria;

namespace Luminance.Core.Balancing
{
    /// <summary>
    /// Use to create new balancing rules by attaching to classes/records.
    /// </summary>
    public interface INPCHitBalancingRule
    {
        /// <summary>
        /// A unique name for the rule. Good practice is to prefix it with your mods name.
        /// </summary>
        string UniqueRuleName { get; }

        /// <summary>
        /// The priority of this balancing rule.
        /// </summary>
        BalancePriority Priority { get; }

        /// <summary>
        /// Whether the rule should apply to the npc based on the hit context.
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="hitContext"></param>
        /// <returns></returns>
        bool AppliesTo(NPC npc, NPCHitContext hitContext);

        /// <summary>
        /// Apply any balancing change(s) here.
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="modifiers"></param>
        void ApplyBalancingChange(NPC npc, ref NPC.HitModifiers modifiers);
    }
}
