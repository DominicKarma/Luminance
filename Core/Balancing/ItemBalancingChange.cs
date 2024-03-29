using System;
using Terraria;

namespace Luminance.Core.Balancing
{
    public record ItemBalancingChange(int ItemType, BalancePriority Priority, Func<bool> ShouldApply, Action<Item> PerformBalancing);
}
