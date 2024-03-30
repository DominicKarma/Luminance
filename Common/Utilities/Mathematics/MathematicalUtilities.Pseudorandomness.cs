using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.RGB;

namespace Luminance.Common.Utilities
{
    public static partial class Utilities
    {
        // When two periodic functions are summed, the resulting function is periodic if the ratio of the b/a is rational, given periodic functions f and g:
        // f(a * x) + g(b * x). However, if the ratio is irrational, then the result has no period.
        // This is desirable for somewhat random wavy fluctuations.
        // In this case, pi and e used, which are indeed irrational numbers.
        /// <summary>
        ///     Calculates an aperiodic sine. This function only achieves this if <paramref name="a"/> and <paramref name="b"/> are irrational numbers.
        /// </summary>
        /// <param name="x">The input value.</param>
        /// <param name="dx">An optional, secondary value that works similarly to x. Unlike x, however, it serves as an input offset that is unaffected by the two coefficients.</param>
        /// <param name="a">The first irrational coefficient.</param>
        /// <param name="b">The second irrational coefficient.</param>
        public static float AperiodicSin(float x, float dx = 0f, float a = Pi, float b = MathHelper.E)
        {
            return (Sin(x * a + dx) + Sin(x * b + dx)) * 0.5f;
        }

        /// <summary>
        ///     Applies 2D FBM, an iterative process commonly use with things like Perlin noise to give a natural, "crisp" aesthetic to noise, rather than a blobby one.
        ///     <br></br>
        ///     The greater the amount of octaves, the more pronounced this effect is, but the more performance intensive it is.
        /// </summary>
        /// <param name="x">The X position to sample from.</param>
        /// <param name="y">The Y position to sample from.</param>
        /// <param name="seed">The RNG seed for the underlying noise calculations.</param>
        /// <param name="octaves">The amount of octaves. The greater than is, the more crisp the results are.</param>
        /// <param name="gain">The exponential factor between each iteration. Iterations have an intensity of g^n, where g is the gain and n is the iteration number.</param>
        /// <param name="lacunarity">The degree of self-similarity of the noise.</param>
        public static float FractalBrownianMotion(float x, float y, int seed, int octaves, float gain = 0.5f, float lacunarity = 2f)
        {
            float result = 0f;
            float frequency = 1f;
            float amplitude = 0.5f;

            x += seed * 0.00489937f % 10f;

            for (int i = 0; i < octaves; i++)
            {
                float noise = NoiseHelper.GetStaticNoise(new Vector2(x, y) * frequency) * 2f - 1f;

                result += noise * amplitude;
                amplitude *= gain;
                frequency *= lacunarity;
            }
            return result;
        }

        internal static readonly List<Vector2> Directions =
        [
            new(-1f, -1f),
            new(1f, -1f),
            new(-1f, 1f),
            new(1f, 1f),
            new(0f, -1f),
            new(-1f, 0f),
            new(0f, 1f),
            new(1f, 0f),
        ];

        /// <summary>
        /// Computes 2-dimensional Perlin Noise, which gives "random" but continuous values.
        /// </summary>
        /// <param name="x">The X position on the map.</param>
        /// <param name="y">The Y position on the map.</param>
        /// <param name="octaves">A metric of "instability" of the noise. The higher this is, the more unstable. Lower of bounds of 2-3 are preferable.</param>
        /// <param name="seed">The seed for the noise.</param>
        public static float PerlinNoise2D(float x, float y, int octaves, int seed)
        {
            float SmoothFunction(float n) => 3f * n * n - 2f * n * n * n;
            float NoiseGradient(int s, int noiseX, int noiseY, float xd, float yd)
            {
                int hash = s;
                hash ^= 1619 * noiseX;
                hash ^= 31337 * noiseY;

                hash = hash * hash * hash * 60493;
                hash = (hash >> 13) ^ hash;

                Vector2 g = Directions[hash & 7];

                return xd * g.X + yd * g.Y;
            }

            int frequency = (int)Math.Pow(2D, octaves);
            x *= frequency;
            y *= frequency;

            int flooredX = (int)x;
            int flooredY = (int)y;
            int ceilingX = flooredX + 1;
            int ceilingY = flooredY + 1;
            float interpolatedX = x - flooredX;
            float interpolatedY = y - flooredY;
            float interpolatedX2 = interpolatedX - 1;
            float interpolatedY2 = interpolatedY - 1;

            float fadeX = SmoothFunction(interpolatedX);
            float fadeY = SmoothFunction(interpolatedY);

            float smoothX = Lerp(NoiseGradient(seed, flooredX, flooredY, interpolatedX, interpolatedY), NoiseGradient(seed, ceilingX, flooredY, interpolatedX2, interpolatedY), fadeX);
            float smoothY = Lerp(NoiseGradient(seed, flooredX, ceilingY, interpolatedX, interpolatedY2), NoiseGradient(seed, ceilingX, ceilingY, interpolatedX2, interpolatedY2), fadeX);
            return Lerp(smoothX, smoothY, fadeY);
        }
    }
}
