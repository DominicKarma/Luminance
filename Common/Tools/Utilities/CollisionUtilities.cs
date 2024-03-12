using Microsoft.Xna.Framework;
using Terraria;

namespace KarmaLibrary.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        /// Shifts a point until it reaches level ground.
        /// </summary>
        /// <param name="p">The original point.</param>
        public static Point FindGroundVertical(Point p)
        {
            // The tile is solid. Check up to verify that this tile is not inside of solid ground.
            if (WorldGen.SolidTile(p))
            {
                while (WorldGen.SolidTile(p.X, p.Y - 1) && p.Y >= 1)
                    p.Y--;
            }

            // The tile is not solid. Check down to verify that this tile is not above ground in the middle of the air.
            else
            {
                while (!WorldGen.SolidTile(p.X, p.Y + 1) && p.Y < Main.maxTilesY)
                    p.Y++;
            }

            return p;
        }

        /// <summary>
        /// Shifts a point until it reaches level ground.
        /// </summary>
        /// <param name="p">The original point.</param>
        /// <param name="direction">The direction to search in.</param>
        public static Point FindGround(Point p, Vector2 direction)
        {
            // The tile is solid. Check backward to verify that this tile is not inside of solid ground.
            Vector2 roundedDirection = new(Round(direction.X), Round(direction.Y));
            if (WorldGen.SolidTile(p))
            {
                while (WorldGen.SolidOrSlopedTile(p.X + (int)direction.X, p.Y + (int)direction.Y) && WorldGen.InWorld(p.X, p.Y, 2))
                {
                    p.X -= (int)roundedDirection.X;
                    p.Y -= (int)roundedDirection.Y;
                }
            }

            // The tile is not solid. Check forward to verify that this tile is not above ground in the middle of the air.
            else
            {
                while (!WorldGen.SolidOrSlopedTile(p.X + (int)direction.X, p.Y + (int)direction.Y) && WorldGen.InWorld(p.X, p.Y, 2))
                {
                    p.X += (int)roundedDirection.X;
                    p.Y += (int)roundedDirection.Y;
                }
            }

            return p;
        }
    }
}
