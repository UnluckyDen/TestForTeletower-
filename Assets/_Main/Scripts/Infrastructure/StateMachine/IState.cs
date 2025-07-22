namespace _Main.Scripts.Infrastructure.StateMachine
{
    public interface IState
    {
        public void Enter();
        public void Update();
        public void Dispose();
    }
}