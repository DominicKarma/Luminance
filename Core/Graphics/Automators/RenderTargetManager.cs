using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace KarmaLibrary.Core.Graphics.Automators
{
    public class RenderTargetManager : ModSystem
    {
        internal static List<ManagedRenderTarget> ManagedTargets = [];

        public delegate void RenderTargetUpdateDelegate();

        public static event RenderTargetUpdateDelegate RenderTargetUpdateLoopEvent;

        public static readonly int TimeUntilUntilUnusedTargetsAreDisposed = SecondsToFrames(5f);

        internal static void DisposeOfTargets()
        {
            if (ManagedTargets is null)
                return;

            Main.QueueMainThreadAction(() =>
            {
                foreach (ManagedRenderTarget target in ManagedTargets)
                    target?.Dispose();
                ManagedTargets.Clear();
            });
        }

        // For some reason, adding more arguments to this constructor causes really low end pcs to crash. I cannot find out why, so do not use them.
        public static RenderTarget2D CreateScreenSizedTarget(int screenWidth, int screenHeight) => new(Main.instance.GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.Bgra4444, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);

        public override void OnModLoad()
        {
            // Prepare update functionalities.
            Main.OnPreDraw += HandleTargetUpdateLoop;
        }

        public override void OnModUnload()
        {
            // Clear any lingering GPU resources.
            DisposeOfTargets();

            // Unsubscribe from the OnPreDraw event.
            Main.OnPreDraw -= HandleTargetUpdateLoop;

            // Reset the update loop event.
            RenderTargetUpdateLoopEvent = null;
        }

        /// <summary>
        /// Evaluates all active render targets, checking if they need to be reset or disposed of.
        /// </summary>
        private void HandleTargetUpdateLoop(GameTime obj)
        {
            RenderTargetUpdateLoopEvent?.Invoke();

            // Increment the render target lifetime timers. Once this reaches a certain threshold, the render target is automatically disposed.
            // This timer is reset back to 0 if it's accessed anywhere. The intent of this is to ensure that render targets that are not relevant at a given point in time
            // don't sit around in VRAM forever.
            // The managed wrapper that is the ManagedRenderTarget instance will persist in the central list of this class, but the amount of memory that holds is
            // negligible compared to the unmanaged texture data that the RenderTarget2D itself stores when not disposed.
            for (int i = 0; i < ManagedTargets.Count; i++)
            {
                // Check if the render target needs to be reset.
                ManagedRenderTarget target = ManagedTargets[i];
                PerformTargetResetCheck(target);

                // Determine whether the target is eligible to be automatically disposed.
                if (!target.SubjectToGarbageCollection || target.IsUninitialized)
                    continue;

                target.TimeSinceLastUsage++;
                if (target.TimeSinceLastUsage >= TimeUntilUntilUnusedTargetsAreDisposed)
                    target.Dispose();
            }
        }

        /// <summary>
        /// Checks a render target for whether it needs to be reset for any reason. If so, the reset is scheduled on the main thread for the next frame.
        /// </summary>
        /// <param name="target">The render target to check.</param>
        private static void PerformTargetResetCheck(ManagedRenderTarget target)
        {
            // Don't attempt to recreate targets that are already initialized or shouldn't be recreated.
            int width = Main.screenWidth;
            int height = Main.screenHeight;
            bool incorrectSize = (width != target.Width || height != target.Height) && target.ShouldResetUponScreenResize;
            if (target is null || (!target.IsDisposed && !incorrectSize) || target.WaitingForFirstInitialization)
                return;

            Main.QueueMainThreadAction(() =>
            {
                target.Recreate(width, height);
            });
        }
    }
}
