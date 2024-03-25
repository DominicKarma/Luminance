using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;

namespace Luminance.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        ///     Returns a color interpolation similar to <see cref="Color.Lerp(Color, Color, float)"/> that supports multiple colors.
        /// </summary>
        /// <param name="interpolant">The 0-1 incremental value used when interpolating.</param>
        /// <param name="colors">The various colors to interpolate across.</param>
        public static Color MulticolorLerp(float interpolant, params Color[] colors)
        {
            interpolant %= 0.999f;

            int gradientStartingIndex = (int)(interpolant * colors.Length);
            float currentColorInterpolant = interpolant * colors.Length % 1f;
            Color gradientSubdivisionA = colors[gradientStartingIndex];
            Color gradientSubdivisionB = colors[(gradientStartingIndex + 1) % colors.Length];
            return Color.Lerp(gradientSubdivisionA, gradientSubdivisionB, currentColorInterpolant);
        }

        /// <summary>
        ///     Returns a color lerp that allows for smooth transitioning between two given colors.
        /// </summary>
        /// <param name="firstColor">The first color you want it to switch between.</param>
        /// <param name="secondColor">The second color you want it to switch between.</param>
        /// <param name="seconds">How long you want it to take to swap between colors. This accounts for dividing by zero errors.</param>
        public static Color ColorSwap(Color firstColor, Color secondColor, float seconds)
        {
            float timeMultiplier = TwoPi / MathHelper.Max(seconds, float.Epsilon);
            return Color.Lerp(firstColor, secondColor, (Sin(timeMultiplier * Main.GlobalTimeWrappedHourly) + 1f) * 0.5f);
        }

        /// <summary>
        ///     Hue shifts a given color by a desired amount. The hue spectrum is within a 0-1 range.
        /// </summary>
        /// <param name="color">The original color.</param>
        /// <param name="hueOffset">The amount to offset the hue by.</param>
        public static Color HueShift(this Color color, float hueOffset)
        {
            Vector3 hsl = Main.rgbToHsl(color);
            hsl.X = (hsl.X + hueOffset).Modulo(1f);

            Color rgb = Main.hslToRgb(hsl) with
            {
                A = color.A
            };
            return rgb;
        }

        /// <summary>
        ///     Flips an origin around a <see cref="Texture2D"/> in accordance with a <see cref="SpriteEffects"/> direction.
        /// </summary>
        /// <param name="texture">The texture to flip based on.</param>
        /// <param name="origin">The unmodified origin.</param>
        /// <param name="flipDirection">The direction to use as a basis for flipping.</param>
        public static Vector2 FlipOriginByDirection(Vector2 origin, Texture2D texture, SpriteEffects flipDirection)
        {
            if (flipDirection.HasFlag(SpriteEffects.FlipHorizontally))
                origin.X = texture.Width - origin.X;
            if (flipDirection.HasFlag(SpriteEffects.FlipVertically))
                origin.Y = texture.Height - origin.Y;

            return origin;
        }

        /// <summary>
        ///     Flips an origin around a <see cref="Rectangle"/> frame in accordance with a <see cref="SpriteEffects"/> direction.
        /// </summary>
        /// <param name="frame">The frame to flip based on.</param>
        /// <param name="origin">The unmodified origin.</param>
        /// <param name="flipDirection">The direction to use as a basis for flipping.</param>
        public static Vector2 FlipOriginByDirection(Vector2 origin, Rectangle frame, SpriteEffects flipDirection)
        {
            if (flipDirection.HasFlag(SpriteEffects.FlipHorizontally))
                origin.X = frame.Width - origin.X;
            if (flipDirection.HasFlag(SpriteEffects.FlipVertically))
                origin.Y = frame.Height - origin.Y;

            return origin;
        }

        /// <summary>
        ///     Generates an arbitrary quantity of evenly spaced laser point positions for a projectile. Commonly used when calculating points for primitive-based laser beams.
        /// </summary>
        /// <param name="projectile">The projectile to calculate positions from.</param>
        /// <param name="samplesCount">The amount of subdivisions that should be performed. Larger values are more precise, but also more computationally expensive to use.</param>
        /// <param name="laserLength">The length of the laser. Used for determining the end point of the laser.</param>
        /// <param name="laserDirection">The direction of the laser. By default uses the unit direction of the projectile's velocity.</param>
        public static List<Vector2> GetLaserControlPoints(this Projectile projectile, int samplesCount, float laserLength, Vector2? laserDirection = null)
        {
            Vector2 laserStart = projectile.Center;
            Vector2 laserEnd = laserStart + (laserDirection ?? projectile.velocity.SafeNormalize(Vector2.Zero)) * laserLength;

            List<Vector2> controlPoints = [];
            for (int i = 0; i < samplesCount; i++)
                controlPoints.Add(Vector2.Lerp(laserStart, laserEnd, i / (float)(samplesCount - 1f)));

            return controlPoints;
        }
    }
}
