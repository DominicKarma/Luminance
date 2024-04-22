using System.Collections.Generic;
using Terraria.ModLoader;

namespace Luminance.Core.MenuInfoUI
{
    public abstract class InfoUIManager : ModType
    {
        public virtual IEnumerable<PlayerInfoIcon> GetPlayerInfoIcons() => [];

        public virtual IEnumerable<WorldInfoIcon> GetWorldInfoIcons() => [];

        protected sealed override void Register()
        {
            ModTypeLookup<InfoUIManager>.Register(this);
            InternalInfoUIManager.RegisterManager(this);
        }

        public sealed override void SetupContent() => SetStaticDefaults();

        public sealed override bool IsLoadingEnabled(Mod mod) => true;
    }
}
