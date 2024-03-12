using Microsoft.Xna.Framework.Graphics;

namespace KarmaLibrary.Core.Graphics.Automators
{
    public interface IDrawLocalDistortion
    {
        public void DrawLocalDistortion(SpriteBatch spriteBatch);

        public void DrawLocalDistortionExclusion(SpriteBatch spriteBatch) { }
    }
}
