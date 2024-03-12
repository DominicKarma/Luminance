using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace KarmaLibrary.Assets
{
    public class MiscTexturesRegistry : ModSystem
    {
        #region Texture Path Constants

        public const string PixelPath = $"{ExtraTexturesPath}/Pixel";

        public const string InvisiblePixelPath = $"{ExtraTexturesPath}/InvisiblePixel";

        public const string ExtraTexturesPath = "KarmaLibrary/Assets/ExtraTextures";

        public const string GreyscaleTexturesPath = "KarmaLibrary/Assets/ExtraTextures/GreyscaleTextures";

        public const string NoiseTexturesPath = "KarmaLibrary/Assets/ExtraTextures/Noise";

        public const string ChromaticBurstPath = $"{GreyscaleTexturesPath}/ChromaticBurst";

        #endregion Texture Path Constants

        #region Greyscale Textures

        public static readonly LazyAsset<Texture2D> BloomCircleSmall = LoadDeferred($"{GreyscaleTexturesPath}/BloomCircleSmall");

        public static readonly LazyAsset<Texture2D> BloomFlare = LoadDeferred($"{GreyscaleTexturesPath}/BloomFlare");

        public static readonly LazyAsset<Texture2D> BloomLineTexture = LoadDeferred($"{GreyscaleTexturesPath}/BloomLine");

        public static readonly LazyAsset<Texture2D> ChromaticBurst = LoadDeferred(ChromaticBurstPath);

        public static readonly LazyAsset<Texture2D> ShineFlareTexture = LoadDeferred($"{GreyscaleTexturesPath}/ShineFlare");

        #endregion Greyscale Textures

        #region Noise Textures

        public static readonly LazyAsset<Texture2D> DendriticNoise = LoadDeferred($"{NoiseTexturesPath}/DendriticNoise");

        public static readonly LazyAsset<Texture2D> DendriticNoiseZoomedOut = LoadDeferred($"{NoiseTexturesPath}/DendriticNoiseZoomedOut");

        public static readonly LazyAsset<Texture2D> TurbulentNoise = LoadDeferred($"{NoiseTexturesPath}/TurbulentNoise");

        public static readonly LazyAsset<Texture2D> WavyBlotchNoise = LoadDeferred($"{NoiseTexturesPath}/WavyBlotchNoise");

        #endregion Noise Textures

        #region Pixel

        // Self-explanatory. Sometimes shaders need a "blank slate" in the form of an invisible texture to draw their true contents onto, which this can be beneficial for.
        public static readonly LazyAsset<Texture2D> InvisiblePixel = LoadDeferred(InvisiblePixelPath);

        // Self-explanatory.
        public static readonly LazyAsset<Texture2D> Pixel = LoadDeferred(PixelPath);

        #endregion Pixel

        #region Loader Utility

        private static LazyAsset<Texture2D> LoadDeferred(string path)
        {
            // Don't attempt to load anything server-side.
            if (Main.netMode == NetmodeID.Server)
                return default;

            return LazyAsset<Texture2D>.Request(path, AssetRequestMode.ImmediateLoad);
        }

        #endregion Loader Utility
    }
}
