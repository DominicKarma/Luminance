using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace Luminance.Core.Hooking
{
    /// <summary>
    /// Provides useful methods for working with IL editing and custom detouring.
    /// </summary>
    public static class HookHelper
    {
        private static List<Hook> detours = [];

        private static List<ILHook> ilHooks = [];

        private static List<IExistingDetourProvider> existingDetourProviders = [];

        internal static void LoadHookInterfaces(Mod mod)
        {
            var existingDetourProvidersColection = GetEveryTypeDerivedFrom(typeof(IExistingDetourProvider), mod.Code).ToList();
            var customDetourProvidersCollection = GetEveryTypeDerivedFrom(typeof(ICustomDetourProvider), mod.Code).ToList();

            existingDetourProviders ??= [];
            foreach (var type in existingDetourProvidersColection)
            {
                var detour = (IExistingDetourProvider)RuntimeHelpers.GetUninitializedObject(type);
                detour.Subscribe();
                existingDetourProviders.Add(detour);
            }

            foreach (var type in customDetourProvidersCollection)
            {
                var detour = (ICustomDetourProvider)RuntimeHelpers.GetUninitializedObject(type);
                detour.ModifyMethods();
            }
        }

        /// <summary>
        /// Modifies the provided methodbase with the provided detour, and caches it. This is automatically undone on unloading.
        /// </summary>
        public static void ModifyMethodWithDetour(MethodBase methodToModify, Delegate detourMethod)
        {
            detours ??= [];
            Hook hook = new(methodToModify, detourMethod);
            hook.Apply();
            detours.Add(hook);
        }

        /// <summary>
        /// Modifies the provided methodbase with the provided IL manipulator, and caches it. This is automatically undone on unloading.
        /// </summary>
        public static void ModifyMethodWithIL(MethodBase methodToModify, ILContext.Manipulator ilMethod)
        {
            ilHooks ??= [];
            ILHook hook = new(methodToModify, ilMethod);
            hook.Apply();
            ilHooks.Add(hook);
        }

        internal static void UnloadHooks()
        {
            foreach (var hook in detours)
                hook.Undo();

            foreach (var hook in ilHooks)
                hook.Undo();

            foreach (var detour in existingDetourProviders)
                detour.Unsubscribe();

            detours = null;
            ilHooks = null;
            existingDetourProviders = null;
        }

        /// <summary>
        /// Does nothing, existing solely for the purpose of unsubscription in custom IL edit event implementations that use add/remove event syntax.
        /// </summary>
        public static void ILEventRemove()
        {

        }

        /// <summary>
        /// A generic IL edit that simply immediately emits a return.
        /// </summary>
        /// <param name="il"></param>
        /// <param name="_"></param>
        public static void EarlyReturnEdit(ILContext il, ManagedILEdit _)
        {
            ILCursor cursor = new(il);
            cursor.Emit(OpCodes.Ret);
        }
    }
}
