using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics.Automators
{
    [Autoload(Side = ModSide.Client)]
    public class InterfacedProjectileDrawSystem : ModSystem
    {
        public override void Load()
        {
            On_Main.DrawProjectiles += DrawInterfaceProjectiles;
        }

        private void DrawInterfaceProjectiles(On_Main.orig_DrawProjectiles orig, Main self)
        {
            // Call the base DrawProjectiles method.
            orig(self);

            List<IDrawAdditive> additiveDrawers = Main.projectile.Take(Main.maxProjectiles).Where(p =>
            {
                return p.active && p.ModProjectile is IDrawAdditive drawer && !p.IsOffscreen();
            }).Select(p => p.ModProjectile as IDrawAdditive).ToList();

            if (additiveDrawers.Any())
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, DefaultRasterizerScreenCull, null, Main.GameViewMatrix.TransformationMatrix);
                DrawAdditiveProjectiles(additiveDrawers);
                Main.spriteBatch.End();
            }
        }

        public static void DrawAdditiveProjectiles(List<IDrawAdditive> orderedDrawers)
        {
            // Draw all projectiles that have the additive interface.
            foreach (var drawer in orderedDrawers)
                drawer.DrawAdditive(Main.spriteBatch);
        }
    }
}
