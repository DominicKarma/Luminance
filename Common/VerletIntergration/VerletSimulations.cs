using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;

namespace Luminance.Common.VerletIntergration
{
    /// <summary>
    /// Contains various simulations for verlet chains.
    /// </summary>
    public static class VerletSimulations
    {
        /// <summary>
        /// Performs a verlet simulation, configure specific details with <paramref name="settings"/>.
        /// </summary>
        /// <param name="segments">The segments to run through the simulaion.</param>
        /// <param name="segmentDistance">The ideal distance between each segment.</param>
        /// <param name="settings">The settings for the simulation.</param>
        /// <param name="loops">The number of loops to perform to calculate the final positions. More is more precise, but more costly.</param>
        /// <returns>The segments post simulation.</returns>
        public static List<VerletSegment> VerletSimulation(List<VerletSegment> segments, float segmentDistance, VerletSettings settings, int loops = 10)
        {
            // https://youtu.be/PGk0rnyTa1U?t=400 is a good verlet integration chains reference.
            List<int> groundHitSegments = [];
            for (int i = segments.Count - 1; i >= 0; i--)
            {
                var segment = segments[i];
                if (!segment.Locked)
                {
                    Vector2 positionBeforeUpdate = segment.Position;

                    Vector2 gravityForce = Vector2.UnitY * settings.Gravity;
                    float maxFallSpeed = settings.MaxFallSpeed;

                    if (settings.SlowInWater && Collision.WetCollision(segment.Position, 1, 1))
                    {
                        gravityForce *= 0.4f;
                        maxFallSpeed *= 0.3f;
                    }

                    Vector2 velocity = segment.Velocity + gravityForce;
                    if (settings.TileCollision)
                    {
                        velocity = Collision.TileCollision(segment.Position, velocity, (int)segmentDistance, (int)segmentDistance);
                        groundHitSegments.Add(i);
                    }

                    if (velocity.Y > maxFallSpeed)
                        velocity.Y = maxFallSpeed;

                    if (settings.ConserveEnergy)
                        segment.Position += (segment.Position - segment.OldPosition) * 0.03f;

                    segment.Position += velocity;
                    segment.Velocity = velocity;
                    segment.Position.X = Lerp(segment.Position.X, segments[0].Position.X, 0.04f);

                    segment.OldPosition = positionBeforeUpdate;
                }
            }

            int segmentCount = segments.Count;

            for (int k = 0; k < loops; k++)
            {
                for (int j = 0; j < segmentCount - 1; j++)
                {
                    var pointA = segments[j];
                    var pointB = segments[j + 1];
                    Vector2 segmentCenter = (pointA.Position + pointB.Position) / 2f;
                    Vector2 segmentDirection = (pointA.Position - pointB.Position).SafeNormalize(Vector2.UnitY);

                    if (!pointA.Locked && !groundHitSegments.Contains(j))
                        pointA.Position = segmentCenter + segmentDirection * segmentDistance / 2f;

                    if (!pointB.Locked && !groundHitSegments.Contains(j + 1))
                        pointB.Position = segmentCenter - segmentDirection * segmentDistance / 2f;

                    segments[j] = pointA;
                    segments[j + 1] = pointB;
                }
            }

            return segments;
        }

        /// <summary>
        /// Performs a verlet simulation for a rope with a fixed end(s), configure specific details with <paramref name="settings"/>.
        /// </summary>
        /// <param name="segments">The segments to run through the simulaion.</param>
        /// <param name="topPosition">The fixed postion of the start of the rope.</param>
        /// <param name="idealRopeLength">The ideal length of the rope.</param>
        /// <param name="settings">The settings for the simulation.</param>
        /// <param name="endPosition">The optional, fixed position of the end of the rope.</param>
        /// <returns>The segments post simulation.</returns>
        public static List<VerletSegment> RopeVerletSimulation(List<VerletSegment> segments, Vector2 topPosition, float idealRopeLength, VerletSettings settings, Vector2? endPosition = null)
        {
            if (segments is null || !segments.Any())
                return segments;

            segments[0].Position = topPosition;
            segments[0].Locked = true;

            if (endPosition.HasValue)
            {
                segments[^1].Position = endPosition.Value;
                segments[^1].Locked = true;
            }
            segments = VerletSimulation(segments, idealRopeLength / segments.Count, settings, segments.Count * 2 + 10);
            return segments;
        }
    }
}
