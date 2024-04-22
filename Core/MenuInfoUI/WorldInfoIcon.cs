using System;
using Terraria.IO;

namespace Luminance.Core.MenuInfoUI
{
    public record WorldInfoIcon(string TexturePath, string HoverText, Func<WorldFileData, bool> ShouldAppear, byte Priority) : IInfoIcon;
}
