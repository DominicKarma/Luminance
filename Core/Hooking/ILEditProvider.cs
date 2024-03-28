using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Hooking
{
    /// <summary>
    /// A basic provider class for wrapping around a single <see cref="ManagedILEdit"/>. If you need to do multiple in one class, use a <see cref="ModSystem"/> or similar.
    /// </summary>
    public abstract class ILEditProvider : ModType
    {
        public sealed override void Load()
        {
            Main.QueueMainThreadAction(() =>
            {
                new ManagedILEdit(Name, Mod, Subscribe, Unsubscribe, PerformEdit).Apply();
            });
        }

        protected sealed override void Register() => ModTypeLookup<ILEditProvider>.Register(this);

        public sealed override void SetupContent() => SetStaticDefaults();

        /// <summary>
        /// Subscribe <see cref="ManagedILEdit.SubscriptionWrapper"/> to your IL event here.
        /// </summary>
        public abstract void Subscribe(ManagedILEdit edit);

        /// <summary>
        /// Unsubscribe <see cref="ManagedILEdit.SubscriptionWrapper"/> to your IL event here.
        /// </summary>
        public abstract void Unsubscribe(ManagedILEdit edit);

        /// <summary>
        /// Perform the actual IL edit here. Use the provided ManagedILEdit's log method if something goes wrong.
        /// </summary>
        public abstract void PerformEdit(ILContext il, ManagedILEdit edit);
    }
}
