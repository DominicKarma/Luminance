using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Threading;
using Terraria.ModLoader;
using Terraria;

namespace Luminance.Core.Graphics
{
    public class ParticleManager : ModSystem
    {
        internal static readonly List<Particle> ActiveParticles = new();

        private static readonly Dictionary<BlendState, HashSet<Particle>> DrawLists = new();

        public override void Load() => On_Main.DrawDust += DrawParticles;

        public override void Unload() => On_Main.DrawDust -= DrawParticles;

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

            foreach (var keyValuePair in  DrawLists)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, keyValuePair.Key, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (var particle in keyValuePair.Value)
                    particle.Draw(Main.spriteBatch);

                Main.spriteBatch.End();
            }
        }

        internal static void AddToDrawList(Particle particle) => GetCorrectDrawCollection(particle.BlendState).Add(particle);

        private static void RemoveFromDrawList(Particle particle) => GetCorrectDrawCollection(particle.BlendState).Remove(particle);

        private static HashSet<Particle> GetCorrectDrawCollection(BlendState blendState)
        {
            if (!DrawLists.ContainsKey(blendState))
                DrawLists[blendState] = new();
            return DrawLists[blendState];
        }
    }
}
