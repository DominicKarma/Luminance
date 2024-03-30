using System.Collections.Generic;
using Terraria.ModLoader;

namespace Luminance.Core.Balancing
{
    /// <summary>
    /// A class to supply balancing changes on mod loading.
    /// </summary>
    public abstract class BalancingManager : ModType
    {
        /// <summary>
        /// Return all relevant item balancing changes here. <br/>
        /// <b>Called automatically, do not call.</b>
        /// </summary>
        public virtual IEnumerable<ItemBalancingChange> GetItemBalancingChanges() => [];

        /// <summary>
        /// Return all relevant npc hit balancing changes here. <br/>
        /// <b>Called automatically, do not call.</b>
        /// </summary>
        public virtual IEnumerable<NPCHitBalancingChange> GetNPCHitBalancingChanges() => [];

        /// <summary>
        /// Return all relevant npc hp balancing changes here. <br/>
        /// <b>Called automatically, do not call.</b>
        /// </summary>
        public virtual IEnumerable<NPCHPBalancingChange> GetNPCHPBalancingChanges() => [];

        protected override void Register()
        {
            ModTypeLookup<BalancingManager>.Register(this);
            InternalBalancingManager.RegisterManager(this);
        }

        public sealed override void SetupContent() => SetStaticDefaults();

        public sealed override bool IsLoadingEnabled(Mod mod) => true;
    }
}
