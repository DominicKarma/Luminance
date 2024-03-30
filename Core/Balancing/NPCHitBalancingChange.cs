namespace Luminance.Core.Balancing
{
    public record NPCHitBalancingChange(int NPCType, params INPCHitBalancingRule[] BalancingRules);
}
