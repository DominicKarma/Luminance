using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;

namespace Luminance.Core.Graphics
{
    public class ManagedScreenFilter : IDisposable
    {
        /// <summary>
        /// All deferred textures that should be applied when the filter's shader is applied.
        /// </summary>
        private readonly Dictionary<int, DeferredTexture> DeferredTextures = [];

        /// <summary>
        /// A managed copy of all parameter data. Used to minimize excess SetValue calls, in cases where the value aren't actually being changed.
        /// </summary>
        internal readonly Dictionary<string, object> parameterCache;

        public Ref<Effect> Shader
        {
            get;
            internal set;
        }

        public Effect WrappedEffect => Shader.Value;

        public bool Disposed
        {
            get;
            private set;
        }

        public bool IsActive
        {
            get;
            private set;
        }

        /// <summary>
        /// The opacity of the filter. As long as this is above 0, the filter will be applied for that frame.
        /// </summary>
        public float Opacity
        {
            get;
            private set;
        }

        public Vector2 FocusPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// The standard parameter name prefix for texture sizes.
        /// </summary>
        public const string TextureSizeParameterPrefix = "textureSize";

        /// <summary>
        /// Represents a texture that is supplied to a filter when its shader is ready to be applied.
        /// </summary>
        /// <param name="Texture">The texture to use.</param>
        /// <param name="Index">The index in the <see cref="GraphicsDevice.Textures"/> array that the texture should go in.</param>
        /// <param name="SamplerState">An optional sampler state that should be used alongside the texture. Does nothing if <see langword="null"/>.</param>
        public record DeferredTexture(Texture2D Texture, int Index, SamplerState SamplerState);

        /// <summary>
        /// A wrapper class for <see cref="Effect"/> that is focused around screen filter effects.
        /// </summary>
        internal ManagedScreenFilter(Ref<Effect> shader)
        {
            Shader = shader;

            // Initialize the parameter cache.
            parameterCache = [];
        }

        /// <summary>
        /// Sets "mainColor" to the provided value, if it exists.
        /// </summary>
        /// <param name="color"></param>
        public ManagedScreenFilter SetMainColor(Color color)
        {
            WrappedEffect.Parameters["mainColor"]?.SetValue(color.ToVector3());
            return this;
        }

        /// <summary>
        /// Sets "secondaryColor" to the provided value, if it exists.
        /// </summary>
        /// <param name="color"></param>
        public ManagedScreenFilter SetSecondaryColor(Color color)
        {
            WrappedEffect.Parameters["secondaryColor"]?.SetValue(color.ToVector3());
            return this;
        }

        public ManagedScreenFilter SetFocusPosition(Vector2 worldPosition)
        {
            FocusPosition = worldPosition;
            return this;
        }

        internal bool ParameterIsCachedAsValue(string parameterName, object value)
        {
            // If the parameter cache has not registered this parameter yet, that means it can't have changed, because there's nothing to compare against.
            // In this case, initialize the parameter in the cache for later.
            if (!parameterCache.TryGetValue(parameterName, out object parameter))
                return false;

            return parameter?.Equals(value) ?? false;
        }

        /// <summary>
        /// Attempts to send parameter data to the GPU for the filter to use.
        /// </summary>
        /// <param name="parameterName">The name of the parameter. This must correspond with the parameter name in the filter.</param>
        /// <param name="value">The value to supply to the parameter.</param>
        public bool TrySetParameter(string parameterName, object value)
        {
            // Shaders do not work on servers. If this method is called on one, terminate it immediately.
            if (Main.netMode == NetmodeID.Server)
                return false;

            // Check if the parameter even exists. If it doesn't, obviously do nothing else.
            EffectParameter parameter = Shader.Value.Parameters[parameterName];
            if (parameter is null)
                return false;

            // Check if the parameter value is already cached as the supplied value. If it is, don't waste resources informing the GPU of
            // parameter data, since nothing relevant has changed.
            if (ParameterIsCachedAsValue(parameterName, value))
                return false;

            // Store the value in the cache.
            parameterCache[parameterName] = value;

            // Unfortunately, there is no simple type upon which singles, integers, matrices, etc. can be converted in order to be sent to the GPU, and there is no
            // super easy solution for checking a parameter's expected type. FNA just messes with pointers under the hood and tosses back exceptions if that doesn't work.
            // Unless something neater arises, this conditional chain will do, I suppose.

            // Booleans.
            if (value is bool b)
            {
                parameter.SetValue(b);
                return true;
            }
            if (value is bool[] b2)
            {
                parameter.SetValue(b2);
                return true;
            }

            // Integers.
            if (value is int i)
            {
                parameter.SetValue(i);
                return true;
            }
            if (value is int[] i2)
            {
                parameter.SetValue(i2);
                return true;
            }

            // Floats.
            if (value is float f)
            {
                parameter.SetValue(f);
                return true;
            }
            if (value is float[] f2)
            {
                parameter.SetValue(f2);
                return true;
            }

            // Vector2s.
            if (value is Vector2 v2)
            {
                parameter.SetValue(v2);
                return true;
            }
            if (value is Vector2[] v22)
            {
                parameter.SetValue(v22);
                return true;
            }

            // Vector3s.
            if (value is Vector3 v3)
            {
                parameter.SetValue(v3);
                return true;
            }
            if (value is Vector3[] v32)
            {
                parameter.SetValue(v32);
                return true;
            }

            // Colors.
            if (value is Color c)
            {
                parameter.SetValue(c.ToVector3());
                return true;
            }

            // Vector4s.
            if (value is Vector4 v4)
            {
                parameter.SetValue(v4);
                return true;
            }
            if (value is Rectangle rect)
            {
                parameter.SetValue(new Vector4(rect.X, rect.Y, rect.Width, rect.Height));
                return true;
            }
            if (value is Vector4[] v42)
            {
                parameter.SetValue(v42);
                return true;
            }

            // Matrices.
            if (value is Matrix m)
            {
                parameter.SetValue(m);
                return true;
            }
            if (value is Matrix[] m2)
            {
                parameter.SetValue(m2);
                return true;
            }

            // Textures, for if those are explicitly designed as parameters.
            if (value is Texture2D t)
            {
                parameter.SetValue(t);
                return true;
            }

            // None of the condition cases were met, and something went wrong.
            return false;
        }

