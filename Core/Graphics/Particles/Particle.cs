using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public abstract class Particle
    {
        #region Instance Members
        /// <summary>
        /// The texture of the particle.
        /// </summary>
        public AtlasTexture Texture
        {
            get;
            private set;
        }

        /// <summary>
        /// The texture name of this particle on the particle atlas. Should be prefixed with "YourModName."
        /// </summary>
        public abstract string AtlasTextureName { get; }

        /// <summary>
        /// Whether the particle is manually drawn.
        /// </summary>
        internal bool ManuallyDrawn;

        /// <summary>
        /// The position of the particle.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The velocity of the particle.
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// The scale of the particle.
        /// </summary>
        public Vector2 Scale;

        /// <summary>
        /// The draw color of the particle.
        /// </summary>
        public Color DrawColor;

        /// <summary>
        /// The frame of the particle.
        /// </summary>
        public Rectangle? Frame;

        /// <summary>
        /// The rotation of the particle.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The rotation speed of the particle.
        /// </summary>
        public float RotationSpeed;

        /// <summary>
        /// The opacity of the particle.
        /// </summary>
        public float Opacity;

        /// <summary>
        /// The time the particle has existed for.
        /// </summary>
        public int Time;

        /// <summary>
        /// The maximum lifetime of the particle, in seconds.
        /// </summary>
        public int Lifetime;

        /// <summary>
        /// The direction of the particle.
        /// </summary>
        public int Direction;

        /// <summary>
        /// A 0-1 interlopant of how far along its lifetime the particle is.
        /// </summary>
        public float LifetimeRatio => Time / (float)Lifetime;

        /// <summary>
        /// The blend state to draw the particle with. Defaults to <see cref="BlendState.AlphaBlend"/>.
        /// </summary>
        public virtual BlendState BlendState => BlendState.AlphaBlend;

        /// <summary>
        /// How many frames this particle has in its standard texture. Defaults to 1.
        /// </summary>
        public virtual int FrameCount => 1;

        /// <summary>
        /// Spawns the particle into the world.
        /// </summary>
        /// <returns></returns>
        public Particle Spawn()
        {
            // Initialize the life timer.
            Time = new();

            if (ParticleManager.ActiveParticles.Count > ModContent.GetInstance<Config>().MaxParticles)
                ParticleManager.ActiveParticles.First().Kill();

            if (ParticleManager.ManualRenderers.ContainsKey(GetType()))
                ManuallyDrawn = true;

            ParticleManager.ActiveParticles.Add(this);
            ParticleManager.AddToDrawList(this);

            Texture = AtlasManager.GetTexture(AtlasTextureName);
            return this;
        }

        /// <summary>
        /// Immediately destroys this particle next update.
        /// </summary>
        public void Kill() => Time = Lifetime;

        /// <summary>
        /// Override to run custom update code for the particle. Does nothing by default.
        /// </summary>
        public virtual void Update()
        {

        }

        /// <summary>
        /// Override to run custom drawcode for the particle. Draws the particle texture to the screen by default.
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch) => spriteBatch.Draw(Texture, Position - Main.screenPosition, Frame, DrawColor, Rotation, null, Scale, Direction.ToSpriteDirection());
        #endregion
    }
}
