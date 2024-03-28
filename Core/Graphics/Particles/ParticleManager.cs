using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Threading;
using Terraria;
using System;
using System.Linq;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public class ParticleManager : ModSystem
    {
        internal static readonly List<Particle> ActiveParticles = [];

        private static readonly Dictionary<BlendState, List<Particle>> DrawCollectionsByBlendState = [];

        internal static readonly Dictionary<Type, List<Particle>> ManualDrawCollections = [];

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
            // Guarenteed to be registered here unless manually drawn is set by reflection or smth (do not do that).
            if (particle.ManuallyDrawn)
                ManualRenderers[particle.GetType()].AddParticle(particle);
            else
                GetCorrectDrawCollection(particle).Add(particle);
        }

        private static void RemoveFromDrawList(Particle particle)
        {
            if (particle.ManuallyDrawn)
                ManualRenderers[particle.GetType()].RemoveParticle(particle);
            else
                GetCorrectDrawCollection(particle).Remove(particle);
        }

        private static List<Particle> GetCorrectDrawCollection(Particle particle)
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
    }
}
