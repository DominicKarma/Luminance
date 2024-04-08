using System;
using System.Reflection;

namespace Luminance.Common.StateMachines
{
    /// <summary>
    /// Marks a method as assositated with the provided <typeparamref name="TStateIdentifier"/> for the purpose of automated state machine behavior linking.
    /// </summary>

    /// <param name="assosiatedState">The state to register this method as behavior for.</param>
    [AttributeUsage(AttributeTargets.Method)]
    public class AutoloadAsBehavior<TStateWrapper, TStateIdentifier>(TStateIdentifier assosiatedState) : Attribute where TStateWrapper : class, IState<TStateIdentifier> where TStateIdentifier : struct
    {
        /// <summary>
        /// The assosiated state of the method.
        /// </summary>
        public readonly TStateIdentifier AssosiatedState = assosiatedState;

        /// <summary>
        /// Fills the <paramref name="stateMachine"/>'s behaviors with all methods in the providedinstance that have this attribute.
        /// </summary>
        /// <typeparam name="TInstanceType">The type of the instance that will access the methods.</typeparam>
        /// <param name="stateMachine">The state machine to fill.</param>
        /// <param name="instance">The instance to access the methods with.</param>
        public static void FillStateMachineBehaviors<TInstanceType>(PushdownAutomata<TStateWrapper,TStateIdentifier> stateMachine, TInstanceType instance)
        {
            var methods = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
            if (methods == null || methods.Length == 0)
                return;

            foreach (var method in methods)
            {
                var autoloadAttribute = method.GetCustomAttribute<AutoloadAsBehavior<TStateWrapper, TStateIdentifier>>();
                if (autoloadAttribute != null)
                    stateMachine.RegisterStateBehavior(autoloadAttribute.AssosiatedState, () => method.Invoke(instance, null));
            }
        }
    }
}
