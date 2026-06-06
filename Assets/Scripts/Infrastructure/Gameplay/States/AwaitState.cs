using Cysharp.Threading.Tasks;
using Infrastructure.States;

namespace Infrastructure.Gameplay.States
{
    public class AwaitState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly SceneStateMachine _sceneStateMachine;

        public AwaitState(GameStateMachine gameStateMachine, SceneStateMachine sceneStateMachine)
        {
            _sceneStateMachine = sceneStateMachine;
            _gameStateMachine = gameStateMachine;
        }

        public async UniTask Enter()
        {
            await UniTask.WaitUntil(IsInGameplay);
            
            _sceneStateMachine.Enter<InitializeGameplayState>().Forget();
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
        
        private bool IsInGameplay() => 
            _gameStateMachine.CurrentState is GameplayState;
    }
}