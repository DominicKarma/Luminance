using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Luminance.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        /// Determines the sign of a number. Does not return zero. If zero is supplied as an input, one is returned.
        /// </summary>
        /// <param name="x">The input number.</param>
        public static int NonZeroSign(this float x) => x >= 0f ? 1 : -1;

        /* Given the aforementioned definitions from the ToRotationVector2 utility, it is evident that the sine and cosine are inherent in determining the unit direction of a
         * vector from an angle, with the following definitions:
         * x = cos(theta)
         * y = sin(theta)
         * 
         * Naturally, the sign of these values will provide the X and Y directions from the given angle. As an example, the following equation can be used to calculate
         * which horizontal direction a given angle points in:
         * 
         * h = sign(cos(theta))
         * 
         * And conversely, this naturally works in the vertical direction as well, as such:
         * 
         * v = sign(sin(theta))
         */
        public static int AngleToXDirection(float angle) => Cos(angle).NonZeroSign();

        public static int AngleToYDirection(float angle) => Sin(angle).NonZeroSign();

        /// <summary>
        /// Converts a -1 or 1 based direction to an equivalent <see cref="SpriteEffects"/> for convenience.
        /// </summary>
        /// <param name="direction">The numerical direction.</param>
        public static SpriteEffects ToSpriteDirection(this int direction) => direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        /// <summary>
        /// Commonly known as a sine bump. Converts 0 to 1 values to a 0 to 1 to 0 again bump.
        /// </summary>
        /// <param name="x">The input number.</param>
        public static float Convert01To010(float x) => Sin(Pi * Saturate(x));

        /// <summary>
        /// Easy shorthand that converts seconds to whole number frames.
        /// </summary>
        /// <param name="seconds">The amount of seconds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SecondsToFrames(float seconds) => (int)Round(seconds * 60f);

        /// <summary>
        /// Easy shorthand that converts minutes to whole number frames.
        /// </summary>
        /// <param name="minutes">The amount of minutes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int MinutesToFrames(float minutes) => (int)Round(minutes * 3600f);

        /// <summary>
        /// Easy shorthand for (sin(x) + 1) / 2, which has the useful property of having a range of 0 to 1 rather than -1 to 1.
        /// </summary>
        /// <param name="x">The input number.</param>
        public static float Sin01(float x) => Sin(x) * 0.5f + 0.5f;

        /// <summary>
        /// Easy shorthand for (cos(x) + 1) / 2, which has the useful property of having a range of 0 to 1 rather than -1 to 1.
        /// </summary>
        /// <param name="x">The input number.</param>
        public static float Cos01(float x) => Cos(x) * 0.5f + 0.5f;

        /// <summary>
        /// Clamps a given number between 0 and 1.
        /// </summary>
        /// <param name="x">The number to clamp.</param>
        public static float Saturate(float x)
        {
            if (x > 1f)
                return 1f;
            if (x < 0f)
                return 0f;
            return x;
        }

        /* This acts as a simple arbitrary range to 0-1 function, with all values outside of the giving range being clamped to 0 or 1.
         * This can be very useful as a way of converting things to a 0-1 percentage value, for calculating interpolants, etc.
         * A common example of such is having an intensity value go from a 0-1 based on an attack timer, like this:
         * 
         * float burnIntensity = InverseLerp(60f, 180f, Time);
         * 
         * In this example, burnIntensity will be 0 if Time is less than or equal 60, 1 if it's greater than or equal to 180, 0.5 if it's 120 (since that's the midway point between the min and max), and
         * everything in-between.
         */
        public static float InverseLerp(float from, float to, float x, bool clamped = true)
        {
            float inverse = (x - from) / (to - from);
            if (!clamped)
                return inverse;

            return Saturate(inverse);
        }

        /// <summary>
        /// This is a useful trick that has been deemed common enough to be made into a utility.<br></br>
        /// It relies on a powerful property of the InverseLerp function, that being that the results are a linear "ramp" that starts at 0 and ends at 1, with all values outside<br></br>
        /// of the specialized range resulting in either zero or one, depending on which threshold was exceeded.Given this, it is possible to create a starting and ending ramp via<br></br>
        /// multiplying two of these InverseLerps together, seeing as past the end point of the first one everything will just be a multiplication by one.<br></br>
        /// This resulting shape is like that of a trapezoid, with each of the two triangles corresponding to each of the two InverseLerp bumps, and any intermediate values between the two bumps<br></br>
        /// resulting simply in one.
        /// </summary>
        /// <param name="start1"></param>
        /// <param name="start2"></param>
        /// <param name="end1"></param>
        /// <param name="end2"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float InverseLerpBump(float start1, float start2, float end1, float end2, float x)
        {
            return InverseLerp(start1, start2, x) * InverseLerp(end2, end1, x);
        }

        /// <summary>
        /// A smooth polynomial curve (4x^3 - 6x^2 + 3x, to be exact) that takes in a 0-1 interpolant and biases it smoothly towards 0.5.
        /// </summary>
        /// <remarks>
        /// The resulting polynomial of 4x^3 - 6x^2 + 3x was arrived at via a lerp(x^3, 1 - (1 - x)^3, x) operation and simplifying results.
        /// </remarks>
        /// <param name="interpolant">The input interpolant.</param>
        public static float SmoothBump(float interpolant)
        {
            float a = interpolant * interpolant * 4f;
            float b = interpolant * -6f;
            return (a + b + 3f) * interpolant;
        }

        /// <summary>
        /// Multiplies x by itself twice.
        /// </summary>
        /// <param name="x">The input number.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Squared(this float x) => x * x;

        /// <summary>
        /// Multiplies x by itself three times.
        /// </summary>
        /// <param name="x">The input number.</param>
        public static float Cubed(this float x) => x * x * x;

        /* Same deal as InverseLerpBump. This acts as common, useful a shorthand for Lerp(a, b, InverseLerp(c, d, t)).
         * It effectively remaps the range of one domain to another.
         */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Remap(float x, float fromMin, float fromMax, float toMin, float toMax)
        {
            return Utils.Remap(x, fromMin, fromMax, toMin, toMax);
        }

        /* Same deal as InverseLerpBump. This acts as common, useful a shorthand for Lerp(a, b, Pow(InverseLerp(c, d, t), exponent)).
         * It effectively remaps the range of one domain to another.
         */
        public static float RemapPow(float x, float exponent, float fromMin, float fromMax, float toMin, float toMax)
        {
            return Lerp(toMin, toMax, Pow(InverseLerp(fromMin, fromMax, x), exponent));
        }
    }
}
