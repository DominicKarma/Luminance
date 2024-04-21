namespace Luminance.Core.Graphics
{
    internal interface IPrimitiveSettings
    {
        bool Pixelate { get; init; }

        ManagedShader Shader { get; init; }

        int? ProjectionAreaWidth { get; init; }

        int? ProjectionAreaHeight { get; init; }

        bool UseUnscaledMatrix { get; init; }
    }
}
