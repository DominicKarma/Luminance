namespace Luminance.Common.StateMachines
{
    public interface IState<T> where T : struct
    {
        public T Identifier
        {
            get;
            protected set;
        }

        public void OnPopped();
    }
}
