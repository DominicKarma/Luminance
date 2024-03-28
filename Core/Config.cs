using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Luminance.Core
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Slider]
        [DefaultValue(2500)]
        [Range(100, 5000)]
        public int MaxParticles { get; set; }

        [Slider]
        [DefaultValue(100)]
        [Range(0, 100)]
        public int ScreenshakeModifier { get; set; }
    }
}
