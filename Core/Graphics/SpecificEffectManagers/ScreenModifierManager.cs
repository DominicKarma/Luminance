using System.Collections.Generic;
using System.Linq;
using Luminance.Core.Cutscenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public class ScreenModifierManager : ModSystem
    {
        private record ScreenModifierInfo(ScreenTargetModifierDelegate Info, byte Layer);

        public delegate void ScreenTargetModifierDelegate(RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor);

        private static List<ScreenModifierInfo> screenModifiers;

        /// <summary>
        /// The layer of cutscenes in the modifiers.
        /// </summary>
        public const byte CutsceneLayer = 100;

        /// <summary>
        /// The layer of screen filters in the modifiers.
        /// </summary>
        public const byte FilterLayer = 200;

        public override void Load()
        {
            On_FilterManager.EndCapture += EndCaptureDetour;
            screenModifiers = [];
            RegisterScreenModifier(CutsceneManager.DrawWorld, CutsceneLayer);
            RegisterScreenModifier(ShaderManager.ApplyScreenFilters, FilterLayer);
        }

        public override void Unload()
        {
            On_FilterManager.EndCapture -= EndCaptureDetour;
            screenModifiers.Clear();
        }

        /// <summary>
        /// Call to register a screen modifier delegate at the provided layer. Each registered modifier is ran in ascending layer order.
        /// </summary>
        public static void RegisterScreenModifier(ScreenTargetModifierDelegate screenTargetModifierDelegate, byte layer)
        {
            if (Main.dedServ)
                return;

            screenModifiers.Add(new(screenTargetModifierDelegate, layer));

            if (screenModifiers.Count > 1)
                screenModifiers = [.. screenModifiers.OrderBy(element => element.Layer)];
        }

        private void EndCaptureDetour(On_FilterManager.orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
        {
            foreach (var screenModifier in screenModifiers)
                screenModifier.Info(finalTexture, screenTarget1, screenTarget2, clearColor);

            orig(self, finalTexture, screenTarget1, screenTarget2, clearColor);
        }
    }
}
