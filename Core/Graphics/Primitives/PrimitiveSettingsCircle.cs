using static Luminance.Core.Graphics.PrimitiveSettings;

namespace Luminance.Core.Graphics
{
    public record PrimitiveSettingsCircle(VertexWidthFunction WidthFunction, VertexColorFunction ColorFunction, bool Pixelate,
        ManagedShader Shader = null, int? ProjectionAreaWidth = null, int? ProjectionAreaHeight = null, bool UseUnscaledMatrix = false) : IPrimitiveSettings;
}
