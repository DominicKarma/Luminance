using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Threading;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public abstract class MetaballType : ModType
    {
        #region Fields/Properties
        internal List<MetaballInstance> Particles = new();

        public int ActiveParticleCount => Particles.Count;

        public readonly List<ManagedRenderTarget> LayerTargets = new();
        #endregion

        #region Abstract/Virtual Fields/Properties
        public abstract bool ShouldRender
        {
            get;
        }

        // This uses a Texture2D rather than an Asset to allow for the usage of render targets when working with metaballs.
        public abstract Func<Texture2D>[] LayerTextures
        {
            get;
        }

        public abstract Color EdgeColor
        {
            get;
        }

        public abstract string MetaballAtlasTextureToUse { get; }
        #endregion

        #region Instance Methods
        public void CreateParticle(Vector2 spawnPosition, Vector2 velocity, float size, float extraInfo0 = 0f, float extraInfo1 = 0f, float extraInfo2 = 0f, float extraInfo3 = 0f) =>
            Particles.Add(new(spawnPosition, velocity, size, extraInfo0, extraInfo1, extraInfo2, extraInfo3));

        public void ClearInstances() => Particles.Clear();

        public void Update()
        {
            // Initial overhead is worth the amount of speed this gains with higher amounts of instances.
            FastParallel.For(0, Particles.Count, (j, k, callback) =>
            {
                for (int i = j; i < k; i++)
                {
                    var p = Particles[i];
                    UpdateParticle(p);
                    p.Center += p.Velocity;
                }
            });

            Particles.RemoveAll(ShouldKillParticle);
        }

        public void DrawInstances()
        {
            var texture = AtlasManager.GetTexture(MetaballAtlasTextureToUse);

            foreach (var particle in Particles)
                Main.spriteBatch.Draw(texture, particle.Center - Main.screenPosition, null, Color.White, 0f, null, new Vector2(particle.Size) / texture.Frame.Size(), SpriteEffects.None);

            ExtraDrawing();
        }

        protected sealed override void Register()
        {
            // Register this metaball mod TML's inbuilt ModType handlers.
            ModTypeLookup<MetaballType>.Register(this);

            // Store this metaball instance in the personalized manager so that it can be kept track of for rendering purposes.
            if (!MetaballManager.MetaballTypes.Contains(this))
                MetaballManager.MetaballTypes.Add(this);

            // Disallow render target creation on servers.
            if (Main.netMode == NetmodeID.Server)
                return;

            // Generate render targets.
            Main.QueueMainThreadAction(() =>
            {
                // Load render targets.
                int layerCount = LayerTextures.Length;
                for (int i = 0; i < layerCount; i++)
                    LayerTargets.Add(new(true, ManagedRenderTarget.CreateScreenSizedTarget));
            });
        }

        /// <summary>
        /// Renders the contents of this metaball.
        /// </summary>
        public void RenderLayerWithShader()
        {
            for (int i = 0; i < LayerTargets.Count; i++)
            {
                PrepareShaderForTarget(i);
                Main.spriteBatch.Draw(LayerTargets[i], Main.screenLastPosition - Main.screenPosition, Color.White);
            }
        }

        /// <summary>
        /// Disposes of all unmanaged GPU resources used up by the <see cref="LayerTargets"/>. This is called automatically on mod unload.<br></br>
        /// <i>It is your responsibility to recreate layer targets later if you call this method manually.</i>
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < LayerTargets.Count; i++)
                LayerTargets[i]?.Dispose();
        }
        #endregion

        #region Virtual Methods

        // This is needed for Wrath of the Gods.
        /// <summary>
        /// Whether this metaball should be drawn manually.
        /// </summary>
        public virtual bool DrawnManually => false;

        /// <summary>
        /// Whether the layer overlay contents at the provided index should be fixed to the screen.<br/>
        /// When true, the texture will be statically drawn to the screen with no respect for world position.
        /// </summary>
        public virtual bool LayerIsFixedToScreen(int layerIndex) => false;

        /// <summary>
        /// Optionally overridable method that can be used to make layers offset when drawn, to allow for layer-specific animations. Defaults to <see cref="Vector2.Zero"/>, aka no animation.
        /// </summary>
        public virtual Vector2 CalculateManualOffsetForLayer(int layerIndex) => Vector2.Zero;

        /// <summary>
        /// Use this method to use your own custom spritebatch begin call.<br/>
        /// <b>Return <see langword="true"/> if you do this.</b><br/>
        /// Returns <see langword="false"/> by default.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <returns></returns>
        public virtual bool PerformCustomSpritebatchBegin(SpriteBatch spriteBatch) => false;

        /// <summary>
        /// Whether a given particle in the <see cref="Particles"/> list should be killed or not.
        /// </summary>
        /// <param name="particle">The particle to determine the kill state of.</param>
        public abstract bool ShouldKillParticle(MetaballInstance particle);

        /// <summary>
        /// Optionally overridable method that defines for preparations for the render target. Defaults to using the typical texture overlay behavior.
        /// </summary>
        /// <param name="layerIndex">The layer index that should be prepared for.</param>
        public virtual void PrepareShaderForTarget(int layerIndex)
        {
            // Store the in an easy to use local variables.
            var metaballShader = ShaderManager.GetShader("Luminance.MetaballEdgeShader");

            // Fetch the layer texture. This is the texture that will be overlaid over the greyscale contents on the screen.
            Texture2D layerTexture = LayerTextures[layerIndex]();

            // Calculate the layer scroll offset. This is used to ensure that the texture contents of the given metaball have parallax, rather than being static over the screen
            // regardless of world position.
            // This may be toggled off optionally by the metaball.
            Vector2 screenSize = Main.ScreenSize.ToVector2();
            Vector2 layerScrollOffset = Main.screenPosition / screenSize + CalculateManualOffsetForLayer(layerIndex);
            if (LayerIsFixedToScreen(layerIndex))
                layerScrollOffset = Vector2.Zero;

            // Supply shader parameter values.
            metaballShader.TrySetParameter("layerSize", layerTexture.Size());
            metaballShader.TrySetParameter("screenSize", screenSize);
            metaballShader.TrySetParameter("layerOffset", layerScrollOffset);
            metaballShader.TrySetParameter("edgeColor", EdgeColor.ToVector4());
            metaballShader.TrySetParameter("singleFrameScreenOffset", (Main.screenLastPosition - Main.screenPosition) / screenSize);

            // Supply the metaball's layer texture to the graphics device so that the shader can read it.
            metaballShader.SetTexture(layerTexture, 1, SamplerState.LinearWrap);

            // Apply the metaball shader.
            metaballShader.Apply();
        }

        /// <summary>
        /// Optional method that allows for drawing optional things to the layers.
        /// </summary>
        public virtual void ExtraDrawing() { }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Update things such as velocity, size, and any optional things here. The center will be automatically updated by velocity after this method has ran.<br/>
        /// Be aware that this method is called in parallel, and as such should not modify anything other than the particle instance provided.
        /// </summary>
        public abstract void UpdateParticle(MetaballInstance particle);
        #endregion
    }
}
