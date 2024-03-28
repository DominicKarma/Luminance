using Luminance.Core.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace Luminance.Core.Cutscenes
{
    public abstract class Cutscene : ModType
    {
        /// <summary>
        /// How long the cutscene has been active for.
        /// </summary>
        public int Timer
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the cutscene is active.
        /// </summary>
        public bool IsActive
        {
            get;
            internal set;
        }

        /// <summary>
        /// Set to true to force the cutscene to instantly end.
        /// </summary>
        public bool EndAbruptly
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// A 0-1 ratio of how far along its lifetime this cutscene is.
        /// </summary>
        public float LifetimeRatio => (float)Timer / CutsceneLength;

        /// <summary>
        /// The length of time the cutscene should be active for.
        /// </summary>
        public abstract int CutsceneLength { get; }

        /// <summary>
        /// An optional blocker condition to use for the cutscene. Returns <see cref="BlockerSystem.BlockCondition.None"/> by default.
        /// </summary>
        public virtual BlockerSystem.BlockCondition GetBlockCondition => BlockerSystem.BlockCondition.None;

        protected sealed override void Register() => ModTypeLookup<Cutscene>.Register(this);

        public sealed override void SetupContent() => SetStaticDefaults();

        /// <summary>
        /// Called when the cutscene begins.
        /// </summary>
        public virtual void OnBegin()
        {

        }

        /// <summary>
        /// Called when the cutscene ends.
        /// </summary>
        public virtual void OnEnd()
        {

        }

        /// <summary>
        /// Called each tick the cutscene is active. Runs at the end of the update.
        /// </summary>
        public virtual void Update()
        {

        }

        /// <summary>
        /// Use to modify <see cref="Main.screenPosition"/>
        /// </summary>
        public virtual void ModifyScreenPosition()
        {

        }

        /// <summary>
        /// Use to modify the transform matrix.
        /// </summary>
        public virtual void ModifyTransformMatrix(ref SpriteViewMatrix transform)
        {

        }

        /// <summary>
        /// Called after NPC drawing.
        /// </summary>
        public virtual void DrawToWorld(SpriteBatch spriteBatch)
        {

        }

        /// <summary>
        /// Called in <see cref="ScreenModifierManager"/> at layer <see cref="ScreenModifierManager.CutsceneLayer"/> (125). Draw to <paramref name="screen"/> last.
        /// </summary>
        public virtual void DrawWorld(SpriteBatch spriteBatch, RenderTarget2D screen)
        {

        }

        /// <summary>
        /// Called during PostDraw.
        /// </summary>
        public virtual void PostDraw(SpriteBatch spriteBatch)
        {

        }
    }
}
