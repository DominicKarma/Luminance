using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Threading;
using Terraria;
using System;
using Luminance.Common.Utilities;
using System.Linq;

namespace Luminance.Core.Graphics
{
    public class ParticleManager : ModSystem
    {
        internal static readonly List<Particle> ActiveParticles = [];

        private static readonly Dictionary<BlendState, HashSet<Particle>> DrawCollectionsByBlendState = [];

        internal static readonly Dictionary<Type, HashSet<Particle>> ManualDrawCollections = [];

        internal static readonly Dictionary<Type, IManualParticleRenderer> ManualRenderers = [];

        public override void Load() => On_Main.DrawDust += DrawParticles;

        public override void Unload() => On_Main.DrawDust -= DrawParticles;

        public override void OnWorldUnload()
        {
            ActiveParticles.Clear();
            foreach (var collection in DrawCollectionsByBlendState.Values)
                collection.Clear();
        }

        internal static void InitializeManualRenderers(Mod mod)
        {
            var contentInterfaces = mod.GetContent().Where(c =>
            {
                return c is ModType t and IManualParticleRenderer;
            }).Select(c => c as IManualParticleRenderer);

            foreach (var content in contentInterfaces)
                content.RegisterRenderer();
        }

        public override void PostUpdateDusts()
        {
            // Testing has shown that fast parallel is faster in most cases with lower particle counts, and drastically faster with higher.
            FastParallel.For(0, ActiveParticles.Count, (int x, int y, object context) =>
            {
                for (int i = x; i < y; i++)
                {
                    ActiveParticles[i].Update();
                    ActiveParticles[i].Position += ActiveParticles[i].Velocity;
                    ActiveParticles[i].Time++;
                }
            });

            ActiveParticles.RemoveAll(particle =>
            {
                if (particle.Time >= particle.Lifetime)
                {
                    RemoveFromDrawList(particle);
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Draws all active particles of a given type with a given blend state.
        /// </summary>
        /// <typeparam name="ParticleType">The particle type to draw.</typeparam>
        /// <param name="blendState">The blend state to draw with.</param>
        public static void DrawAllParticlesOfType<ParticleType>(BlendState blendState)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, blendState, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var particle in ActiveParticles)
            {
                if (particle is not ParticleType)
                    continue;

                particle.Draw(Main.spriteBatch);
            }

            Main.spriteBatch.ResetToDefault();
        }

        private void DrawParticles(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            foreach (var keyValuePair in DrawCollectionsByBlendState)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, keyValuePair.Key, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (var particle in keyValuePair.Value)
                    particle.Draw(Main.spriteBatch);

                Main.spriteBatch.End();
            }

            foreach (var pair in ManualRenderers)
                pair.Value.RenderParticles();

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            MetaballManager.DrawMetaballTargets();
            Main.spriteBatch.End();
        }

        internal static void RegisterRenderer<TParticleType>(IManualParticleRenderer particleRenderer) where TParticleType : Particle => ManualRenderers[typeof(TParticleType)] = particleRenderer;

        internal static void AddToDrawList(Particle particle)
        {
            if (particle.ManuallyDrawn)
            {
                if (!ManualRenderers.TryGetValue(particle.GetType(), out var renderer))
                {
                    ModContent.GetInstance<Luminance>().Logger.Error($"Particle {particle.GetType()} is marked as manually drawn, but no renderer was found! Defaulting to automated drawing.");
                    GetCorrectDrawCollection(particle).Add(particle);
                }
                else
                    renderer.AddParticle(particle);
            }
            else
                GetCorrectDrawCollection(particle).Add(particle);
        }

        private static void RemoveFromDrawList(Particle particle)
        {
            if (particle.ManuallyDrawn)
            {
                if (!ManualRenderers.TryGetValue(particle.GetType(), out var renderer))
                {
                    ModContent.GetInstance<Luminance>().Logger.Error($"Particle {particle.GetType()} is marked as manually drawn, but no renderer was found! Defaulting to automated drawing.");
                    GetCorrectDrawCollection(particle).Remove(particle);
                }
                else
                    renderer.RemoveParticle(particle);
            }
            else
                GetCorrectDrawCollection(particle).Remove(particle);
        }

        private static HashSet<Particle> GetCorrectDrawCollection(Particle particle)
        {
            if (!particle.ManuallyDrawn)
            {
                if (!DrawCollectionsByBlendState.ContainsKey(particle.BlendState))
                    DrawCollectionsByBlendState[particle.BlendState] = [];
                return DrawCollectionsByBlendState[particle.BlendState];
            }
            else
            {
                if (!ManualDrawCollections.ContainsKey(particle.GetType()))
                    ManualDrawCollections[particle.GetType()] = [];
                return ManualDrawCollections[particle.GetType()];
            }
        }
        
        /// <summary>
        /// Returns a list of active particles to be drawn manually. Try to do this optimally.
        /// </summary>
        /// <typeparam name="TParticleType"></typeparam>
        /// <returns></returns>
        public static HashSet<Particle> GetManualDrawingParticles<TParticleType>() where TParticleType : Particle
        {
            if (ManualDrawCollections.TryGetValue(typeof(TParticleType), out var collection))
                return collection;
            return [];
        }
    }
}
