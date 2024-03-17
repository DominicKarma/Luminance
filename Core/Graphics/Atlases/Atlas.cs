using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    /// <summary>
    /// Represents a texture atlas, which is a large texture sheet with smaller, single textures crammed into it as tightly as possible.<br/>
    /// Single textures are loaded by a data file that contains json data for each texture. Access these textures via <see cref="AtlasManager.GetTexture(string)"/>.
    /// You can create atlases using <see href="http://free-tex-packer.com">this software</see>
    /// </summary>
    public sealed class Atlas : IDisposable
    {
        #region Fields/Properties
        private readonly List<AtlasTexture> textures = new();

        /// <summary>
        /// The texture of the atlas.
        /// </summary>
        public Asset<Texture2D> Texture
        {
            get;
            private set;
        }

        /// <summary>
        /// The mod that owns this atlas.
        /// </summary>
        public Mod AtlasMod
        {
            get;
            private set;
        }

        /// <summary>
        /// The name of the atlas.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// The file path to both the texture and data files, without the file extensions.
        /// </summary>
        public string AtlasPath
        {
            get;
            private set;
        }

        /// <summary>
        /// The size of the atlas texture.
        /// </summary>
        public Vector2 TextureSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether the texture has been disposed of.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// All textures stored on the atlas.
        /// </summary>
        public List<AtlasTexture> Textures => textures;
        #endregion

        #region Constructor
        /// <summary>
        /// Represents a texture atlas, which is a large texture with smaller, single textures crammed into it as tightly as possible.<br/>
        /// Single textures are loaded by a data file, that contains json data for each texture. Access these via <see cref="AtlasManager.GetTexture(string)"/>.
        /// </summary>
        /// <param name="name">The name of the atlas.</param>
        /// <param name="atlasPath">The file path to both the texture and data files, without the file extensions nor the quality suffix.</param>
        /// <param name="usesTextureQuality">Whether this atlas uses texture quality, and should pick from 3 versions based on it.</param>
        public Atlas(Mod mod, string name, string atlasPath)
        {
            AtlasMod = mod;
            Name = name;
            AtlasPath = atlasPath;
            LoadTexture();
            LoadAtlasData();
        }
        #endregion

        #region Methods
        private void LoadTexture()
        {
            Texture = ModContent.Request<Texture2D>(AtlasPath, AssetRequestMode.ImmediateLoad);
            TextureSize = Texture.Value.Size();
        }

        private void LoadAtlasData()
        {
            var filePath = AtlasPath + ".json";

            var cleanedPath = filePath[filePath.IndexOf("Assets")..];
            var bytes = AtlasMod.GetFileBytes(cleanedPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(bytes);

            foreach (var pair in data["frames"])
            {
                var elementInformation = (JsonElement)pair.Value;

                var frameInformation = elementInformation.GetProperty("frame");
                float x = float.Parse(frameInformation.GetProperty("x").ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                float y = float.Parse(frameInformation.GetProperty("y").ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                float width = float.Parse(frameInformation.GetProperty("w").ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                float height = float.Parse(frameInformation.GetProperty("h").ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                Rectangle frame = new((int)x, (int)y, (int)width, (int)height);

                AtlasTexture atlasTexture = new(pair.Key, this, frame);
                textures.Add(atlasTexture);
            }
        }

        void IDisposable.Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Texture.Dispose();
        }
        #endregion
    }
}
