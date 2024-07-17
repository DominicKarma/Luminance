using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public class RenderTargetManager : ModSystem
    {
        /// <summary>
        ///     The set of all managed render targets.
        /// </summary>
        internal static List<ManagedRenderTarget> ManagedTargets = [];

        /// <summary>
        ///     The event responsible for updating all render targets.
        /// </summary>
        /// <remarks>
        ///     Should be subscribed for the purpose of rendering into render targets, to ensure that their contents are defined in a way that does not interfere with other parts of the game's rendering loop.
        /// </remarks>
        public static event RenderTargetUpdateDelegate RenderTargetUpdateLoopEvent;

        /// <summary>
        ///     How long standard render targets can go, in frames, before they are subject to automatic disposal.
        /// </summary>
        public static readonly int TimeUntilUntilUnusedTargetsAreDisposed = SecondsToFrames(10f);

        public delegate void RenderTargetUpdateDelegate();

        /// <summary>
        ///     Causes all managed render targets to become disposed, freeing their unmanaged resources.
        /// </summary>
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

        public override void OnModLoad()
        {
            // Prepare update functionalities.
            Main.OnPreDraw += HandleTargetUpdateLoop;
            On_Main.SetDisplayMode += ResizeScreenSizedTargets;
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
        ///     Evaluates all active render targets, checking if they need to be reset or disposed of.
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
                ManagedRenderTarget target = ManagedTargets[i];

                // Determine whether the target is eligible to be automatically disposed.
                if (!target.SubjectToGarbageCollection || target.IsUninitialized)
                    continue;

                target.TimeSinceLastUsage++;
                if (target.TimeSinceLastUsage >= TimeUntilUntilUnusedTargetsAreDisposed)
                    target.Dispose();
            }
        }

        private void ResizeScreenSizedTargets(On_Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            int oldScreenWidth = Main.screenWidth;
            int oldScreenHeight = Main.screenHeight;
            orig(width, height, fullscreen);

            if (Main.screenWidth == oldScreenWidth && Main.screenHeight == oldScreenHeight)
                return;

            for (int i = 0; i < ManagedTargets.Count; i++)
            {
                ManagedRenderTarget target = ManagedTargets[i];
                if (target is null || target.WaitingForFirstInitialization)
                    continue;

                Main.QueueMainThreadAction(() =>
                {
                    target.Recreate(Main.screenWidth, Main.screenHeight);
                });
            }
        }
    }
}
