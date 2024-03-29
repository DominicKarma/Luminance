using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Balancing
{
    public record NPCHitContext(int Pierce, int Damage, int? ProjectileIndex, int? ProjectileType, DamageClass ClassType)
    {
        public static NPCHitContext FromProjectile(Projectile proj) => new(proj.penetrate, proj.damage, proj.whoAmI, proj.type, proj.DamageType);
    }
}
