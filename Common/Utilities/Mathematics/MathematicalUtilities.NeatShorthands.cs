using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Luminance.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        ///     Determines the sign of a number. Does not return zero. If zero is supplied as an input, one is returned.
        /// </summary>
        /// <param name="x">The input number.</param>
        public static int NonZeroSign(this float x) => x >= 0f ? 1 : -1;

        /// <summary>
        ///     Converts a -1 or 1 based direction to an equivalent <see cref="SpriteEffects"/> for convenience.
        /// </summary>
        /// <param name="direction">The numerical direction.</param>
        public static SpriteEffects ToSpriteDirection(this int direction) => direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        /// <summary>
        ///     Commonly known as a sine bump. Converts 0 to 1 values to a 0 to 1 to 0 again bump.
        /// </summary>
        /// <param name="x">The input number.</param>
        public static float Convert01To010(float x) => Sin(Pi * Saturate(x));

        /// <summary>
        ///     Easy shorthand that converts seconds to whole number frames.
        /// </summary>
        /// <param name="seconds">The amount of seconds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SecondsToFrames(float seconds) => (int)Round(seconds * 60f);

        /// <summary>
        ///     Easy shorthand that converts minutes to whole number frames.
        /// </summary>
        /// <param name="minutes">The amount of minutes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int MinutesToFrames(float minutes) => (int)Round(minutes * 3600f);

        /// <summary>
        ///     Easy shorthand for (sin(x) + 1) / 2, which has the useful property of having a range of 0 to 1 rather than -1 to 1.
        /// </summary>
        /// <param name="x">The input number.</param>
        public static float Sin01(float x) => Sin(x) * 0.5f + 0.5f;

        /// <summary>
        ///     Easy shorthand for (cos(x) + 1) / 2, which has the useful property of having a range of 0 to 1 rather than -1 to 1.
        /// </summary>
        /// <param name="x">The input number.</param>
        public static float Cos01(float x) => Cos(x) * 0.5f + 0.5f;

        /// <summary>
        ///     Clamps a given number between 0 and 1.
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

        /// <summary>
        ///     A shorthand for <see cref="Utils.GetLerpValue(float, float, float, bool)"/> with <paramref name="clamped"/> defaulting to true.
        /// </summary>
        /// <param name="from">The value to interpolate from.</param>
        /// <param name="to">The value to interpolate to.</param>
        /// <param name="x">The value to interpolate in accordance with.</param>
        /// <param name="clamped">Whether outputs should be clamped between 0 and 1.</param>
        public static float InverseLerp(float from, float to, float x, bool clamped = true)
        {
            float interpolant = (x - from) / (to - from);
            if (!clamped)
                return interpolant;

            return Saturate(interpolant);
        }

        /// <summary>
        ///     Performs a linear bump across a spectrum of two in/out values.
        /// </summary>
        /// <param name="start1">The value at which the output should rise from 0 to 1.</param>
        /// <param name="start2">The value at which the output start bumping at 1.</param>
        /// <param name="end1">The value at which the output cease bumping at 1.</param>
        /// <param name="end2">The value at which the output should descent from 1 to 0.</param>
        /// <param name="x">The input interpolant.</param>
        /// <returns>
        ///     0 when <paramref name="x"/> is less than or equal to <paramref name="start1"/>.
        ///     <br></br>
        ///     Anywhere between 0 and 1, ascending, when <paramref name="x"/> is greater than <paramref name="start1"/> but less than <paramref name="start2"/>.
        ///     <br></br>
        ///     1 when <paramref name="x"/> is between <paramref name="start2"/> and <paramref name="end1"/>.
        ///     <br></br>
        ///     Anywhere between 0 and 1, descending, when <paramref name="x"/> is greater than <paramref name="end1"/> but less than <paramref name="end2"/>.
        ///     <br></br>
        ///     1 when <paramref name="x"/> is greater than or equal to <paramref name="end2"/>.
        /// </returns>
        public static float InverseLerpBump(float start1, float start2, float end1, float end2, float x)
        {
            return InverseLerp(start1, start2, x) * InverseLerp(end2, end1, x);
        }

        /// <summary>
        ///     A smooth polynomial curve (4x^3 - 6x^2 + 3x, to be exact) that takes in a 0-1 interpolant and biases it smoothly towards 0.5.
        /// </summary>
        /// <remarks>
        ///     The resulting polynomial of 4x^3 - 6x^2 + 3x was arrived at via a lerp(x^3, 1 - (1 - x)^3, x) operation and simplifying results.
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
    }
}
