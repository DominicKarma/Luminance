using Terraria;
using Terraria.ModLoader;

namespace Luminance.Common.DataStructures
{
    public interface IProjOwnedByBoss<T> where T : ModNPC
    {
        public bool SetActiveFalseInsteadOfKill => false;

        public static void KillAll()
        {
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.ModProjectile is not IProjOwnedByBoss<T> ownedBy)
                    continue;

                if (ownedBy.SetActiveFalseInsteadOfKill)
                    p.active = false;
                else
                    p.Kill();
            }
        }
    }
}
