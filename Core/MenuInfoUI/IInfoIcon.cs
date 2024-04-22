namespace Luminance.Core.MenuInfoUI
{
    internal interface IInfoIcon
    {
        string TexturePath { get; init; }

        string HoverText { get; init; }

        byte Priority { get; init; }
    }
}
