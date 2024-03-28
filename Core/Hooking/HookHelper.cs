using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Luminance.Core.Hooking
{
    public static class HookHelper
    {
        private static List<Hook> detours = [];

        private static List<ILHook> ilHooks = [];

        public static void ModifyMethodWithDetour(MethodBase methodToModify, Delegate detourMethod)
        {
            detours ??= [];
            Hook hook = new(methodToModify, detourMethod);
            hook.Apply();
            detours.Add(hook);
        }

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

            detours = null;
            ilHooks = null;
        }

        /// <summary>
        /// Does nothing, but is required for custom IL edit event implementations for unsubscribing.
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
