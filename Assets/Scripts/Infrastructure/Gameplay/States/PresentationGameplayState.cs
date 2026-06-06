using Cysharp.Threading.Tasks;
using Infrastructure.States;

namespace Infrastructure.Gameplay.States
{
    public class PresentationGameplayState : IState
    {
        private readonly SceneStateMachine _sceneStateMachine;

        public PresentationGameplayState(SceneStateMachine sceneStateMachine)
        {
            _sceneStateMachine = sceneStateMachine;
        }
        
        public async UniTask Enter()
        {
            _sceneStateMachine.Enter<GameLoopState>().Forget();
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}