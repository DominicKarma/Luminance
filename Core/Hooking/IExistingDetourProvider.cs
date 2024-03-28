using Terraria.ModLoader;

namespace Luminance.Core.Hooking
{
    /// <summary>
    /// Provides a class with automanaged implementation of an existing tModLoader detour(s).
    /// </summary>
    public interface IExistingDetourProvider : ILoadable
    {
        void ILoadable.Load(Mod mod) => Subscribe();

        void ILoadable.Unload() => Unsubscribe();

        /// <summary>
        /// Subscribe to the detour here.
        /// </summary>
        void Subscribe();

        /// <summary>
        /// Unsubscribe to the detour here.
        /// </summary>
        void Unsubscribe();
    }
}
