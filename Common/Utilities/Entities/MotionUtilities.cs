using Microsoft.Xna.Framework;
using Terraria;

namespace Luminance.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        ///     Calculates the direction to a given position from an entity with safely performed underlying normalization.
        /// </summary>
        /// <param name="entity">The entity to perform the calculations relative to.</param>
        /// <param name="destination">The position to get the direction towards.</param>
        public static Vector2 SafeDirectionTo(this Entity entity, Vector2 destination)
        {
            return (destination - entity.Center).SafeNormalize(Vector2.Zero);
        }

        /// <summary>
        ///     Calculates the X direction to a given position from an entity.
        /// </summary>
        /// <param name="entity">The entity to perform the calculations relative to.</param>
        /// <param name="destination">The position to get the direction towards.</param>
        public static float HorizontalDirectionTo(this Entity entity, Vector2 destination)
        {
            // If on the right side of the target, the direction to the target is to the left, and vice versa.
            // This is why a negative sign is used in this calculation.
            return -entity.OnRightSideOf(destination).ToDirectionInt();
        }

        public static void SmoothFlyNear(this Entity entity, Vector2 destination, float movementSharpnessInterpolant, float movementSmoothnessInterpolant)
        {
            // Calculate the ideal velocity. The closer movementSharpnessInterpolant is to 1, the more closely the entity will hover exactly at the destination.
            // Lower, greater than zero values result in a greater tendency to hover in the general vicinity of the destination, rather than zipping straight towards it.
            Vector2 idealVelocity = (destination - entity.Center) * Clamp(movementSharpnessInterpolant, 0.0001f, 1f);

            // Interpolate towards the ideal velocity. The closer movementSmoothnessInterpolant is to 1, the more opportunities the entity has for overshooting and
            // more "curvy" motion.
            entity.velocity = Vector2.Lerp(entity.velocity, idealVelocity, Clamp(1f - movementSmoothnessInterpolant, 0.0001f, 1f));
        }

        // The math for this is very similar to the above method, albeit with one major difference:
        // Around the destination there is a "slowdown radius" wherein the entity will attempt to come to a halt.
        // This behavior is beneficial for cases where exact hovering is not desired, but getting close to a destination is, such as a short lived, fast redirect.
        public static void SmoothFlyNearWithSlowdownRadius(this Entity entity, Vector2 destination, float movementSharpnessInterpolant, float movementSmoothnessInterpolant, float slowdownRadius)
        {
            // Calculate the distance to the slowdown radius. If the entity is within the slowdown radius, the distance is registered as zero.
            float distanceToSlowdownRadius = entity.Distance(destination) - slowdownRadius;
            if (distanceToSlowdownRadius < 0f)
                distanceToSlowdownRadius = 0f;

            // Determine the ideal speed based on the distance to the slowdown radius rather than the destination itself.
            // This math is functionally equivalent to the idealVelocity vector in SmoothFlyNear, barring that quirk.
            float idealSpeed = distanceToSlowdownRadius * Clamp(movementSharpnessInterpolant, 0.0001f, 1f);
            Vector2 idealVelocity = entity.SafeDirectionTo(destination) * idealSpeed;

            // Same velocity interpolation behavior as SmoothFlyNear.
            entity.velocity = Vector2.Lerp(entity.velocity, idealVelocity, Clamp(1f - movementSmoothnessInterpolant, 0.0001f, 1f));
        }
    }
}
