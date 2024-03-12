using Microsoft.Xna.Framework.Graphics;

namespace Luminance.Core.Graphics.Automators
{
    public interface IDrawLocalDistortion
    {
        public void DrawLocalDistortion(SpriteBatch spriteBatch);

        public void DrawLocalDistortionExclusion(SpriteBatch spriteBatch) { }
    }
}
