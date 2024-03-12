using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics.SpecificEffectManagers
{
    [Autoload(Side = ModSide.Client)]
    public class CameraPanSystem : ModSystem
    {
        /// <summary>
        ///     The position the camera should focus on.
        /// </summary>
        internal static Vector2 CameraFocusPoint
        {
            get;
            set;
        }

        /// <summary>
        ///     The 0-1 interpolant that dictates how much the camera position should move.
        /// </summary>
        internal static float CameraPanInterpolant
        {
            get;
            set;
        }

        /// <summary>
        ///     How much the camera should zoom in. Accepts negative values up -1 for zoom-out effects.
        /// </summary>
        internal static float Zoom
        {
            get;
            set;
        }

        /// <summary>
        ///     Causes the camera to pan towards a given point, with a given 0-1 interpolant.
        /// </summary>
        /// <param name="panDestination">The point at which the camera should focus on.</param>
        /// <param name="panInterpolant">How much the screen position should pan towards the destination.</param>
        public static void PanTowards(Vector2 panDestination, float panInterpolant)
        {
            CameraFocusPoint = panDestination;
            CameraPanInterpolant = panInterpolant;
        }

        /// <summary>
        ///     Zooms the camera in by a given factor. Does not work with negative values.
        /// </summary>
        /// <remarks>
        ///     A value of 0 means no zoom-in, a value of 1 means 2x the zoom in, a value of 2 means 3x, and so on.
        /// </remarks>
        /// <param name="zoom">The amount to zoom in by.</param>
        public static void ZoomIn(float zoom)
        {
            if (zoom < 0f)
                zoom = 0f;

            Zoom = zoom;
        }

        /// <summary>
        ///     Zooms the camera out by a given factor. Does not work with negative values.
        /// </summary>
        /// <remarks>
        ///     A value of 0 means no zoom-in, a value of 1 means 0.5x the zoom, a value of 2 means 0.333x, and so on.
        /// </remarks>
        /// <param name="zoom">The amount to zoom out by.</param>
        public static void ZoomOut(float zoom)
        {
            if (zoom < 0f)
                zoom = 0f;

            Zoom = 1f / (zoom + 1f) - 1f;
        }

        public override void ModifyScreenPosition()
        {
            if (Main.LocalPlayer.dead && !Main.gamePaused)
                return;

            // Handle camera focus effects.
            if (CameraPanInterpolant > 0f)
            {
                Vector2 idealScreenPosition = CameraFocusPoint - Main.ScreenSize.ToVector2() * 0.5f;
                Main.screenPosition = Vector2.Lerp(Main.screenPosition, idealScreenPosition, CameraPanInterpolant);
            }
        }

        public override void PreUpdateEntities()
        {
            if (Main.LocalPlayer.dead && !Main.gamePaused)
            {
                Zoom = Lerp(Zoom, 0f, 0.13f);
                CameraPanInterpolant = 0f;
                return;
            }

            // Make interpolants gradually return to their original values.
            if (!Main.gamePaused)
            {
                CameraPanInterpolant = Saturate(CameraPanInterpolant - 0.06f);
                Zoom = Lerp(Zoom, 0f, 0.09f);
            }
        }

        public override void ModifyTransformMatrix(ref SpriteViewMatrix transform)
        {
            transform.Zoom *= 1f + Zoom;
        }
    }
}
