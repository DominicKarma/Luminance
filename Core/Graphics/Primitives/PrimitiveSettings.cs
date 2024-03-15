using Microsoft.Xna.Framework;
using static Luminance.Core.Graphics.PrimitiveSettings;

namespace Luminance.Core.Graphics
{
    public record PrimitiveSettings(VertexWidthFunction WidthFunction, VertexColorFunction ColorFunction, VertexOffsetFunction OffsetFunction = null, bool Smoothen = true, bool Pixelate = false, ManagedShader Shader = null, (Vector2 Left, Vector2 Right)? InitialVertexPositionsOverride = null)
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
