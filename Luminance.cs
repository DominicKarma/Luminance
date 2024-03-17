global using static System.MathF;
global using static Luminance.Assets.MiscTexturesRegistry;
global using static Luminance.Common.Utilities.Utilities;
global using static Microsoft.Xna.Framework.MathHelper;
using Luminance.Core.Graphics;
using Terraria.ModLoader;

namespace Luminance
{
    public class Luminance : Mod
    {
        /// <summary>
        ///     The mod instance for this library.
        /// </summary>
        public static Mod Instance
        {
            get;
            private set;
        }

        public override void Load()
        {
            Instance = this;
        }

        /// <summary>
        /// Performs initialization on the provided mod with all of the librarys features.
        /// </summary>
        /// <param name="mod"></param>
        public static void InitializeMod(Mod mod)
        {
            AtlasManager.InitializeModAtlases(mod);
        }
    }
}
