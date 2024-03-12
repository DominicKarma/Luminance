using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace KarmaLibrary.Core.Graphics.Automators
{
    [DebuggerDisplay("Width: {target?.Width ?? 0}, Height: {target?.Height ?? 0}, Uninitialized: {IsUninitialized}, Time since last usage: {TimeSinceLastUsage} frame(s)")]
    public class ManagedRenderTarget : IDisposable
    {
        private RenderTarget2D target;

        internal bool WaitingForFirstInitialization
        {
            get;
            private set;
        } = true;

        internal RenderTargetInitializationAction InitializationAction
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether this render target is uninitialized or not.
        /// </summary>
        public bool IsUninitialized => target is null || target.IsDisposed;

        /// <summary>
        /// How long it's been, in frames, since this render target was last used in some way.
        /// </summary>
        /// <remarks>
        /// This is based on calls to the <see cref="Target"/> property getter.
        /// </remarks>
        public int TimeSinceLastUsage
        {
            get;
            internal set;
        }

        /// <summary>
        /// Whether this render target is disposed or not.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether this render target should be reset when the screen size changes.
        /// </summary>
        public bool ShouldResetUponScreenResize
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether this render target should be subject to automatic garbage collection when not in use.
        /// </summary>
        public bool SubjectToGarbageCollection
        {
            get;
            private set;
        }

        /// <summary>
        /// The raw <see cref="RenderTarget2D"/> this wrapper holds.
        /// </summary>
        public RenderTarget2D Target
        {
            get
            {
                TimeSinceLastUsage = 0;
                if (IsUninitialized)
                {
                    target = InitializationAction(Main.screenWidth, Main.screenHeight);
                    WaitingForFirstInitialization = false;
                }

                return target;
            }
            private set => target = value;
        }

        /// <summary>
        /// The width of the render target.
        /// </summary>
        public int Width => Target.Width;

        /// <summary>
        /// The height of the render target.
        /// </summary>
        public int Height => Target.Height;

        public delegate RenderTarget2D RenderTargetInitializationAction(int screenWidth, int screenHeight);

        public ManagedRenderTarget(bool shouldResetUponScreenResize, RenderTargetInitializationAction creationCondition, bool subjectToGarbageCollection = true)
        {
            ShouldResetUponScreenResize = shouldResetUponScreenResize;
            InitializationAction = creationCondition;
            SubjectToGarbageCollection = subjectToGarbageCollection;
            RenderTargetManager.ManagedTargets.Add(this);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            target?.Dispose();
            TimeSinceLastUsage = 0;
            GC.SuppressFinalize(this);
        }

        public void Recreate(int screenWidth, int screenHeight)
        {
            Dispose();
            IsDisposed = false;
            TimeSinceLastUsage = 0;

            target = InitializationAction(screenWidth, screenHeight);
        }

        // These extension methods don't apply to ManagedRenderTarget instances, even with the implicit conversion operator. As such, it is implemented manually.
        public Vector2 Size() => Target.Size();

        public void SwapToRenderTarget(Color? flushColor = null) => Target.SwapToRenderTarget(flushColor);

        public void CopyContentsFrom(RenderTarget2D from)
        {
            Main.instance.GraphicsDevice.SetRenderTarget(Target);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Main.spriteBatch.Draw(from, Vector2.Zero, null, Color.White);
            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(from);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);
            Main.instance.GraphicsDevice.SetRenderTarget(null);
        }

        // This allows for easy shorthand conversions from ManagedRenderTarget to RenderTarget2D without having to manually type out ManagedTarget.Target all the time.
        // This is functionally equivalent to accessing the getter manually and will activate all of the relevant checks within said getter.
        public static implicit operator RenderTarget2D(ManagedRenderTarget targetWrapper) => targetWrapper.Target;
    }
}
