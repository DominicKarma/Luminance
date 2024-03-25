using Luminance.Core.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace Luminance.Core.Cutscenes
{
    public sealed class CutsceneManager : ModSystem
    {
        internal static Queue<Cutscene> CutscenesQueue = new();

        /// <summary>
        /// The cutscene that is currently active.
        /// </summary>
        internal static Cutscene ActiveCutscene
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether any cutscenes are active.
        /// </summary>
        public static bool AnyActive => ActiveCutscene != null;

        /// <summary>
        /// Queues a cutscene to be played.
        /// </summary>
        /// <param name="cutscene"></param>
        public static void QueueCutscene(Cutscene cutscene)
        {
            if (Main.netMode != NetmodeID.Server)
                CutscenesQueue.Enqueue(cutscene);
        }

        /// <summary>
        /// Returns whether the provided cutscene is active, via checking the names.
        /// </summary>
        public static bool IsActive(Cutscene cutscene)
        {
            if (ActiveCutscene == null)
                return false;

            return ActiveCutscene.Name == cutscene.Name;
        }

        public override void Load()
        {
            Main.OnPostDraw += PostDraw;
            On_Main.DrawNPCs += DrawToWorld;
        }

        public override void Unload()
        {
            Main.OnPostDraw -= PostDraw;
            On_Main.DrawNPCs -= DrawToWorld;
        }

        public override void PostUpdateEverything()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            if (ActiveCutscene == null)
            {
                if (CutscenesQueue.TryDequeue(out Cutscene cutscene))
                {
                    ActiveCutscene = cutscene;
                    ActiveCutscene.Timer = 0;
                    ActiveCutscene.IsActive = true;
                    ActiveCutscene.OnBegin();

                    if (ActiveCutscene.GetBlockCondition != BlockerSystem.BlockCondition.None)
                        BlockerSystem.Start(ActiveCutscene.GetBlockCondition);
                }
            }

            if (ActiveCutscene != null)
            {
                ActiveCutscene.Update();
                ActiveCutscene.Timer++;

                if (ActiveCutscene.EndAbruptly || ActiveCutscene.Timer > ActiveCutscene.CutsceneLength)
                {
                    ActiveCutscene.OnEnd();
                    ActiveCutscene.Timer = 0;
                    ActiveCutscene.IsActive = false;
                    ActiveCutscene.EndAbruptly = false;
                    ActiveCutscene = null;
                }
            }
        }

        public override void ModifyScreenPosition() => ActiveCutscene?.ModifyScreenPosition();

        public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform) => ActiveCutscene?.ModifyTransformMatrix(ref Transform);

        private void DrawToWorld(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
        {
            orig(self, behindTiles);
            ActiveCutscene?.DrawToWorld(Main.spriteBatch);
        }

        internal static void DrawWorld(RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor) => ActiveCutscene?.DrawWorld(Main.spriteBatch, screenTarget1);

        private void PostDraw(GameTime obj) => ActiveCutscene?.PostDraw(Main.spriteBatch);
    }
}
