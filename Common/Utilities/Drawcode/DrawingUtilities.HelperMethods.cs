using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;

namespace Luminance.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        ///     Displays arbitrary text in the game chat with a desired color. This method expects to be called server-side in multiplayer, with the message display packet being sent to all clients from there.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="color">The color of the text.</param>
        public static void BroadcastText(string text, Color color)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                Main.NewText(text, color);
            else if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), color);
        }

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
