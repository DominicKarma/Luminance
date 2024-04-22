using Microsoft.Xna.Framework;

namespace Luminance.Common.VerletIntergration
{
    /// <summary>
    /// Represents a verlet segment, for use in verlet simulatons.
    /// </summary>
    /// <param name="position">The position of this segment.</param>
    /// <param name="velocity">The velocity of this segment.</param>
    /// <param name="locked">Whether the segment is locked in place. If true, its position will not update via simulations.</param>
    public class VerletSegment(Vector2 position, Vector2 velocity, bool locked = false)
    {
        public Vector2 Position = position;

        public Vector2 OldPosition = position;

        public Vector2 Velocity = velocity;

        public bool Locked = locked;
    }
}
