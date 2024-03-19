using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Threading;
using Terraria.ModLoader;
using Terraria;

namespace Luminance.Core.Graphics
{
    public class ParticleManager : ModSystem
    {
        internal static readonly List<Particle> ActiveParticles = new();

        private static readonly Dictionary<BlendState, HashSet<Particle>> DrawCollectionsByBlendState = new();

        public override void Load() => On_Main.DrawDust += DrawParticles;

        public override void Unload() => On_Main.DrawDust -= DrawParticles;

        public override void OnWorldUnload()
        {
            ActiveParticles.Clear();
            foreach (var collection in DrawCollectionsByBlendState.Values)
                collection.Clear();
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

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            MetaballManager.DrawMetaballTargets();
            Main.spriteBatch.End();
        }

        internal static void AddToDrawList(Particle particle) => GetCorrectDrawCollection(particle.BlendState).Add(particle);

        private static void RemoveFromDrawList(Particle particle) => GetCorrectDrawCollection(particle.BlendState).Remove(particle);

        private static HashSet<Particle> GetCorrectDrawCollection(BlendState blendState)
        {
            if (!DrawCollectionsByBlendState.ContainsKey(blendState))
                DrawCollectionsByBlendState[blendState] = new();
            return DrawCollectionsByBlendState[blendState];
        }
    }
}
