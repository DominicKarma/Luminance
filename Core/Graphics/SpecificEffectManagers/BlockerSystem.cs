using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;

namespace Luminance.Core.Graphics
{
    public class BlockerSystem : ModSystem
    {
        public record BlockCondition(bool BlockInputs, bool BlockUI, Func<bool> BlockIsInEffect)
        {
            public bool IsntDoingAnything => !BlockInputs && !BlockUI;

            public static BlockCondition None => new(false, false, () => false);
        }

        private static readonly List<BlockCondition> blockerConditions = [];

        public static bool AnythingWasBlockedLastFrame
        {
            get;
            private set;
        }

        public static void Start(bool blockInput, bool blockUi, Func<bool> condition) => Start(new(blockInput, blockUi, condition));

        public static void Start(BlockCondition condition) => blockerConditions.Add(condition);

        public override void UpdateUI(GameTime gameTime)
        {
            // Determine whether the UI or game inputs should be blocked.
            AnythingWasBlockedLastFrame = false;
            foreach (var block in blockerConditions)
            {
                // Blocks that affect neither UI nor game inputs are irrelevant and can be safely skipped over. They will be naturally disposed of in the blocker condition.
                if (block.IsntDoingAnything)
                    continue;

                if (block.BlockIsInEffect())
                {
                    if (block.BlockInputs)
                        Main.blockInput = true;
                    if (block.BlockUI)
                        Main.hideUI = true;

                    AnythingWasBlockedLastFrame = true;
                }
            }

            // Remove all block conditions that are no longer applicable, keeping track of how many conditions existed prior to the block.
            int originalBlockCount = blockerConditions.Count;
            blockerConditions.RemoveAll(b => b.IsntDoingAnything || !b.BlockIsInEffect());

            // Check if the block conditions are all gone. If they are, return things to normal on the frame they were removed.
            // Condition verification checks are not necessary for these queries because invalid blocks are already filtered out by the RemoveAll above.
            bool anythingWasRemoved = blockerConditions.Count < originalBlockCount;
            if (anythingWasRemoved)
            {
                bool anythingIsBlockingInputs = blockerConditions.Any(b => b.BlockUI);
                bool anythingIsBlockingUI = blockerConditions.Any(b => b.BlockUI);
                Main.hideUI = anythingIsBlockingInputs;
                Main.blockInput = anythingIsBlockingUI;
            }
        }

        public static void WorldEnterAndExitClearing()
        {
            // Clear all blocking conditions if any were active at the time of entering/exiting the world.
            blockerConditions.Clear();
            if (AnythingWasBlockedLastFrame)
            {
                Main.blockInput = false;
                Main.hideUI = false;
                AnythingWasBlockedLastFrame = false;
            }
        }

        public override void OnWorldLoad() => WorldEnterAndExitClearing();

        public override void OnWorldUnload() => WorldEnterAndExitClearing();
    }
}
