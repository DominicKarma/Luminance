using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
