using System;
using System.Collections.Generic;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Hooking
{
    public delegate void ManagedILManipulator(ILContext context, ManagedILEdit edit);

    /// <summary>
    /// Wrapper for ILEdits that automatically unapplies them all, and provides a useful error logging template.
    /// </summary>
    /// <param name="Name">The name of the edit.</param>
    /// <param name="AssosiatedMod">The mod that owns this ILEdit.</param>
    /// <param name="SubscriptionFunction">An action that subscribes the ILEdit.</param>
    /// <param name="UnsubscriptionFunction">An action that unsubscribes the ILEdit.</param>
    /// <param name="EditingFunction">The delagate that contains/represents the ILEdit.</param>
    public sealed record ManagedILEdit(string Name, Mod AssosiatedMod, Action<ManagedILEdit> SubscriptionFunction, Action<ManagedILEdit> UnsubscriptionFunction, ManagedILManipulator EditingFunction)
    {
        private static readonly Dictionary<string, List<ManagedILEdit>> EditsByMod = [];

        /// <summary>
        /// Exposes the editing function directly, this should be used in <see cref="ILEditProvider.Subscribe"/> and <see cref="ILEditProvider.Unsubscribe"/>
        /// </summary>
        /// <param name="context"></param>
        public void SubscriptionWrapper(ILContext context) => EditingFunction(context, this);

        /// <summary>
        /// Applies the ILEdits <see cref="EditingFunction"/>.
        /// </summary>
        /// <param name="onMainThread"></param>
        public void Apply(bool applyOnMainThread = false)
        {
            if (!applyOnMainThread)
            {
                SubscriptionFunction?.Invoke(this);
                return;
            }

            Main.QueueMainThreadAction(() =>
            {
                SubscriptionFunction?.Invoke(this);
            });

            CacheEdit(AssosiatedMod, this);
        }

        /// <summary>
        /// Provides a standardization for IL editing failure cases, making use of <see cref="Mod.Logger"/>.<br></br>
        /// This should be used if an IL edit could not be loaded for any reason, such as a <see cref="ILCursor.TryGotoNext(MoveType, System.Func{Mono.Cecil.Cil.Instruction, bool}[])"/> failure.
        /// </summary>
        /// <param name="reason">The reason that the IL edit failed.</param>
        public void LogFailure(string reason) => ModContent.GetInstance<Luminance>().Logger.Warn($"The IL edit of the name '{Name}' by {AssosiatedMod.DisplayName} failed to load for the following reason:\n{reason}");

        private static void CacheEdit(Mod mod, ManagedILEdit edit) => GetEditListSafely(mod.Name).Add(edit);

        private static List<ManagedILEdit> GetEditListSafely(string modName)
        {
            if (!EditsByMod.ContainsKey(modName))
                EditsByMod[modName] = [];
            return EditsByMod[modName];
        }

        internal static void UnloadEdits()
        {
            foreach (var edits in EditsByMod.Values)
            {
                foreach (var edit in edits)
                    edit.UnsubscriptionFunction?.Invoke(edit);
            }

            EditsByMod.Clear();
        }
    }
}
