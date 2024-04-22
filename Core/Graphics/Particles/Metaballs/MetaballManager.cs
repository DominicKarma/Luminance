using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public class MetaballManager : ModSystem
    {
        internal static readonly List<MetaballType> MetaballTypes = new();

        public override void OnModLoad()
        {
            RenderTargetManager.RenderTargetUpdateLoopEvent += PrepareMetaballTargets;
        }

        public override void OnModUnload()
        {
            Main.QueueMainThreadAction(() =>
            {
                foreach (var type in MetaballTypes)
                    type?.Dispose();

                MetaballTypes.Clear();
            });
        }

        public override void OnWorldUnload()
        {
            foreach (var type in MetaballTypes)
                type.ClearInstances();
        }

        private void PrepareMetaballTargets()
        {
            if (Main.gameMenu)
                return;

            var activeTypes = MetaballTypes.Where(type => type.ShouldRender);

            if (!activeTypes.Any())
                return;

            // TODO: Can this be optimised in any way? Feels kind of stinky, the differenting layers could potentially be handled entirely in the shader instead of a bunch of RTs?
            foreach (var type in activeTypes)
            {
                if (!Main.gamePaused)
                    type.Update();

                foreach (var target in type.LayerTargets)
                {
                    target.SwapToRenderTarget();

                    if (!type.PerformCustomSpritebatchBegin(Main.spriteBatch))
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);

                    type.DrawInstances();

                    Main.spriteBatch.End();
                }

                Main.instance.GraphicsDevice.SetRenderTarget(null);
            }
        }

        internal static void DrawMetaballTargets()
        {
            foreach (var type in MetaballTypes.Where(type => type.ShouldRender))
            {
                // TODO: Same as above, this does not seem very optimal.
                if (!type.DrawnManually)
                    type.RenderLayerWithShader();
            }
        }
    }
}
