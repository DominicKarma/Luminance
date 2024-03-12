using Microsoft.Xna.Framework;
using Terraria;

namespace KarmaLibrary.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        /// Determine whether a given destination point is right of a given entity.
        /// </summary>
        /// <param name="entity">The entity to check relative to.</param>
        /// <param name="destination">The point to check.</param>
        public static bool OnRightSideOf(this Entity entity, Vector2 destination) =>
            entity.Center.X >= destination.X;

        /// <summary>
        /// Determine whether a given destination point is right of a given entity.
        /// </summary>
        /// <param name="entity">The entity to check relative to.</param>
        /// <param name="other">The other entity whose center point should be checked.</param>
        public static bool OnRightSideOf(this Entity entity, Entity other) =>
            entity.Center.X >= other.Center.X;
    }
}
