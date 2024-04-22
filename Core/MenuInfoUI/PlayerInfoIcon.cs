using System;
using Terraria;

namespace Luminance.Core.MenuInfoUI
{
    public record PlayerInfoIcon(string TexturePath, string HoverText, Func<Player, bool> ShouldAppear, byte Priority) : IInfoIcon;
}
