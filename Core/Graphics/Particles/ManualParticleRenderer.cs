using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    internal interface IManualParticleRenderer
    {
        void RegisterRenderer();

        void AddParticle(Particle particle);

        void RemoveParticle(Particle particle);

        void RenderParticles();
    }

    /// <summary>
    /// Provides iterating over the active assosiated particle collection, allowing for more control about how they are rendered.
    /// </summary>
    /// <typeparam name="TParticleType">The particle type that is assosiated with this renderer. This particle should set <see cref="Particle.ManuallyDrawn"/> to true.</typeparam>
    public abstract class ManualParticleRenderer<TParticleType> : ModType, IManualParticleRenderer where TParticleType : Particle
    {
        protected HashSet<TParticleType> Particles = [];

        protected sealed override void Register() => ModTypeLookup<ManualParticleRenderer<TParticleType>>.Register(this);

        public sealed override void SetupContent() => SetStaticDefaults();

        void IManualParticleRenderer.RegisterRenderer() => ParticleManager.RegisterRenderer<TParticleType>(this);

        void IManualParticleRenderer.AddParticle(Particle particle)
        {
            if (particle is TParticleType castedParticle)
                Particles.Add(castedParticle);
        }

        void IManualParticleRenderer.RemoveParticle(Particle particle)
        {
            if (particle is TParticleType castedParticle)
                Particles.Remove(castedParticle);
        }

        void IManualParticleRenderer.RenderParticles() => RenderParticles();

        /// <summary>
        /// Iterate over <see cref="Particles"/> here and perform rendering logic.
        /// </summary>
        public abstract void RenderParticles();
    }
}
