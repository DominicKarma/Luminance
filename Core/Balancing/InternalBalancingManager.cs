using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Luminance.Core.Balancing
{
    public class InternalBalancingManager : ModSystem
    {
        private static List<BalancingManager> balancingManagers;

        private static List<INPCHitBalancingRule>[] npcHitRulesTable;

        private static IEnumerable<ItemBalancingChange> itemBalancingChanges;

        private static IEnumerable<NPCHitBalancingChange> npcHitBalancingChanges;

        private static IEnumerable<NPCHPBalancingChange> npcHPBalancingChanges;

        internal static void RegisterManager(BalancingManager manager)
        {
            balancingManagers ??= [];
            balancingManagers.Add(manager);
        }

        public override void PostSetupContent()
        {
            List<ItemBalancingChange> totalItemBalancingChanges = [];
            List<NPCHitBalancingChange> totalNPCHitBalancingChanges = [];
            List<NPCHPBalancingChange> totalNPCHPBalancingChanges = [];

            foreach (var manager in balancingManagers)
            {
                totalItemBalancingChanges.AddRange(manager.GetItemBalancingChanges());
                totalNPCHitBalancingChanges.AddRange(manager.GetNPCHitBalancingChanges());
                totalNPCHPBalancingChanges.AddRange(manager.GetNPCHPBalancingChanges());
            }

            itemBalancingChanges = totalItemBalancingChanges;
            npcHitBalancingChanges = totalNPCHitBalancingChanges;
            npcHPBalancingChanges= totalNPCHPBalancingChanges;

            SetFactory factory = new(ContentSamples.NpcsByNetId.Count);
            npcHitRulesTable = factory.CreateCustomSet<List<INPCHitBalancingRule>>(null);
        }

        public override void Unload()
        {
            itemBalancingChanges = null;
            npcHitBalancingChanges = null;
            npcHPBalancingChanges = null;
            balancingManagers = null;
            npcHitRulesTable = null;
        }

        internal static void ApplyFromProjectile(NPC npc, ref NPC.HitModifiers modifiers, Projectile proj)
        {
            PopulateArrayIfRequired(npc.type);
            var hitContext = NPCHitContext.FromProjectile(proj);
            Dictionary<string, INPCHitBalancingRule> rulesToApplyRuleLookupTable = [];

            foreach (var rule in npcHitRulesTable[npc.type])
                if (rule.AppliesTo(npc, hitContext) && (!rulesToApplyRuleLookupTable.TryGetValue(rule.UniqueRuleName, out var previousBestRule) || previousBestRule.Priority < rule.Priority))
                    rulesToApplyRuleLookupTable[rule.UniqueRuleName] = rule;

            foreach (var rule in rulesToApplyRuleLookupTable.Values)
                rule.ApplyBalancingChange(npc, ref modifiers);
        }

        private static void PopulateArrayIfRequired(int npcType)
        {
            if (npcHitRulesTable[npcType] != null)
                return;

            var rulesCollections = npcHitBalancingChanges
                .Where(element => element.NPCType == npcType)
                .Select(element => element.BalancingRules);

            npcHitRulesTable[npcType] = [];

            foreach (var rules in rulesCollections)
                foreach (var rule in rules)
                    npcHitRulesTable[npcType].Add(rule);
        }

        internal static int GetBalancedHP(NPC npc)
        {
            int maxHP = npc.lifeMax;
            BalancePriority highestPriority = (BalancePriority)(-1);

            foreach (var change in npcHPBalancingChanges)
            {
                if (npc.type == change.NPCType && (change.ShouldApply?.Invoke() ?? true) && change.Priority > highestPriority)
                {
                    highestPriority = change.Priority;
                    maxHP = change.HP;
                }
            }
            return maxHP;
        }

        internal static void BalanceItem(Item item)
        {
            BalancePriority highestPriority = (BalancePriority)(-1);
            ItemBalancingChange changeToPerform = null;
            foreach (var change in itemBalancingChanges)
            {
                if (item.type == change.ItemType && (change.ShouldApply?.Invoke() ?? true) && change.Priority > highestPriority)
                {
                    highestPriority = change.Priority;
                    changeToPerform = change;
                }
            }

            changeToPerform?.PerformBalancing(item);
        }
    }
}
