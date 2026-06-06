using Cysharp.Threading.Tasks;
using Infrastructure.States;

namespace Infrastructure.Gameplay.States
{
    public class GameLoopState : IState
    {
        private readonly SceneStateMachine _sceneStateMachine;

        public GameLoopState(SceneStateMachine sceneStateMachine
        )
        {
            _sceneStateMachine = sceneStateMachine;
        }

        public async UniTask Enter()
        {
        }

        public UniTask Exit()
        {
            
            return UniTask.CompletedTask;
        }
    }
}
