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

        void Subscribe();

        void Unsubscribe();
    }
}
