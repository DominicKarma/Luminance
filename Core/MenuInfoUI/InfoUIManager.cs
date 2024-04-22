using System.Collections.Generic;
using Terraria.ModLoader;

namespace Luminance.Core.MenuInfoUI
{
    /// <summary>
    /// A class to supply all information icons for your mod on loading.
    /// </summary>
    public abstract class InfoUIManager : ModType
    {
        /// <summary>
        /// Return all player info icons here.<br/>
        /// <b>Called automatically, do not call.</b>
        /// </summary>
        public virtual IEnumerable<PlayerInfoIcon> GetPlayerInfoIcons() => [];

        /// <summary>
        /// Return all world info icons here.<br/>
        /// <b>Called automatically, do not call.</b>
        /// </summary>
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
