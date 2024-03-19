using Microsoft.Xna.Framework;

namespace Luminance.Core.Graphics
{
    /// <summary>
    /// Represents a base metaball particle instance.
    /// </summary>
    public sealed class MetaballInstance
    {
        /// <summary>
        /// The position of the metaball particle.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// The velocity of the metaball particle.
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// The size of the metaball particle.
        /// </summary>
        public float Size;

        /// <summary>
        /// An array of length 4 that contains optional extra info for per metaball information. What this is used for depends on the <see cref="MetaballType"/> that owns this particle instance.
        /// </summary>
        public float[] ExtraInfo;

        internal MetaballInstance(Vector2 center, Vector2 velocity, float size, float extraInfo0 = 0f, float extraInfo1 = 0, float extraInfo2 = 0, float extraInfo3 = 0)
        {
            Center = center;
            Velocity = velocity;
            Size = size;
            ExtraInfo = new float[4] { extraInfo0, extraInfo1, extraInfo2, extraInfo3 };
        }
    }
}
