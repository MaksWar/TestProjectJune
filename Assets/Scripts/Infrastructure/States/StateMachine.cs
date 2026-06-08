using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Infrastructure.States
{
    public abstract class StateMachine : IStateMachine
    {
        private readonly Dictionary<Type, IExitableState> _registeredStates;
        private IExitableState _currentState;

        public StateMachine() => 
            _registeredStates = new Dictionary<Type, IExitableState>();

        public IExitableState CurrentState => _currentState;

        public async UniTask Enter<TState>() where TState : class, IState
        {
           TState newState = await ChangeState<TState>();
           await newState.Enter();
        }

        public async UniTask Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>
        {
            TState newState = await ChangeState<TState>();
            await newState.Enter(payload);
        }

        public void RegisterState<TState>(TState state) where TState : IExitableState => 
            _registeredStates.Add(typeof(TState), state);

        private async UniTask<TState> ChangeState<TState>() where TState : class, IExitableState
        {
            if (CurrentState != null)
                await CurrentState.Exit();
            
            TState state = GetState<TState>();
            _currentState = state;
            
            return state;
        }

        private TState GetState<TState>() where TState : class, IExitableState => 
            _registeredStates[typeof(TState)] as TState;
    }
}