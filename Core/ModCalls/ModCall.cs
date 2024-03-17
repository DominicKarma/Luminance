using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Luminance.Core.ModCalls
{
    public abstract class ModCall : ILoadable
    {
        ///<summary>
        /// Once a call makes it to a public version, NEVER delete it from here.
        /// </summary>
        public abstract IEnumerable<string> CallCommands
        {
            get;
        }

        /// <summary>
        /// The ordered types that the args must be. Set as null if none are needed.
        /// </summary>
        public abstract IEnumerable<Type> InputTypes
        {
            get;
        }

        // WHY doesnt ILoadable.Unload() pass the mod??
        public Mod AssosiatedMod
        {
            get;
            internal set;
        }

        /// <summary>
        /// Processes the modcall, checking that all the parameters match and throws if not.
        /// </summary>
        /// <param name="argsWithoutCommand"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        internal object ProcessInternal(params object[] argsWithoutCommand)
        {
            // If null, no input types are required so skip to processing.
            if (InputTypes == null)
                return SafeProcess(argsWithoutCommand);

            IEnumerable<Type> expectedInputTypes = InputTypes;
            int expectedInputCount = expectedInputTypes.Count();

            if (argsWithoutCommand.Length != expectedInputCount)
                throw new ArgumentException($"The inputted arguments for the '{GetType()}' mod call were of an invalid length! {argsWithoutCommand.Length} arguments were inputted, {expectedInputCount} were expected.");

            for (int i = 0; i < argsWithoutCommand.Length; i++)
            {
                // i + 1 is used because the 0th argument (aka the mod call command) isn't included in this method.
                Type expectedType = expectedInputTypes.ElementAt(i);
                if (argsWithoutCommand[i].GetType() != expectedType)
                    throw new ArgumentException($"Argument {i + 1} was invalid for the '{GetType()}' mod call! It was of type '{argsWithoutCommand[i].GetType()}', but '{expectedType}' was expected.");
            }

            return SafeProcess(argsWithoutCommand);
        }

        // Feel free to assume that the argument types are valid when setting up mod calls.
        // Any cases where they wouldn't be should be neatly handled via ProcessInternal's error handling.
        /// <summary>
        /// Process the mod call here. Return <see cref="ModCallManager.DefaultObject"/> instead of null if no other return value is suitable.
        /// </summary>
        /// <param name="argsWithoutCommand"></param>
        /// <returns></returns>
        protected abstract object SafeProcess(params object[] argsWithoutCommand);

        public void Load(Mod mod)
        {
            AssosiatedMod = mod;
            ModCallManager.RegisterModCall(mod, this);
        }

        public void Unload() => ModCallManager.RemoveModCall(AssosiatedMod, this);
    }
}
