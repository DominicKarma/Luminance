using Microsoft.Xna.Framework;
using Terraria;
using static Luminance.Core.Graphics.PrimitiveSettings;

namespace Luminance.Core.Graphics
{
    /// <summary>
    /// Configurable settings for a primitive trail.
    /// </summary>
    /// <param name="WidthFunction">Determines the width of the trail.</param>
    /// <param name="ColorFunction">Determines the color of the trail.</param>
    /// <param name="OffsetFunction">Determines the offset of the trail.</param>
    /// <param name="Smoothen">Whether to smoothen the positions of the trail.</param>
    /// <param name="Pixelate">Whether to pixelate the trail. <b>MUST be used with <see cref="IPixelatedPrimitiveRenderer"/> or <see cref="PrimitivePixelationSystem.RenderToPrimsNextFrame"/></b></param>
    /// <param name="Shader">The shader to use when rendering.</param>
    /// <param name="ProjectionAreaWidth">The width of the projection matrix area. Defaults to <see cref="Main.screenWidth"/>.</param>
    /// <param name="ProjectionAreaHeight">The height of the projection matrix area. Defaults to <see cref="Main.screenHeight"/>.</param>
    /// <param name="UseUnscaledMatrix">Whether to use an unscaled matrix when rendering.</param>
    /// <param name="InitialVertexPositionsOverride">Optional tuple to replace the first vertex positions.</param>
    public record PrimitiveSettings(VertexWidthFunction WidthFunction, VertexColorFunction ColorFunction, VertexOffsetFunction OffsetFunction = null, bool Smoothen = true, bool Pixelate = false,
        ManagedShader Shader = null, int? ProjectionAreaWidth = null, int? ProjectionAreaHeight = null, bool UseUnscaledMatrix = false, (Vector2 Left, Vector2 Right)? InitialVertexPositionsOverride = null) : IPrimitiveSettings
    {
        /// <summary>
        /// A delegate to dynamically determine the width of the trail at each position.
        /// </summary>
        /// <param name="trailLengthInterpolant">The current position along the trail as a 0-1 interpolant value.</param>
        /// <returns>The width for the current point.</returns>
        public delegate float VertexWidthFunction(float trailLengthInterpolant);

        /// <summary>
        /// A delegate to dynamically determine the color of the trail at each position.
        /// </summary>
        /// <param name="trailLengthInterpolant">The current position along the trail as a 0-1 interpolant value.</param>
        /// <returns>The color for the current point.</returns>
        public delegate Color VertexColorFunction(float trailLengthInterpolant);

        /// <summary>
        /// A delegate to dynamically determine the offset of the trail at each position.
        /// </summary>
        /// <param name="trailLengthInterpolant">The current position along the trail as a 0-1 interpolant value.</param>
        /// <returns>The offset for the current point.</returns>
        public delegate Vector2 VertexOffsetFunction(float trailLengthInterpolant);
    }
}
