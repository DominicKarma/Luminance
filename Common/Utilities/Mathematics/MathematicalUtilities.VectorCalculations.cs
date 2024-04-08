using Microsoft.Xna.Framework;
using Terraria;

namespace Luminance.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        ///     Calculates the elbow position of a two-limbed IK system via trigonometry.
        /// </summary>
        /// <param name="start">The start of the IK system.</param>
        /// <param name="end">The end effector position of the IK system.</param>
        /// <param name="armLength">The length of the first limb.</param>
        /// <param name="forearmLength">The length of the second limb.</param>
        /// <param name="flip">Whether the angles need to be flipped.</param>
        public static Vector2 CalculateElbowPosition(Vector2 start, Vector2 end, float armLength, float forearmLength, bool flip)
        {
            float c = Vector2.Distance(start, end);
            float angle = Acos(Clamp((c * c + armLength * armLength - forearmLength * forearmLength) / (c * armLength * 2f), -1f, 1f)) * flip.ToDirectionInt();
            return start + (angle + start.AngleTo(end)).ToRotationVector2() * armLength;
        }

        /// <summary>
        ///     Clamps the length of a vector.
        /// </summary>
        /// <param name="v">The vector to clamp the length of.</param>
        /// <param name="min">The minimum vector length.</param>
        /// <param name="max">The maximum vector length.</param>
        public static Vector2 ClampLength(this Vector2 v, float min, float max)
        {
            return v.SafeNormalize(Vector2.UnitY) * Clamp(v.Length(), min, max);
        }

        /// <summary>
        ///     Interpolates between three <see cref="Vector2"/>-based points via a quadratic Bezier spline.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <param name="interpolant">The interpolant to sample points by.</param>
        public static Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float interpolant)
        {
            Vector2 firstTerm = (1f - interpolant).Squared() * a;
            Vector2 secondTerm = (2f - interpolant * 2f) * interpolant * b;
            Vector2 thirdTerm = interpolant.Squared() * c;

            return firstTerm + secondTerm + thirdTerm;
        }

        /// <summary>
        ///     Calculates the signed distance of a point from a given line. This is relative to how far it is perpendicular to said line.
        /// </summary>
        /// <param name="evaluationPoint">The point to check.</param>
        /// <param name="linePoint">The pivot point upon which the line rotates.</param>
        /// <param name="lineDirection">The direction of the line.</param>
        public static float SignedDistanceToLine(Vector2 evaluationPoint, Vector2 linePoint, Vector2 lineDirection)
        {
            return Vector2.Dot(lineDirection, evaluationPoint - linePoint);
        }

        /// <summary>
        ///     Rotates a vector's direction towards an ideal angle at a specific incremental rate.
        /// </summary>
        /// <param name="originalVector">The original vector to rotated from.</param>
        /// <param name="idealAngle">The ideal direction to approach.</param>
        /// <param name="angleIncrement">The maximum angular increment to make in the pursuit of approaching the destination.</param>
        public static Vector2 RotateTowards(this Vector2 originalVector, float idealAngle, float angleIncrement)
        {
            float initialAngle = originalVector.ToRotation();
            float newAngle = initialAngle.AngleTowards(idealAngle, angleIncrement);
            Vector2 newDirection = newAngle.ToRotationVector2();
            return newDirection * originalVector.Length();
        }
    }
}
