using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public class ScreenShakeSystem : ModSystem
    {
        /// <summary>
        /// Represents a screenshake instance.
        /// </summary>
        public class ShakeInfo
        {
            /// <summary>
            /// This indicates the maximum amount BaseDirection can be rotated by when a shake occurs. At sufficiently high values there is effectively no shake direction and it's equivalent to NextVector2Circular.
            /// </summary>
            public float AngularVariance;

            /// <summary>
            /// This indicate how much, in pixels, the screen should shake.
            /// </summary>
            public float ShakeStrength;

            /// <summary>
            /// This indicates the general direction the shake should occur in.
            /// </summary>
            public Vector2 BaseDirection;

            /// <summary>
            /// This indicates how much the shake should dissipate every frame.
            /// </summary>
            public float ShakeStrengthDissipationIncrement;

            /// <summary>
            /// A default-object ShakeInfo that does nothing.
            /// </summary>
            public static ShakeInfo None => new();

            internal void Apply()
            {
                float shakeOffset = ShakeStrength;
                Main.screenPosition += BaseDirection.RotatedByRandom(AngularVariance) * shakeOffset * (ModContent.GetInstance<Config>().ScreenshakeModifier * 0.01f);
            }
        }

        private static ShakeInfo universalRumble = ShakeInfo.None;

        private static readonly List<ShakeInfo> shakes = [];

        /// <summary>
        /// The overall intensity of every shake currently active.
        /// </summary>
        public static float OverallShakeIntensity => shakes.Sum(s => s.ShakeStrength);

        /// <summary>
        /// Creates a general screenshake.
        /// </summary>
        /// <param name="strength">The strength of the screenshake.</param>
        /// <param name="angularVariance">The size of the angle to randomly offset the screenshake direction by.</param>
        /// <param name="shakeDirection">The direction of the screenshake. Is <see cref="Vector2.Zero"/> by default.</param>
        /// <param name="shakeStrengthDissipationIncrement">The amount to decrease the screenshake strength by each frame.</param>
        public static ShakeInfo StartShake(float strength, float angularVariance = TwoPi, Vector2? shakeDirection = null, float shakeStrengthDissipationIncrement = 0.2f)
        {
            if (Main.dedServ)
                return ShakeInfo.None;

            ShakeInfo shake = new()
            {
                ShakeStrength = strength,
                AngularVariance = angularVariance,
                BaseDirection = (shakeDirection ?? Vector2.Zero).SafeNormalize(Vector2.UnitX),
                ShakeStrengthDissipationIncrement = MathF.Max(0.01f, shakeStrengthDissipationIncrement)
            };

            if (shake != ShakeInfo.None)
                shakes.Add(shake);
            return shake;
        }

        /// <summary>
        /// Creates a screenshake at a specific point.
        /// </summary>
        /// <param name="shakeCenter">The position of the screenshake.</param>
        /// <param name="strength">The strength of the screenshake.</param>
        /// <param name="angularVariance">The size of the angle to randomly offset the screenshake direction by.</param>
        /// <param name="shakeDirection">The direction of the screenshake. Is <see cref="Vector2.Zero"/> by default.</param>
        /// <param name="shakeStrengthDissipationIncrement">The amount to decrease the screenshake strength by each frame.</param>
        /// <param name="intensityTaperEndDistance">The distance where beyond this, the player should not be affected by the screenshake.</param>
        /// <param name="intensityTaperStartDistance">The starting distance for the screenshake intensity to fall off for the player.</param>
        public static ShakeInfo StartShakeAtPoint(Vector2 shakeCenter, float strength, float angularVariance = TwoPi, Vector2? shakeDirection = null, float shakeStrengthDissipationIncrement = 0.2f, float intensityTaperEndDistance = 2300f, float intensityTaperStartDistance = 1476f)
        {
            if (Main.dedServ)
                return ShakeInfo.None;

            // Calculate the shake strength based on how far away the player is from the shake center.
            float distanceToShakeCenter = Main.LocalPlayer.Distance(shakeCenter);
            float desiredScreenShakeStrength = InverseLerp(intensityTaperEndDistance, intensityTaperStartDistance, distanceToShakeCenter) * strength;

            // Start the shake with the distance taper in place.
            return StartShake(desiredScreenShakeStrength, angularVariance, shakeDirection, shakeStrengthDissipationIncrement);
        }

        /// <summary>
        /// Sets the universal screenshake. Only one of these exists at a time, and setting another while one is active will override the existing one.
        /// </summary>
        /// <param name="strength">The strength of the screenshake.</param>
        /// <param name="angularVariance">The size of the angle to randomly offset the screenshake direction by.</param>
        /// <param name="shakeDirection">The direction of the screenshake. Is <see cref="Vector2.Zero"/> by default.</param>
        /// <param name="shakeStrengthDissipationIncrement">The amount to decrease the screenshake strength by each frame.</param>
        public static ShakeInfo SetUniversalRumble(float strength, float angularVariance = TwoPi, Vector2? shakeDirection = null, float shakeStrengthDissipationIncrement = 0.2f)
        {
            if (Main.dedServ)
                return ShakeInfo.None;

            universalRumble = new()
            {
                ShakeStrength = strength,
                AngularVariance = angularVariance,
                BaseDirection = (shakeDirection ?? Vector2.Zero).SafeNormalize(Vector2.UnitX),
                ShakeStrengthDissipationIncrement = MathF.Max(0.01f, shakeStrengthDissipationIncrement)
            };
            return universalRumble;
        }

        [Obsolete("This is obsolete, use 'SetUniversalRumble(float strength, float angularVariance = TwoPi, Vector2? shakeDirection = null, float shakeStrengthDissipationIncrement = 0.2f)' instead.", false)]
        public static void SetUniversalRumble(float strength, float angularVariance = TwoPi, Vector2? shakeDirection = null)
            => SetUniversalRumble(strength, angularVariance, shakeDirection, 0.2f);

        public override void ModifyScreenPosition()
        {
            // Clear all shakes that are no longer in use.
            shakes.RemoveAll(s => s.ShakeStrength <= 0f);

            // Update the screen position based on shake intensities.
            foreach (ShakeInfo shake in shakes)
            {
                shake.Apply();

                // Make the shake dissipate in intensity.
                shake.ShakeStrength = Clamp(shake.ShakeStrength - shake.ShakeStrengthDissipationIncrement, 0f, 50f);
            }

            // Apply the universal rumble if necessary.
            if (universalRumble != ShakeInfo.None && OverallShakeIntensity < universalRumble.ShakeStrength)
            {
                universalRumble.Apply();
                shakes.Clear();
                universalRumble.ShakeStrength = Clamp(universalRumble.ShakeStrength - universalRumble.ShakeStrengthDissipationIncrement, 0f, 50f);

                if (universalRumble.ShakeStrength <= 0f)
                    universalRumble = ShakeInfo.None;
            }
        }
    }
}
