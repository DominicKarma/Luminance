using Terraria;
using static Luminance.Core.Graphics.PrimitiveSettings;

namespace Luminance.Core.Graphics
{
    /// <summary>
    /// Configurable settings for a primitive circle edge.
    /// </summary>
    /// <param name="EdgeWidthFunction">Determines the width of the circle edge.</param>
    /// <param name="ColorFunction">Determines the color of the circle edge.</param>
    /// <param name="RadiusFunction">Determines the radius of the circle edge.</param>
    /// <param name="Pixelate">Whether to pixelate the circle edge. <b>MUST be used with <see cref="IPixelatedPrimitiveRenderer"/> or <see cref="PrimitivePixelationSystem.RenderToPrimsNextFrame"/></b></param>
    /// <param name="Shader">The shader to use when rendering.</param>
    /// <param name="ProjectionAreaWidth">The width of the projection matrix area. Defaults to <see cref="Main.screenWidth"/>.</param>
    /// <param name="ProjectionAreaHeight">The height of the projection matrix area. Defaults to <see cref="Main.screenHeight"/>.</param>
    /// <param name="UseUnscaledMatrix">Whether to use an unscaled matrix when rendering.</param>
    public record PrimitiveSettingsCircleEdge(VertexWidthFunction EdgeWidthFunction, VertexColorFunction ColorFunction, VertexWidthFunction RadiusFunction, bool Pixelate = false,
        ManagedShader Shader = null, int? ProjectionAreaWidth = null, int? ProjectionAreaHeight = null, bool UseUnscaledMatrix = false) : IPrimitiveSettings;
}
