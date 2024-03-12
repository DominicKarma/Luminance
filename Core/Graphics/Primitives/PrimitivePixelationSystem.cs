using KarmaLibrary.Core.Graphics.Automators;
using KarmaLibrary.Core.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace KarmaLibrary.Core.Graphics.Primitives
{
    public class PrimitivePixelationSystem : ModSystem
    {
        private static bool primsWereDrawnLastFrame;

        public static ManagedRenderTarget PixelationTarget
        {
            get;
            private set;
        }

        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Main.QueueMainThreadAction(() => PixelationTarget = new(true, RenderTargetManager.CreateScreenSizedTarget));
            RenderTargetManager.RenderTargetUpdateLoopEvent += PreparePixelationTarget;
            On_Main.DoDraw_DrawNPCsOverTiles += DrawPixelationTarget;
        }

        private void PreparePixelationTarget()
        {
            bool primsExist = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active)
                    continue;

                bool modProjectileHasInterface = p.ModProjectile is IDrawPixelated;
                if (!modProjectileHasInterface)
                    continue;

                primsExist = true;
                break;
            }

            if (!primsExist)
            {
                primsWereDrawnLastFrame = false;
                return;
            }

            // Start a spritebatch, as a Begin call does not exist before the method that's being detoured.
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);

            // Go to the pixelation target.
            var gd = Main.instance.GraphicsDevice;
            gd.SetRenderTarget(PixelationTarget);
            gd.Clear(Color.Transparent);

            // Draw prims to the render target.
            primsWereDrawnLastFrame = false;
            DrawPixelatedProjectiles();

            // Return to the backbuffer.
            gd.SetRenderTarget(null);

            // Prepare the sprite batch for the next draw cycle.
            Main.spriteBatch.End();
        }

        private void DrawPixelationTarget(On_Main.orig_DoDraw_DrawNPCsOverTiles orig, Main self)
        {
            // Simply call orig if no prims were drawn as an optimization.
            if (!primsWereDrawnLastFrame)
            {
                orig(self);
                return;
            }

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

            // Apply the pixelation shader.
            var pixelationShader = ShaderManager.GetShader("PixelationShader");
            pixelationShader.TrySetParameter("pixelationFactor", Vector2.One * 2f / PixelationTarget.Size());
            pixelationShader.Apply();

            Main.spriteBatch.Draw(PixelationTarget, Main.screenLastPosition - Main.screenPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();

            orig(self);
        }

        private static void DrawPixelatedProjectiles()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.ModProjectile is not IDrawPixelated primDrawer)
                    continue;

                primDrawer.DrawWithPixelation();
                primsWereDrawnLastFrame = true;
            }
        }
    }
}
