using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace KarmaLibrary.Core.Graphics.ParticleMangers
{
    public class ParticleManager : ModSystem
    {
        internal static readonly List<Particle> activeParticles = [];

        internal static readonly Dictionary<Type, int> particleIDLookup = [];

        // This acts as a central storage for particle textures, so that each one does not require a ModContent.Request call.
        internal static readonly Dictionary<int, Texture2D> particleTextureLookup = [];

        internal static readonly Dictionary<int, Mod> particleModOwnerLookup = [];

        public override void OnModLoad()
        {
            // Don't attempt to load particle information on the server, as particles are purely graphical objects.
            if (Main.netMode == NetmodeID.Server)
                return;

            // Prepare the draw detour.
            On_Main.SortDrawCacheWorms += DrawParticlesDetour;
        }

        public override void PostSetupContent()
        {
            // I do not know why particles are not being autoloaded by TML, but they aren't.
            foreach (Mod mod in ModLoader.Mods)
            {
                var particleTypes = AssemblyManager.GetLoadableTypes(mod.Code).Where(t => t.IsAssignableTo(typeof(Particle)) && !t.IsAbstract);
                foreach (var type in particleTypes)
                {
                    Particle particle = (Particle)FormatterServices.GetUninitializedObject(type);
                    SetIDForParticle(particle, mod);
                    particle.RegisterSelf();
                }
            }
        }

        private void DrawParticlesDetour(On_Main.orig_SortDrawCacheWorms orig, Main self)
        {
            DrawParticles();
            orig(self);
        }

        private static void DrawParticles()
        {
            // Do nothing if there are no particles to draw.
            if (!activeParticles.Any())
                return;

            // Group particles via their blend state. This will determine how they are drawn.
            var blendGroups = activeParticles.GroupBy(p => p.DrawBlendState);
            foreach (var blendGroup in blendGroups)
            {
                // Prepare a rasterizer for screen culling, to keep drawing optimized.
                RasterizerState screenCull = PrepareScreenCullRasterizer();

                // Prepare the drawing with the specified blend state.
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, blendGroup.First().DrawBlendState, Main.DefaultSamplerState, DepthStencilState.None, screenCull, null, Main.GameViewMatrix.TransformationMatrix);

                // Draw all particles with the desired blend state.
                foreach (Particle p in blendGroup)
                    p.Draw();

                Main.spriteBatch.End();
            }
        }

        public override void PostUpdateDusts()
        {
            // Perform automatic particle cleanup.
            activeParticles.RemoveAll(p => p.Time >= p.Lifetime);

            // Update all particles in parallel.
            int particleCount = activeParticles.Count;
            Parallel.For(0, particleCount, i =>
            {
                activeParticles[i].Time++;
                activeParticles[i].Update();
                activeParticles[i].Position += activeParticles[i].Velocity;
            });
        }

        internal static void SetIDForParticle(Particle particle, Mod mod)
        {
            int particleID = 0;
            if (particleIDLookup?.Any() ?? false)
                particleID = particleIDLookup.Values.Max() + 1;

            // Store an ID for the particle. All particles of this type that are spawned will copy the ID.
            Type particleType = particle.GetType();
            particleIDLookup[particleType] = particleID;
            particleModOwnerLookup[particleID] = mod;
            particle.ID = particleID;
        }

        internal static void SetTextureForParticle(Particle particle)
        {
            // Don't attempt to load textures server-side.
            if (Main.netMode == NetmodeID.Server)
                return;

            // Store the particle's texture in the lookup table.
            Texture2D particleTexture = ModContent.Request<Texture2D>(particle.TexturePath, AssetRequestMode.ImmediateLoad).Value;
            particleTextureLookup[particle.ID] = particleTexture;
        }
    }
}
