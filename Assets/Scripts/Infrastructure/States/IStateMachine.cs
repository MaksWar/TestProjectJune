using Cysharp.Threading.Tasks;

namespace Infrastructure.States
{
    public interface IStateMachine
    { 
        UniTask Enter<TState>() where TState : class, IState;
        UniTask Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>;
        void RegisterState<TState>(TState state) where TState : IExitableState;
    }
}