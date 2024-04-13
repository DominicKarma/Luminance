namespace Luminance.Core.Hooking
{
    /// <summary>
    /// Provides a class with automanaged implementation of creating and subscribing to a new detour(s).
    /// </summary>
    public interface ICustomDetourProvider
    {
        /// <summary>
        /// Call <see cref="HookHelper.ModifyMethodWithDetour"/> here to implement your custom detour(s).
        /// </summary>
        void ModifyMethods();
    }
}
