using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public class BlockerSystem : ModSystem
    {
        /// <summary>
        /// Represents a condition that dictates an input block.
        /// </summary>
        /// <param name="BlockInputs">Whether inputs should be blocked.</param>
        /// <param name="BlockUI">Whether UI interactions and drawing should be blocked.</param>
        /// <param name="BlockIsInEffect">The condition delegate that dictates whether the block is in effect.</param>
        public record BlockCondition(bool BlockInputs, bool BlockUI, Func<bool> BlockIsInEffect)
        {
            /// <summary>
            /// Whether this block affects nothing.
            /// </summary>
            public bool IsntDoingAnything => !BlockInputs && !BlockUI;

            /// <summary>
            /// A default-object blocking condition that does nothing.
            /// </summary>
            public static BlockCondition None => new(false, false, () => false);
        }

        private static readonly List<BlockCondition> blockerConditions = [];

        /// <summary>
        /// Whether a block of any kind was in effect lack frame or not.
        /// </summary>
        public static bool AnythingWasBlockedLastFrame
        {
            get;
            private set;
        }

        /// <summary>
        /// Starts a new block effect with a given condition.
        /// </summary>
        /// <param name="blockInput">Whether inputs should be blocked.</param>
        /// <param name="blockUi">Whether UI interactions and drawing should be blocked.</param>
        /// <param name="condition">The condition delegate that dictates whether the block is in effect.</param>
        public static void Start(bool blockInput, bool blockUi, Func<bool> condition) => Start(new(blockInput, blockUi, condition));

        /// <summary>
        /// Starts a new block effect based on a given <see cref="BlockCondition"/>.
        /// </summary>
        /// <param name="condition">The configuration that dictates how the black should operate.</param>
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
