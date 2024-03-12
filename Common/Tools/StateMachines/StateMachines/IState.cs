namespace KarmaLibrary.Common.Tools.StateMachines
{
    public interface IState<T> where T : struct
    {
        public T Identifier
        {
            get;
            protected set;
        }

        public void OnPoppedFromStack();
    }
}
