using System;
using Terraria.IO;

namespace Luminance.Core.MenuInfoUI
{
    /// <summary>
    /// Represents an icon that shows up on the world selection UI to provide information about the world's state.
    /// </summary>
    /// <param name="TexturePath">The path to the texture of the icon, including the mod name.</param>
    /// <param name="HoverTextKey">The localization key for the text that should be displayed when this icon is hovered.</param>
    /// <param name="ShouldAppear">Whether this icon should appear for the provided world. Store things in the world header and check them here.</param>
    /// <param name="Priority">The priority of this icon, this determines the ordering of the icon from low to high.</param>
    public record WorldInfoIcon(string TexturePath, string HoverTextKey, Func<WorldFileData, bool> ShouldAppear, byte Priority) : IInfoIcon;
}
