namespace Luminance.Common.StateMachines
{
    /// <summary>
    /// An implementation of <see cref="IState{T}"/> that adds only a simple integer timer.
    /// </summary>
    /// <typeparam name="TStateIdentifier">The type that this state is associated with.</typeparam>
    /// <param name="identifier">The identifying value for this specific state.</param>
    public class EntityAIState<TStateIdentifier>(TStateIdentifier identifier) : IState<TStateIdentifier> where TStateIdentifier : struct
    {
        /// <summary>
        /// The identifier for this state.
        /// </summary>
        public TStateIdentifier Identifier
        {
            get;
            set;
        } = identifier;

        /// <summary>
        /// A local timer that exists for use by this state.
        /// </summary>
        public int Time;

        public void OnPopped()
        {
            Time = 0;
        }
    }
}
