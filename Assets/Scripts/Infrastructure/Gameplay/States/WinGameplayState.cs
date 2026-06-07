using Cysharp.Threading.Tasks;
using Infrastructure.States;

namespace Infrastructure.Gameplay.States
{
    public class WinGameplayState : IState
    {
        private readonly SceneStateMachine _sceneStateMachine;

        public WinGameplayState(SceneStateMachine sceneStateMachine)
        {
            _sceneStateMachine = sceneStateMachine;
        }

        public async UniTask Enter()
        {
            await _sceneStateMachine.Enter<TransitionToNextLevelState>();
        }

        public UniTask Exit() =>
            UniTask.CompletedTask;
    }
}