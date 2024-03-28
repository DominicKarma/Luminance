using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public class AtlasManager : ILoadable
    {
        #region Fields/Properties
        private static readonly HashSet<Atlas> Atlases = [];

        private static readonly Dictionary<string, AtlasTexture> AllTexturesByName = [];
        #endregion

        #region Loading
        private static void AddAtlas(Atlas atlas)
        {
            for (int i = 0; i < atlas.Textures.Count; i++)
            {
                AtlasTexture texture = atlas.Textures[i];
                // Mods should not throw here due to having a texture with the same name as another mod, as that is unreasonable.
                var storedName = texture.Atlas.AtlasMod.Name + "." + texture.Name;

                if (AllTexturesByName.ContainsKey(storedName))
                    throw new Exception($"Duplicate atlas texture name '{storedName}' found! All atlas texture names must be unique!");

                AllTexturesByName[storedName] = texture;
            }
            Atlases.Add(atlas);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets a texture with the given name from the atlases.
        /// </summary>
        /// <param name="textureName">The unique texture name, prefixed with the mod name. For example, "MyMod.Particles".</param>
        /// <returns>The texture, or <see langword="null"/> if it was not found.</returns>
        public static AtlasTexture GetTexture(string textureName)
        {
            if (AllTexturesByName.TryGetValue(textureName, out var value))
                return value;

            ModContent.GetInstance<Luminance>().Logger.Error($"Atlas texture with name {textureName} was not found!");
            return null;
        }

        // TODO: This should probably use the null object pattern? Either that, or it should be private with the above public (as you almost never need to get the atlas object itself,
        // only the textures stored on it).
        /// <summary>
        /// Gets the atlas with the given name. Will return <see langword="null"/> if it is not registered.
        /// </summary>
        /// <param name="atlasName">The name of the atlas.</param>
        /// <returns>The atlas.</returns>
        public static Atlas GetAtlas(string atlasName)
        {
            foreach (var atlas in Atlases)
            {
                if (atlas.Name == atlasName)
                    return atlas;
            }

            ModContent.GetInstance<Luminance>().Logger.Info($"Atlas '{atlasName}' is not registered!");
            return null;
        }

        /// <summary>
        /// Attempts to get the atlas with the given name.
        /// </summary>
        /// <param name="atlasName">The name of the atlas.</param>
        /// <param name="atlas">The atlas.</param>
        /// <returns>Whether the atlas was found.</returns>
        public static bool TryGetAtlas(string atlasName, out Atlas atlas)
        {
            atlas = GetAtlas(atlasName);
            return atlas != null;
        }

        /// <summary>
        /// Checks if an atlas with the given name is registered.
        /// </summary>
        /// <param name="atlasName">The name of the atlas.</param>
        /// <returns>Whether the atlas is registered.</returns>
        public static bool AtlasIsRegistered(string atlasName)
        {
            for (int i = 0; i < Atlases.Count; i++)
            {
                if (Atlases.ElementAt(i).Name == atlasName)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Registers an atlas.
        /// </summary>
        /// <param name="mod">The mod this atlas belongs to.</param>
        /// <param name="name">The name of the atlas. Must be unique.</param>
        /// <param name="atlasPath">The file path to the atlas files, not including the file extensions.</param>
        /// <returns>The atlas that was registered.</returns>
        public static Atlas RegisterAtlas(Mod mod, string name, string atlasPath)
        {
            if (AtlasIsRegistered(name))
                return GetAtlas(name);

            Atlas atlas = new(mod, name, atlasPath);
            AddAtlas(atlas);
            return atlas;
        }
        #endregion

        #region NonPublic Methods
        internal static void InitializeModAtlases(Mod mod)
        {
            // A variable path *could* be used here, but its easier to enforce mods using the hardcoded path (and encourages usage of a proper assets directory).
            List<string> modFileNames = mod.GetFileNames();
            if (modFileNames is null)
                return;

            var atlases = modFileNames.Where(p => p?.Contains("Assets/Atlases") ?? false);
            foreach (string path in atlases)
            {
                string assetFilepath = $"{mod.Name}/" + path;
                string filename = Path.GetFileNameWithoutExtension(assetFilepath);
                string formattedAtlasPath = assetFilepath.Replace(".json", string.Empty).Replace(".xnb", string.Empty);

                if (!AtlasIsRegistered(filename))
                    RegisterAtlas(mod, filename, formattedAtlasPath);
            }
        }

        // Required by the interface, but as this is a library mod, it does not need to load any atlases from itself.
        public void Load(Mod mod) { }

        public void Unload()
        {
            foreach (var atlas in Atlases)
                // Note: the "as" operator does *not* call custom explicit casting implementations. That does not matter here, but it is a good habit to explicity cast instead.
                // Reference: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/user-defined-conversion-operators
                ((IDisposable)atlas).Dispose();

            Atlases.Clear();
            AllTexturesByName.Clear();
        }
        #endregion
    }
}
