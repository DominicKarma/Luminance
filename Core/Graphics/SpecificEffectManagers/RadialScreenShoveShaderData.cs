using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using KarmaLibrary.Core.Graphics.SpecificEffectManagers;

namespace KarmaLibrary.Core.Graphics.Shaders.Screen
{
    public class RadialScreenShoveShaderData(Ref<Effect> shader, string passName) : ScreenShaderData(shader, passName)
    {
        public const string ShaderKey = "KarmaLibrary:RadialScreenShove";

        public static void ToggleActivityIfNecessary()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            bool shouldBeActive = RadialScreenShoveSystem.DistortionPower >= 0.001f;
            if (shouldBeActive && !Filters.Scene[ShaderKey].IsActive())
            {
                Filters.Scene[ShaderKey].Opacity = 1f;
                Filters.Scene.Activate(ShaderKey);
            }
            if (!shouldBeActive && Filters.Scene[ShaderKey].IsActive())
            {
                Filters.Scene[ShaderKey].Opacity = 0f;
                Filters.Scene.Deactivate(ShaderKey);
            }
        }

        public override void Apply()
        {
            float distortionPower = RadialScreenShoveSystem.DistortionPower * 0.11f;
            Shader.Parameters["blurPower"].SetValue(0.3f);
            Shader.Parameters["pulseTimer"].SetValue(Main.GlobalTimeWrappedHourly * 21f);
            Shader.Parameters["distortionPower"].SetValue(Main.gamePaused ? 0f : distortionPower);
            Shader.Parameters["distortionCenter"].SetValue(WorldSpaceToScreenUV(RadialScreenShoveSystem.DistortionCenter));
            base.Apply();
        }
    }
}
