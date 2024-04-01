using System;
using System.Collections.Generic;
using System.Linq;

namespace Luminance.Common.Easings
{
    public class PiecewiseCurve
    {
        /// <summary>
        /// A piecewise curve that takes up a part of the domain of a <see cref="PiecewiseCurve"/>, specifying the equivalent range and curvature in said domain.
        /// </summary>
        public record CurveSegment(float StartingHeight, float EndingHeight, float AnimationStart, float AnimationEnd, EasingCurves.Curve Curve, EasingType CurveType);

        /// <summary>
        /// The list of <see cref="CurveSegment"/> that encompass the entire 0-1 domain of this function.
        /// </summary>
        protected List<CurveSegment> segments = [];

        /// <summary>
        /// Inserts a new easing curve into the domain of the overall piecewise curve.
        /// </summary>
        /// <param name="curve">The curve to insert.</param>
        /// <param name="curveType">The type to use when evaluating the curve, such as In, Out, or InOut.</param>
        /// <param name="endingHeight">The ending height of the curve.</param>
        /// <param name="animationEnd">The ending input domain for the newly added curve. Must be greater than 0 and less than or equal to 1.</param>
        /// <param name="startingHeight">An optional starting height for the curve. Defaults to the ending height of the last curve, or 0 if there are no curves yet.</param>
        /// <returns>The original easing curve, for method chaining purposes.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public PiecewiseCurve Add(EasingCurves.Curve curve, EasingType curveType, float endingHeight, float animationEnd, float? startingHeight = null)
        {
            float animationStart = segments.Any() ? segments.Last().AnimationEnd : 0f;
            startingHeight ??= segments.Any() ? segments.Last().EndingHeight : 0f;
            if (animationEnd <= 0f || animationEnd > 1f)
                throw new InvalidOperationException("A piecewise animation curve segment cannot have a domain outside of 0-1.");

            segments.Add(new(startingHeight.Value, endingHeight, animationStart, animationEnd, curve, curveType));

            return this;
        }

        /// <summary>
        /// Evaluates the result of the chained easing curves as a given 0-1 interpolant value.
        /// </summary>
        /// 
        /// <remarks>
        /// The interpolant value is automatically clamped between 0-1 by this method, since the domain of piecewise curves exists solely within those bounds.
        /// </remarks>
        /// 
        /// <param name="interpolant">The interpolant input to evaluate at.</param>
        public float Evaluate(float interpolant)
        {
            interpolant = Saturate(interpolant);

            CurveSegment segmentToUse = segments.Find(s => interpolant >= s.AnimationStart && interpolant <= s.AnimationEnd);
            float curveLocalInterpolant = InverseLerp(segmentToUse.AnimationStart, segmentToUse.AnimationEnd, interpolant);

            return segmentToUse.Curve.Evaluate(segmentToUse.CurveType, segmentToUse.StartingHeight, segmentToUse.EndingHeight, curveLocalInterpolant);
        }
    }
}
