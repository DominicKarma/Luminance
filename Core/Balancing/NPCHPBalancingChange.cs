using System;

namespace Luminance.Core.Balancing
{
    public record NPCHPBalancingChange(int NPCType, int HP, BalancePriority Priority, Func<bool> ShouldApply);
}
