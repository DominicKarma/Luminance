using Terraria;

namespace Luminance.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        ///     Gives a given <see cref="Player"/> infinite flight.
        /// </summary>
        /// <param name="p">The player to apply infinite flight to.</param>
        public static void GrantInfiniteFlight(this Player p)
        {
            p.wingTime = p.wingTimeMax;
        }

        /// <summary>
        ///     Gets the current mouse item for a given <see cref="Player"/>. This supports <see cref="Main.mouseItem"/> (the item held by the cursor) and <see cref="Player.HeldItem"/> (the item in use with the hotbar).
        /// </summary>
        /// <param name="player">The player to retrieve the mouse item for.</param>
        public static Item HeldMouseItem(this Player player)
        {
            if (!Main.mouseItem.IsAir)
                return Main.mouseItem;

            return player.HeldItem;
        }
    }
}
