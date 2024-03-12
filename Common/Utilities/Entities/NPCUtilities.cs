using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace KarmaLibrary.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        /// Defines a given <see cref="NPC"/>'s HP based on the current difficulty mode.
        /// </summary>
        /// <param name="npc">The NPC to set the HP for.</param>
        /// <param name="normalModeHP">HP value for normal mode</param>
        /// <param name="expertModeHP">HP value for expert mode</param>
        /// <param name="masterModeHP">HP value for master mode</param>
        public static void SetLifeMaxByMode(this NPC npc, int normalModeHP, int expertModeHP, int masterModeHP)
        {
            npc.lifeMax = normalModeHP;
            if (Main.expertMode)
                npc.lifeMax = expertModeHP;
            if (Main.masterMode)
                npc.lifeMax = masterModeHP;
        }

        /// <summary>
        /// Excludes a given <see cref="NPC"/> from the bestiary completely.
        /// </summary>
        /// <param name="npc">The NPC to apply the bestiary deletion to.</param>
        public static void ExcludeFromBestiary(this ModNPC npc)
        {
            NPCID.Sets.NPCBestiaryDrawModifiers value = new()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(npc.Type, value);
        }

        /// <summary>
        /// A simple utility that gets an <see cref="NPC"/>'s <see cref="NPC.ModNPC"/> instance as a specific type without having to do clunky casting.
        /// </summary>
        /// <typeparam name="T">The ModNPC type to convert to.</typeparam>
        /// <param name="npc">The NPC to access the ModNPC from.</param>
        public static T As<T>(this NPC npc) where T : ModNPC
        {
            return npc.ModNPC as T;
        }

        /// <summary>
        /// Checks if any bosses are present.
        /// </summary>
        public static bool AnyBosses()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i] is null || !Main.npc[i].active)
                    continue;

                NPC npc = Main.npc[i];
                bool isEaterOfWorlds = npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsTail;
                if (npc.boss || isEaterOfWorlds)
                    return true;
            }

            return false;
        }
    }
}
