namespace Luminance.Common.Easings
{
    // Refer to https://easings.net/ for most of the equations in the curve definitions.
    public static class EasingCurves
    {
        /// <summary>
        /// An elastic easing curves. Characterized chiefly by small bumps at extreme points.
        /// </summary>
        public static readonly Curve Elastic = new(interpolant =>
        {
            float sineFactor = TwoPi / 3f;
            float exponentialTerm = -Pow(2f, interpolant * 10f - 10f);
            float sinusoidalTerm = Sin((interpolant * 10f - 10.75f) * sineFactor);
            return exponentialTerm * sinusoidalTerm;
        }, interpolant =>
        {
            float sineFactor = TwoPi / 3f;
            float exponentialTerm = Pow(2f, interpolant * -10f);
            float sinusoidalTerm = Sin((interpolant * 10f - 0.75f) * sineFactor);
            return exponentialTerm * sinusoidalTerm + 1f;
        }, interpolant =>
        {
            float sineFactor = TwoPi / 4.5f;
            float sinusoidalTerm = Sin((interpolant * 20f - 11.125f) * sineFactor) * 0.5f;
            if (interpolant < 0.5f)
            {
                float exponentialTerm = -Pow(2f, interpolant * 20f - 10f);
                return exponentialTerm * sinusoidalTerm;
            }
            else
            {
                float exponentialTerm = Pow(2f, interpolant * -20f + 10f);
                return exponentialTerm * sinusoidalTerm + 1f;
            }
        });

        /// <summary>
        /// A linear easing curve.
        /// </summary>
        public static readonly Curve Linear = new(interpolant => interpolant, interpolant => interpolant, interpolant => interpolant);

        /// <summary>
        /// A sine easing curve.
        /// </summary>
        public static readonly Curve Sine = new(interpolant => 1f - Cos(interpolant * Pi / 2f), interpolant => Sin(interpolant * Pi / 2f), interpolant => -(Cos(interpolant * Pi) - 1f) / 2f);

        /// <summary>
        /// An exponential easing curve.
        /// </summary>
        public static readonly Curve Exp = new(interpolant => interpolant == 0f ? 0f : Pow(2f, 10f * interpolant - 10f),
            interpolant => interpolant == 1f ? 1f : 1f - Pow(2f, -10f * interpolant),
            interpolant => interpolant == 0f ? 0f : interpolant == 1f ? 1f : interpolant < 0.5f ? Pow(2f, 20f * interpolant - 10f) / 2f : (2f - Pow(2f, -20f * interpolant - 10f)) / 2f);

        /// <summary>
        /// A circular easing curve.
        /// </summary>
        public static readonly Curve Circ = new(interpolant => 1f - Sqrt(1f - Pow(interpolant, 2f)),
            interpolant => Sqrt(1f - Pow(interpolant - 1f, 2f)),
            interpolant => interpolant < 0.5 ? 1f - Sqrt(1f - Pow(2f * interpolant, 2f)) / 2f : (Sqrt(1f - Pow(-2f * interpolant - 2f, 2f)) + 1f) / 2f);

        /// <summary>
        /// A polynomial easing curve of degree 2.
        /// </summary>
        public static readonly Curve Quadratic = MakePoly(2f);

        /// <summary>
        /// A polynomial easing curve of degree 3.
        /// </summary>
        public static readonly Curve Cubic = MakePoly(3f);

        /// <summary>
        /// A polynomial easing curve of degree 4.
        /// </summary>
        public static readonly Curve Quartic = MakePoly(4f);

        /// <summary>
        /// A polynomial easing curve of degree 5.
        /// </summary>
        public static readonly Curve Quintic = MakePoly(5f);

        /// <summary>
        /// A polynomial easing curve of degree 6.
        /// </summary>
        public static readonly Curve Sextic = MakePoly(6f);

        public delegate float InterpolationFunction(float interpolant);

        public record Curve(InterpolationFunction InFunction, InterpolationFunction OutFunction, InterpolationFunction InOutFunction);

        /// <summary>
        /// Creates a polynomial easing curves with an arbitrary exponent/potentially-non-integer degree.
        /// </summary>
        /// <param name="exponent">The exponent of the polynomial curve.</param>
        public static Curve MakePoly(float exponent)
        {
            return new(interpolant =>
            {
                return Pow(interpolant, exponent);
            }, interpolant =>
            {
                return 1f - Pow(1f - interpolant, exponent);
            }, interpolant =>
            {
                if (interpolant < 0.5f)
                    return Pow(2f, exponent - 1f) * Pow(interpolant, exponent);
                return 1f - Pow(interpolant * -2f + 2f, exponent) * 0.5f;
            });
        }

        public static float Evaluate(this Curve curve, EasingType easingType, float interpolant) => Evaluate(curve, easingType, 0f, 1f, interpolant);

        public static float Evaluate(this Curve curve, EasingType easingType, float start, float end, float interpolant)
        {
            interpolant = Saturate(interpolant);

            float easedInterpolant = easingType switch
            {
                EasingType.In => curve.InFunction(interpolant),
                EasingType.Out => curve.OutFunction(interpolant),
                EasingType.InOut => curve.InOutFunction(interpolant),
                _ => start,
            };

            return Lerp(start, end, easedInterpolant);
        }
    }
}
