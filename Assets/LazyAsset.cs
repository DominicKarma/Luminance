using System;
using ReLogic.Content;
using Terraria.ModLoader;

namespace KarmaLibrary.Assets
{
    /// <summary>
    ///     <see cref="Asset{T}"/> wrapper that facilitates lazy-loading.
    /// </summary>
    /// <typeparam name="AssetType">The asset type.</typeparam>
    public readonly struct LazyAsset<AssetType>(Func<Asset<AssetType>> assetInitializer) where AssetType : class
    {
        private readonly Lazy<Asset<AssetType>> asset = new(assetInitializer);

        /// <summary>
        ///     The lazy-initialized asset.
        /// </summary>
        public Asset<AssetType> Asset => asset.Value;

        /// <summary>
        ///     The value underlying this asset.
        /// </summary>
        public AssetType Value => asset.Value.Value;

        /// <summary>
        ///     Requests an asset, wrapped in a <see cref="LazyAsset{AssetType}"/>.
        /// </summary>
        /// <param name="path">The path to the asset.</param>
        /// <param name="requestMode">The request mode by which the asset should be loaded. Defaults to <see cref="AssetRequestMode.AsyncLoad"/>.</param>
        public static LazyAsset<AssetType> Request(string path, AssetRequestMode requestMode = AssetRequestMode.AsyncLoad)
        {
            return new LazyAsset<AssetType>(() => ModContent.Request<AssetType>(path, requestMode));
        }
    }
}
