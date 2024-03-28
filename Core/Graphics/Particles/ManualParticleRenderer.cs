using System.Collections.Generic;
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
    /// Provides direct access to the assosiated particle's draw collection, allowing for more control about how they are rendered.
    /// </summary>
    /// <remarks>
    /// Particles using this will not be automatically renderered.
    /// </remarks>
    /// <typeparam name="TParticleType">The particle type that is assosiated with this renderer.</typeparam>
    public abstract class ManualParticleRenderer<TParticleType> : ModType, IManualParticleRenderer where TParticleType : Particle
    {
        protected List<TParticleType> Particles = [];

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
        /// Iterate over <see cref="Particles"/> here and perform rendering logic.<br/>
        /// <b>Note that no spritebatch has been started.</b>
        /// </summary>
        public abstract void RenderParticles();
    }
}
