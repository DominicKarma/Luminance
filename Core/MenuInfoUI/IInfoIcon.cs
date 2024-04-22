namespace Luminance.Core.MenuInfoUI
{
    internal interface IInfoIcon
    {
        string TexturePath { get; init; }

        string HoverTextKey { get; init; }

        byte Priority { get; init; }
    }
}
