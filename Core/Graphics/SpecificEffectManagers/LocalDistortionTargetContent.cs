using System.Collections.Generic;
using System.Linq;
using Luminance.Core.Graphics.Automators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace Luminance.Core.Graphics.SpecificEffectManagers
{
    public class LocalDistortionTargetContent : ARenderTargetContentByRequest
    {
        protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            // Initialize the underlying render target if necessary.
            Vector2 size = new(Main.screenWidth, Main.screenHeight);
            PrepareARenderTarget_WithoutListeningToEvents(ref _target, Main.instance.GraphicsDevice, (int)size.X, (int)size.Y, RenderTargetUsage.PreserveContents);

            device.SetRenderTarget(_target);
            device.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            DrawDistortion();
            Main.spriteBatch.End();

            device.SetRenderTarget(null);

            // Mark preparations as completed.
            _wasPrepared = true;
        }

        private static void DrawDistortion()
        {
            List<IDrawLocalDistortion> distortionDrawers = Main.projectile.Take(Main.maxProjectiles).Where(p =>
            {
                return p.active && p.ModProjectile is IDrawLocalDistortion drawer && !p.IsOffscreen();
            }).Select(p => p.ModProjectile as IDrawLocalDistortion).ToList();

            // Draw all projectiles that have the distortion interface.
            foreach (var drawer in distortionDrawers)
                drawer.DrawLocalDistortion(Main.spriteBatch);
        }
    }
}
