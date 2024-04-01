namespace Luminance.Common.StateMachines
{
    /// <summary>
    /// Represents an abstraction of a state within a <see cref="PushdownAutomata{StateWrapper, StateIdentifier}"/>, containing local information specific to the state, such as timers or switches, as it sits within the stack.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IState<T> where T : struct
    {
        /// <summary>
        /// The identifier for this state.
        /// </summary>
        public T Identifier
        {
            get;
            protected set;
        }

        /// <summary>
        /// A method called whenever this state is popped from the stack in the <see cref="PushdownAutomata{StateWrapper, StateIdentifier}"/>.
        /// </summary>
        public void OnPopped();
    }
}
