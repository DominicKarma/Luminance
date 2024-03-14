using Microsoft.Xna.Framework.Graphics;

namespace Luminance.Core.Graphics
{
    /// <summary>
    /// Use to sucessfully render primitives with pixelation with an NPC or Projectile.
    /// </summary>
    public interface IPixelatedPrimitiveRenderer
    {
        /// <summary>
        /// The layer to render the primitive(s) to.
        /// </summary>
        PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.BeforeProjectiles;

        /// <summary>
        /// Render primitives that use pixelation here.
        /// </summary>
        void RenderPixelatedPrimitives(SpriteBatch spriteBatch);
    }
}
