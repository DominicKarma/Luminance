using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    /// <summary>
    /// The class responsible for central shader management, including both general-purpose and screen filter shaders.
    /// </summary>
    public sealed class ShaderManager : ModSystem
    {
        /// <summary>
        /// The set of all shaders handled by this manager class.
        /// </summary>
        internal static Dictionary<string, ManagedShader> shaders;

        /// <summary>
        /// The set of all filters handled by this manager class.
        /// </summary>
        internal static Dictionary<string, ManagedScreenFilter> filters;

        /// <summary>
        /// Whether this manager class has finished loading all shaders yet or not.
        /// </summary>
        /// 
        /// <remarks>
        /// This primarily exists for cases where shaders may be used at mod loading times, such as on the game title screen.
        /// </remarks>
        public static bool HasFinishedLoading
        {
            get;
            internal set;
        }

        /// <summary>
        /// The main screen render target used for screen filter management.
        /// </summary>
        public static ManagedRenderTarget MainTarget
        {
            get;
            private set;
        }

        /// <summary>
        /// A secondary screen render target used for screen filter management, as a means of swapping back and forth to create repeated shader effects.
        /// </summary>
        public static ManagedRenderTarget AuxiliaryTarget
        {
            get;
            private set;
        }

        /// <summary>
        /// The folder directory in which shaders to autoload are searched for.
        /// </summary>
        public const string AutoloadDirectoryShaders = "AutoloadedEffects/Shaders";

        /// <summary>
        /// The folder directory in which filters to autoload are searched for.
        /// </summary>
        public const string AutoloadDirectoryFilters = "AutoloadedEffects/Filters";

        /// <summary>
        /// Handles all shader initialization effects.
        /// </summary>
        public override void OnModLoad()
        {
            // Don't attempt to load shaders on servers.
            if (Main.netMode == NetmodeID.Server)
                return;

            MainTarget = new ManagedRenderTarget(true, ManagedRenderTarget.CreateScreenSizedTarget, true);
            AuxiliaryTarget = new ManagedRenderTarget(true, ManagedRenderTarget.CreateScreenSizedTarget, true);

            shaders = [];
            filters = [];
        }

        internal static void LoadShaders(Mod mod)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            List<string> fileNames = mod.GetFileNames();
            if (fileNames is null)
                return;

            #region Shaders
            var shaderLoadPaths = fileNames.Where(path => path.Contains(AutoloadDirectoryShaders) && !path.Contains("Compiler/") && (path.Contains(".xnb") || path.Contains(".fxc")));
            var shaderFxPathsToCompile = fileNames.Where(path => path.Contains(AutoloadDirectoryShaders) && !path.Contains("Compiler/") && path.Contains(".fx") && !shaderLoadPaths.Contains(path.Replace(".fx", ".xnb")));

            foreach (var path in shaderLoadPaths)
            {
                string shaderName = mod.Name + '.' + Path.GetFileNameWithoutExtension(path);
                string clearedPath = Path.Combine(Path.GetDirectoryName(path), shaderName).Replace(@"\", @"/").Replace($"{mod.Name}.", string.Empty);
                Ref<Effect> shader = new(mod.Assets.Request<Effect>(clearedPath, AssetRequestMode.ImmediateLoad).Value);
                SetShader(shaderName, shader);
            }

            foreach (var path in shaderFxPathsToCompile)
            {
                string fxPath = mod.Name + "\\" + Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path)).Replace(@"\", @"/");
                ShaderRecompilationMonitor.CompilingFiles.Enqueue(new(Path.Combine(Main.SavePath, "ModSources", fxPath), false));
            }
            #endregion

            #region Filters
            var filterLoadPaths = fileNames.Where(path => path.Contains(AutoloadDirectoryFilters) && !path.Contains("Compiler/") && (path.Contains(".xnb") || path.Contains(".fxc")));
            var filterFxPathsToCompile = fileNames.Where(path => path.Contains(AutoloadDirectoryFilters) && !path.Contains("Compiler/") && path.Contains(".fx") && !filterLoadPaths.Contains(path.Replace(".fx", ".xnb")));

            foreach (var path in filterLoadPaths)
            {
                string filterName = mod.Name + '.' + Path.GetFileNameWithoutExtension(path);
                string clearedPath = Path.Combine(Path.GetDirectoryName(path), filterName).Replace(@"\", @"/").Replace($"{mod.Name}.", string.Empty);
                Ref<Effect> filter = new(mod.Assets.Request<Effect>(clearedPath, AssetRequestMode.ImmediateLoad).Value);
                SetFilter(filterName, filter);
            }

            foreach (var path in filterFxPathsToCompile)
            {
                string fxPath = mod.Name + "\\" + Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path)).Replace(@"\", @"/");
                ShaderRecompilationMonitor.CompilingFiles.Enqueue(new(Path.Combine(Main.SavePath, "ModSources", fxPath), true));
            }
            #endregion
        }

        public override void Unload()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Main.QueueMainThreadAction(() =>
            {
                foreach (var shader in shaders.Values)
                    shader.Dispose();

                foreach (var filter in filters.Values)
                    filter.Dispose();

                shaders = null;
                filters = null;
            });
        }

        internal static void ApplyScreenFilters(RenderTarget2D _, RenderTarget2D screenTarget1, RenderTarget2D _2, Color clearColor)
        {
            RenderTarget2D target1 = null;
            RenderTarget2D target2 = screenTarget1;

            if (Main.player[Main.myPlayer].gravDir == -1f)
            {
                target1 = AuxiliaryTarget;
                Main.instance.GraphicsDevice.SetRenderTarget(target1);
                Main.instance.GraphicsDevice.Clear(clearColor);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Invert(Main.GameViewMatrix.EffectMatrix));
                Main.spriteBatch.Draw(target2, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
                Main.spriteBatch.End();
                target2 = AuxiliaryTarget;
            }

            List<ManagedScreenFilter> activeFilters = filters.Values.Where(filter => filter.Opacity > 0).ToList();
            foreach (var filter in activeFilters)
            {
                target1 = (target2 != MainTarget.Target) ? MainTarget : AuxiliaryTarget;
                Main.instance.GraphicsDevice.SetRenderTarget(target1);
                Main.instance.GraphicsDevice.Clear(clearColor);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                filter.Apply();
                Main.spriteBatch.Draw(target2, Vector2.Zero, Main.ColorOfTheSkies);
                Main.spriteBatch.End();
                target2 = (target2 != MainTarget.Target) ? MainTarget : AuxiliaryTarget;
            }

            if (target1 != null)
            {
                Main.instance.GraphicsDevice.SetRenderTarget(screenTarget1);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Main.spriteBatch.Draw(target1, Vector2.Zero, Color.White);
                Main.spriteBatch.End();
            }
        }

        public override void PostUpdateEverything()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            foreach (var filter in filters.Values)
            {
                filter.Update();
                filter.Deactivate();
            }
        }

        /// <summary>
        /// Retrieves a managed shader of a given name.
        /// </summary>
        /// <remarks>
        /// In this context, the "name" must correspond with the file name of the shader, not including the path extension.
        /// </remarks>
        /// <param name="name">The name of the shader.</param>
        public static ManagedShader GetShader(string name) => shaders[name];

        /// <summary>
        /// Retrieves a managed filter of a given name.
        /// </summary>
        /// <remarks>
        /// In this context, the "name" must correspond with the file name of the filter, not including the path extension.
        /// </remarks>
        /// <param name="name">The name of the filter.</param>
        public static ManagedScreenFilter GetFilter(string name) => filters[name];

        /// <summary>
        /// Safely retrieves a managed shader of a given name.
        /// </summary>
        /// <remarks>
        /// In this context, the "name" must correspond with the file name of the shader, not including the path extension.
        /// </remarks>
        /// <param name="name">The name of the shader.</param>
        /// <param name="shader">The shader output.</param>
        public static bool TryGetShader(string name, out ManagedShader shader) => shaders.TryGetValue(name, out shader);

        /// <summary>
        /// Safely retrieves a managed filter of a given name.
        /// </summary>
        /// <remarks>
        /// In this context, the "name" must correspond with the file name of the filter, not including the path extension.
        /// </remarks>
        /// <param name="name">The name of the filter.</param>
        /// <param name="filter">The filter output.</param>
        public static bool TryGetFilter(string name, out ManagedScreenFilter filter) => filters.TryGetValue(name, out filter);

        /// <summary>
        /// Sets a shader with a given name in the registry manually.
        /// </summary>
        /// <param name="name">The name of the shader.</param>
        /// <param name="newShaderData">The shader data reference to save.</param>
        public static void SetShader(string name, Ref<Effect> newShaderData) => shaders[name] = new(newShaderData);

        /// <summary>
        /// Sets a filter with a given name in the registry manually.
        /// </summary>
        /// <param name="name">The name of the filter.</param>
        /// <param name="newShaderData">The shader data reference to save.</param>
        public static void SetFilter(string name, Ref<Effect> newShaderData) => filters[name] = new(newShaderData);
    }
}
