using static Luminance.Core.Graphics.PrimitiveSettings;

namespace Luminance.Core.Graphics
{
    public record PrimitiveSettingsCircle(VertexWidthFunction RadiusFunction, VertexColorFunction ColorFunction, bool Pixelate = false,
        ManagedShader Shader = null, int? ProjectionAreaWidth = null, int? ProjectionAreaHeight = null, bool UseUnscaledMatrix = false) : IPrimitiveSettings;
}
