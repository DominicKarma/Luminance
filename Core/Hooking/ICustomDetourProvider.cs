using Terraria.ModLoader;

namespace Luminance.Core.Hooking
{
    /// <summary>
    /// Provides a class with automanaged implementation of creating and subscribing to a new detour(s).
    /// </summary>
    public interface ICustomDetourProvider : ILoadable
    {
        void ILoadable.Load(Mod mod) => ModifyMethods();

        void ILoadable.Unload() { }

        /// <summary>
        /// Call <see cref="HookHelper.ModifyMethodWithDetour"/> here to implement your custom detour(s).
        /// </summary>
        void ModifyMethods();
    }
}
