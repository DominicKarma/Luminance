using Microsoft.Xna.Framework;

namespace Luminance.Core.Graphics
{
    /// <summary>
    /// Represents a texture on an <see cref="Graphics.Atlas"/>. Contains its position on the atlas, and a unique string identifier.<br/>
    /// Use <see cref="AtlasManager.GetTexture(string)"/> to retrieve an instance with the given string identifier.
    /// </summary>
    public record AtlasTexture(string Name, Atlas Atlas, Rectangle Frame);
}
