namespace Luminance.Common.StateMachines
{
    /// <summary>
    /// Represents an abstraction of a state within a <see cref="PushdownAutomata{TStateWrapper, TStateIdentifier}"/>, containing local information specific to the state, such as timers or switches, as it sits within the stack.
    /// </summary>
    /// <typeparam name="TStateIdentifier"></typeparam>
    public interface IState<TStateIdentifier> where TStateIdentifier : struct
    {
        /// <summary>
        /// The identifier for this state.
        /// </summary>
        public TStateIdentifier Identifier
        {
            get;
            protected set;
        }

        /// <summary>
        /// A method called whenever this state is popped from the stack in the <see cref="PushdownAutomata{TStateWrapper, TStateIdentifier}"/>.
        /// </summary>
        public void OnPopped();
    }
}
