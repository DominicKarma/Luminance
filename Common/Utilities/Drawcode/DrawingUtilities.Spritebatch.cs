using Luminance.Core.Graphics;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Microsoft.Xna.Framework;

namespace Luminance.Common.Utilities
{
    public static partial class Utilities
    {
        private static BlendState subtractiveBlending;

        private static RasterizerState cullClockwiseAndScreen;

        private static RasterizerState cullCounterclockwiseAndScreen;

        private static RasterizerState cullOnlyScreen;

        /// <summary>
        /// A blend state that works opposite to <see cref="BlendState.Additive"/>, making colors darker based on intensity rather than brighter.
        /// </summary>
        public static BlendState SubtractiveBlending
        {
            get
            {
                return subtractiveBlending ??= new()
                {
                    ColorSourceBlend = Blend.SourceAlpha,
                    ColorDestinationBlend = Blend.One,
                    ColorBlendFunction = BlendFunction.ReverseSubtract,
                    AlphaSourceBlend = Blend.SourceAlpha,
                    AlphaDestinationBlend = Blend.One,
                    AlphaBlendFunction = BlendFunction.ReverseSubtract
                };
            }
        }

        public static RasterizerState CullClockwiseAndScreen
        {
            get
            {
                if (cullClockwiseAndScreen is null)
                {
                    cullClockwiseAndScreen = RasterizerState.CullClockwise;
                    cullClockwiseAndScreen.ScissorTestEnable = true;
                }

                return cullClockwiseAndScreen;
            }
        }

        public static RasterizerState CullCounterclockwiseAndScreen
        {
            get
            {
                if (cullCounterclockwiseAndScreen is null)
                {
                    cullCounterclockwiseAndScreen = RasterizerState.CullCounterClockwise;
                    cullCounterclockwiseAndScreen.ScissorTestEnable = true;
                }

                return cullCounterclockwiseAndScreen;
            }
        }

        public static RasterizerState CullOnlyScreen
        {
            get
            {
                if (cullOnlyScreen is null)
                {
                    cullOnlyScreen = RasterizerState.CullNone;
                    cullOnlyScreen.ScissorTestEnable = true;
                }

                return cullOnlyScreen;
            }
        }

        public static RasterizerState DefaultRasterizerScreenCull => Main.gameMenu || Main.LocalPlayer.gravDir == 1f ? CullCounterclockwiseAndScreen : CullClockwiseAndScreen;

        /// <summary>
        /// Resets a sprite batch with a desired <see cref="BlendState"/>. The <see cref="SpriteSortMode"/> is specified as <see cref="SpriteSortMode.Deferred"/>. If <see cref="SpriteSortMode.Immediate"/> is needed, use <see cref="PrepareForShaders"/> instead.
        /// <br></br>
        /// Like any sprite batch resetting function, use this sparingly. Overusage (such as performing this operation multiple times per frame) will lead to significantly degraded performance on weaker systems.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="newBlendState">The desired blend state.</param>
        public static void UseBlendState(this SpriteBatch spriteBatch, BlendState newBlendState)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, newBlendState, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Resets the sprite batch with <see cref="SpriteSortMode.Immediate"/> blending, along with an optional <see cref="BlendState"/>. For use when shaders are necessary.
        /// <br></br>
        /// Like any sprite batch resetting function, use this sparingly. Overusage (such as performing this operation multiple times per frame) will lead to significantly degraded performance on weaker devices.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="newBlendState">An optional blend state. If none is supplied, <see cref="BlendState.AlphaBlend"/> is used.</param>
        /// <param name="ui">Whether this is for UI drawing or not. Controls what matrix is used.</param>
        public static void PrepareForShaders(this SpriteBatch spriteBatch, BlendState newBlendState = null, bool ui = false)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, newBlendState ?? BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, ui ? Main.UIScaleMatrix : Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Resets the sprite batch to its 'default' state relative to most effects in the game, with a default blend state and sort mode. For use after the sprite batch state has been altered and needs to be reset.
        /// <br></br>
        /// Like any sprite batch resetting function, use this sparingly. Overusage (such as performing this operation multiple times per frame) will lead to significantly degraded performance on weaker systems.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="end">Whether to call <see cref="SpriteBatch.End"/> first and flush the contents of the previous draw batch. Defaults to true.</param>
        public static void ResetToDefault(this SpriteBatch spriteBatch, bool end = true)
        {
            if (end)
                spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Resets the sprite batch to its 'default' state relative to the UI, with a default blend state and sort mode. For use after the sprite batch state has been altered and needs to be reset.
        /// <br></br>
        /// Like any sprite batch resetting function, use this sparingly. Overusage (such as performing this operation multiple times per frame) will lead to significantly degraded performance on weaker systems.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="end">Whether to call <see cref="SpriteBatch.End"/> first and flush the contents of the previous draw batch. Defaults to true.</param>
        public static void ResetToDefaultUI(this SpriteBatch spriteBatch, bool end = true)
        {
            if (end)
                spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }

        /// <summary>
        /// Prepares a specialized <see cref="RasterizerState"/> with enabled screen culling, for efficiency reasons. It also informs the <see cref="GraphicsDevice"/> of this change.
        /// </summary>
        public static RasterizerState PrepareScreenCullRasterizer()
        {
            // Apply the screen culling.
            Main.instance.GraphicsDevice.ScissorRectangle = new(-2, -2, Main.screenWidth + 4, Main.screenHeight + 4);
            return DefaultRasterizerScreenCull;
        }

        /// <summary>
        /// Draws an atlas texture to the screen.
        /// </summary>
        public static void Draw(this SpriteBatch spriteBatch, AtlasTexture atlasTexture, Vector2 position, Rectangle? frame, Color color, float rotation = 0f, Vector2? origin = null, Vector2? scale = null, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            // Initialize default values where necessary.
            frame ??= atlasTexture.Frame;
            scale ??= Vector2.One;

            // Clamp the framing to the atlas element. It should NOT go outside of its bounds on the atlas.
            float xValue = Clamp(frame.Value.X, atlasTexture.Frame.X, atlasTexture.Frame.X + atlasTexture.Frame.Width);
            float yValue = Clamp(frame.Value.Y, atlasTexture.Frame.Y, atlasTexture.Frame.Y + atlasTexture.Frame.Height);
            float width = MathF.Min(atlasTexture.Frame.Width - frame.Value.X + atlasTexture.Frame.X, Clamp(frame.Value.Width, 0f, atlasTexture.Frame.Width));
            float height = MathF.Min(atlasTexture.Frame.Height - frame.Value.Y + atlasTexture.Frame.Y, Clamp(frame.Value.Height, 0f, atlasTexture.Frame.Height));
            Rectangle finalFrame = new((int)xValue, (int)yValue, (int)width, (int)height);

            // The origin must be moved to the appropriate atlas position if user defined.
            if (origin != null)
                origin += new Vector2(atlasTexture.Frame.X, atlasTexture.Frame.Y);
            // Else, initialize it to the center of the atlas texture frame.
            else
                origin ??= finalFrame.Size() * 0.5f;

            spriteBatch.Draw(atlasTexture.Atlas.Texture.Value, position, finalFrame, color, rotation, origin.Value, scale.Value, spriteEffects, 0f);
        }
    }
}