        /// <summary>
        /// Sets a texture at a given index for this filter to use based a the <see cref="Asset{T}"/> wrapper. Typically, index 0 is populated with whatever was passed into a <see cref="SpriteBatch"/>.Draw call.
        /// </summary>
        /// 
        /// <remarks>
        /// The texture is cached rather than used immediately, since it can only be applied properly when the filter is being used (a.k.a during screen shader rendering).
        /// </remarks>
        /// 
        /// <param name="textureAsset">The asset that contains the texture to supply.</param>
        /// <param name="textureIndex">The index to place the texture in.</param>
        /// <param name="samplerStateOverride">Which sampler should be used for the texture.</param>
        public void SetTexture(Asset<Texture2D> textureAsset, int textureIndex, SamplerState samplerStateOverride = null)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Texture2D texture = textureAsset.Value;
            SetTexture(texture, textureIndex, samplerStateOverride);
        }

        /// <summary>
        ///     Sets a texture at a given index for this shader to use. Typically, index 0 is populated with whatever was passed into a <see cref="SpriteBatch"/>.Draw call.
        /// </summary>
        /// <param name="texture">The texture to supply.</param>
        /// <param name="textureIndex">The index to place the texture in.</param>
        /// <param name="samplerStateOverride">Which sampler should be used for the texture.</param>
        public void SetTexture(Texture2D texture, int textureIndex, SamplerState samplerStateOverride = null)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            DeferredTexture deferredTexture = new(texture, textureIndex, samplerStateOverride);
            DeferredTextures[textureIndex] = deferredTexture;
        }

        /// <summary>
        /// Call to indicate that the filter should be active. This needs to happen each frame it should be active for.
        /// </summary>
        public void Activate() => IsActive = true;

        /// <summary>
        /// Automatically called at the end of each update, after updating the filter.
        /// </summary>
        public void Deactivate() => IsActive = false;

        public void Update()
        {
            if (IsActive)
                Opacity = Clamp(Opacity + 0.015f, 0f, 1f);
            else
                Opacity = Clamp(Opacity - 0.015f, 0f, 1f);
        }

        /// <summary>
        /// Apply the filter.
        /// </summary>
        /// <param name="setCommonParams">If true, this will automatically try to set certain parameters in the shader, such as globalTime.</param>
        /// <param name="pass">Specify a specific pass to use, if the shader has multiple.</param>
        public void Apply(bool setCommonParams = true, string pass = null)
        {
            // Apply commonly used parameters.
            if (setCommonParams)
                SetCommonParameters();

            SupplyDeferredTextures();

            WrappedEffect.CurrentTechnique.Passes[pass ?? ManagedShader.DefaultPassName].Apply();
        }

        private void SupplyDeferredTextures()
        {
            var gd = Main.instance.GraphicsDevice;

            foreach (DeferredTexture textureWrapper in DeferredTextures.Values)
            {
                int textureIndex = textureWrapper.Index;
                Texture2D texture = textureWrapper.Texture;
                SamplerState samplerStateOverride = textureWrapper.SamplerState;
                WrappedEffect.Parameters[$"{TextureSizeParameterPrefix}{textureIndex}"]?.SetValue(texture.Size());

                gd.Textures[textureIndex] = texture;
                if (samplerStateOverride is not null)
                    gd.SamplerStates[textureIndex] = samplerStateOverride;
            }
        }

        private void SetCommonParameters()
        {
            WrappedEffect.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            WrappedEffect.Parameters["opacity"]?.SetValue(Opacity);
            WrappedEffect.Parameters["focusPosition"]?.SetValue(FocusPosition);
            WrappedEffect.Parameters["screenPosition"]?.SetValue(Main.screenPosition);
            WrappedEffect.Parameters["screenSize"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            Disposed = true;
            Shader.Value.Dispose();
            parameterCache.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
