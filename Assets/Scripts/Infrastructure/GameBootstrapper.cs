using Cysharp.Threading.Tasks;
using Infrastructure.Factories;
using Infrastructure.States;
using UnityEngine;
using Zenject;

namespace Infrastructure
{
    public class GameBootstrapper : MonoBehaviour
    {
        private GameStateMachine _gameStateMachine;
        private StatesFactory _statesFactory;

        [Inject]
        public void Construct(
            GameStateMachine gameStateMachine,
            StatesFactory statesFactory
        )
        {
            _gameStateMachine = gameStateMachine;
            _statesFactory = statesFactory;
        }

        private void Start()
        {
            _gameStateMachine.RegisterState(_statesFactory.Create<GameBootstrapState>());
            _gameStateMachine.RegisterState(_statesFactory.Create<GameLoadDataState>());
            _gameStateMachine.RegisterState(_statesFactory.Create<GameplayLoadState>());
            _gameStateMachine.RegisterState(_statesFactory.Create<GameplayState>());
            _gameStateMachine.RegisterState(_statesFactory.Create<LevelsMenuLoadState>());
            _gameStateMachine.RegisterState(_statesFactory.Create<LevelsMenuState>());

            _gameStateMachine.Enter<GameBootstrapState>().Forget();

            DontDestroyOnLoad(gameObject);
        }

        public class Factory : PlaceholderFactory<GameBootstrapper>
        {
        }
    }
}
