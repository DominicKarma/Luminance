using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public class ShaderManager : ModSystem
    {
        /// <summary>
        ///     The set of all shaders handled by this manager class.
        /// </summary>
        private static Dictionary<string, ManagedShader> shaders;

        private static Dictionary<string, ManagedScreenFilter> filters;

        /// <summary>
        ///     Whether this manager class has finished loading all shaders yet or not.
        /// </summary>
        /// <remarks>
        ///     This primarily exists for cases where shaders may be used at mod loading times, such as on the game title screen.
        /// </remarks>
        public static bool HasFinishedLoading
        {
            get;
            private set;
        }

        public static ManagedRenderTarget MainTarget
        {
            get;
            private set;
        }

        public static ManagedRenderTarget AuxilaryTarget
        {
            get;
            private set;
        }

        /// <summary>
        ///     The folder directory in which shaders to autoload are searched for.
        /// </summary>
        public const string AutoloadDirectoryShaders = "AutoloadedEffects/Shaders";

        /// <summary>
        ///     The folder directory in which filters to autoload are searched for.
        /// </summary>
        public const string AutoloadDirectoryFilters = "AutoloadedEffects/Filters";

        public override void OnModLoad()
        {
            // Don't attempt to load shaders on servers.
            if (Main.netMode == NetmodeID.Server)
                return;

            MainTarget = new ManagedRenderTarget(true, ManagedRenderTarget.CreateScreenSizedTarget, true);
            AuxilaryTarget = new ManagedRenderTarget(true, ManagedRenderTarget.CreateScreenSizedTarget, true);

            shaders = [];
            filters = [];
        }

        public override void PostSetupContent()
        {
            // Go through every mod and check for effects to autoload.
            foreach (Mod mod in ModLoader.Mods)
            {
                List<string> fileNames = mod.GetFileNames();
                if (fileNames is null)
                    continue;

                foreach (var path in fileNames)
                {
                    // Ignore paths inside of the compiler directory.
                    if (path?.Contains("Compiler/") ?? true)
                        continue;

                    if (path.Contains(AutoloadDirectoryShaders))
                    {
                        string shaderName = Path.GetFileNameWithoutExtension(path);
                        string clearedPath = Path.Combine(Path.GetDirectoryName(path), shaderName).Replace(@"\", @"/");
                        Ref<Effect> shader = new(mod.Assets.Request<Effect>(clearedPath, AssetRequestMode.ImmediateLoad).Value);
                        SetShader(shaderName, shader);
                    }
                    else if (path.Contains(AutoloadDirectoryFilters))
                    {
                        string filterName = Path.GetFileNameWithoutExtension(path);
                        string clearedPath = Path.Combine(Path.GetDirectoryName(path), filterName).Replace(@"\", @"/");

                        Ref<Effect> filter = new(mod.Assets.Request<Effect>(clearedPath, AssetRequestMode.ImmediateLoad).Value);
                        SetFilter(filterName, filter);
                    }
                }
            }

            // Mark loading operations as finished.
            HasFinishedLoading = true;
        }

        public override void Unload()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            foreach (var shader in shaders.Values)
                shader.Dispose();

            foreach (var filter in filters.Values)
                filter.Dispose();

            shaders = null;
            filters = null;
        }

        internal static void ApplyScreenFilters(RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
        {
            RenderTarget2D target1 = null;
            RenderTarget2D target2 = screenTarget1;

            if (Main.player[Main.myPlayer].gravDir == -1f)
            {
                target1 = AuxilaryTarget;
                Main.instance.GraphicsDevice.SetRenderTarget(target1);
                Main.instance.GraphicsDevice.Clear(clearColor);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Invert(Main.GameViewMatrix.EffectMatrix));
                Main.spriteBatch.Draw(target2, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
                Main.spriteBatch.End();
                target2 = AuxilaryTarget;
            }

            foreach (var filter in filters.Values.Where(filter => filter.Opacity > 0))
            {
                target1 = (target2 != MainTarget.Target) ? MainTarget : AuxilaryTarget;
                Main.instance.GraphicsDevice.SetRenderTarget(target1);
                Main.instance.GraphicsDevice.Clear(clearColor);
                Main.spriteBatch.Begin((SpriteSortMode)1, BlendState.AlphaBlend);
                filter.Apply();
                Main.spriteBatch.Draw(target2, Vector2.Zero, Main.ColorOfTheSkies);
                Main.spriteBatch.End();
                target2 = (target2 != MainTarget.Target) ? MainTarget : AuxilaryTarget;
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
        ///     Retrieves a managed shader of a given name.
        /// </summary>
        /// <remarks>
        ///     In this context, the "name" must correspond with the file name of the shader, not including the path extension.
        /// </remarks>
        /// <param name="name">The name of the shader.</param>
        public static ManagedShader GetShader(string name) => shaders[name];

        /// <summary>
        ///     Retrieves a managed filter of a given name.
        /// </summary>
        /// <remarks>
        ///     In this context, the "name" must correspond with the file name of the filter, not including the path extension.
        /// </remarks>
        /// <param name="name">The name of the filter.</param>
        public static ManagedScreenFilter GetFilter(string name) => filters[name];

        /// <summary>
        ///     Safely retrieves a managed shader of a given name.
        /// </summary>
        /// <remarks>
        ///     In this context, the "name" must correspond with the file name of the shader, not including the path extension.
        /// </remarks>
        /// <param name="name">The name of the shader.</param>
        /// <param name="shader">The shader output.</param>
        public static bool TryGetShader(string name, out ManagedShader shader) => shaders.TryGetValue(name, out shader);

        /// <summary>
        ///     Safely retrieves a managed filter of a given name.
        /// </summary>
        /// <remarks>
        ///     In this context, the "name" must correspond with the file name of the filter, not including the path extension.
        /// </remarks>
        /// <param name="name">The name of the filter.</param>
        /// <param name="filter">The filter output.</param>
        public static bool TryGetFilter(string name, out ManagedScreenFilter filter) => filters.TryGetValue(name, out filter);

        /// <summary>
        ///     Sets a shader with a given name in the registry manually.
        /// </summary>
        /// <param name="name">The name of the shader.</param>
        /// <param name="newShaderData">The shader data reference to save.</param>
        public static void SetShader(string name, Ref<Effect> newShaderData) => shaders[name] = new(newShaderData);

        /// <summary>
        ///     Sets a filter with a given name in the registry manually.
        /// </summary>
        /// <param name="name">The name of the filter.</param>
        /// <param name="newShaderData">The shader data reference to save.</param>
        public static void SetFilter(string name, Ref<Effect> newShaderData) => filters[name] = new(newShaderData);
    }
}
