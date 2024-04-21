using static Luminance.Core.Graphics.PrimitiveSettings;

namespace Luminance.Core.Graphics
{
    public record PrimitiveSettingsCircleEdge(VertexWidthFunction EdgeWidthFunction, VertexColorFunction ColorFunction, VertexWidthFunction RadiusFunction, bool Pixelate = false,
        ManagedShader Shader = null, int? ProjectionAreaWidth = null, int? ProjectionAreaHeight = null, bool UseUnscaledMatrix = false) : IPrimitiveSettings;
}
