using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Luminance.Common.Easings
{
    public class PiecewiseRotation
    {
        /// <summary>
        ///     A piecewise rotation curve that takes up a part of the domain of a <see cref="PiecewiseRotation"/>, specifying the equivalent range and curvature in said domain.
        /// </summary>
        public record CurveSegment(Quaternion StartingRotation, Quaternion EndingRotation, float AnimationStart, float AnimationEnd, EasingCurves.Curve Curve, EasingType CurveType);

        /// <summary>
        ///     The list of <see cref="CurveSegment"/> that encompass the entire 0-1 domain of this function.
        /// </summary>
        protected List<CurveSegment> segments = [];

        /// <summary>
        ///     Adds a new interpolation step to this piecewise rotation curve, encompassing a part of the animation function's domain.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="curveType">The type of curve to use.</param>
        /// <param name="endingRotation">The ending rotation to interpolate between.</param>
        /// <param name="animationEnd">The ending interpolant that this rotation should encompass.</param>
        /// <param name="startingRotation">The starting rotation to interpolate between. Uses the last <see cref="segments"/> rotation by default, assuming it isn't empty.</param>
        /// <returns>The original rotation, for method chaining purposes.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public PiecewiseRotation Add(EasingCurves.Curve curve, EasingType curveType, Quaternion endingRotation, float animationEnd, Quaternion? startingRotation = null)
        {
            float animationStart = segments.Any() ? segments.Last().AnimationEnd : 0f;
            startingRotation ??= segments.Any() ? segments.Last().EndingRotation : Quaternion.Identity;
            if (animationEnd <= 0f || animationEnd > 1f)
                throw new InvalidOperationException("A piecewise animation curve segment cannot have a domain outside of 0-1.");

            segments.Add(new(startingRotation.Value, endingRotation, animationStart, animationEnd, curve, curveType));

            return this;
        }

        /// <summary>
        ///     Evaluates the overall rotation curve, interpolating between configurations as necessary.
        /// </summary>
        /// <param name="animationInterpolant">The animation completion interpolant.</param>
        /// <param name="takeOptimalRoute">Whether rotations should take the optimal route in cases where the rotational difference exceeds 180 degrees. One might want this disabled for giant swings, so that the full arc is travelled.</param>
        /// <param name="inversionDirection"></param>
        public Quaternion Evaluate(float animationInterpolant, bool takeOptimalRoute, int inversionDirection)
        {
            // Clamp the animation interpolant to 0-1, since all other ranges of values will result in undefined behavior.
            animationInterpolant = Saturate(animationInterpolant);

            CurveSegment segmentToUse = segments.Find(s => animationInterpolant >= s.AnimationStart && animationInterpolant <= s.AnimationEnd);
            float curveLocalInterpolant = InverseLerp(segmentToUse.AnimationStart, segmentToUse.AnimationEnd, animationInterpolant);
            float segmentInterpolant = segmentToUse.Curve.Evaluate(segmentToUse.CurveType, 0f, 1f, curveLocalInterpolant);

            // Spherically interpolate piecemeal between the quaternions.
            // Unlike a single Quaternion.Slerp, which would typically invert negative dot products, this has the ability to take un-optimal routes to the destination angle, which is desirable for things such as big swings.
            Quaternion start = segmentToUse.StartingRotation;
            Quaternion end = segmentToUse.EndingRotation;

            start.Normalize();
            end.Normalize();
            float similarity = Quaternion.Dot(start, end);
            if (similarity.NonZeroSign() != inversionDirection && takeOptimalRoute)
            {
                similarity *= -1f;
                start *= -1f;
            }

            float angle = Acos(Clamp(similarity, -0.9999f, 0.9999f));
            float cosecantAngle = 1f / Sin(angle);
            return (start * Sin((1f - segmentInterpolant) * angle) + end * Sin(segmentInterpolant * angle)) * cosecantAngle;
        }
    }
}
