namespace Luminance.Common.StateMachines
{
    public class EntityAIState<T>(T identifier) : IState<T> where T : struct
    {
        public T Identifier
        {
            get;
            set;
        } = identifier;

        public int Time;

        public void OnPopped()
        {
            Time = 0;
        }
    }
}
