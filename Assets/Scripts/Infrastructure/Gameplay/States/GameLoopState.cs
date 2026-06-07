using Cysharp.Threading.Tasks;
using Infrastructure.Gameplay.Tips;
using Infrastructure.States;

namespace Infrastructure.Gameplay.States
{
    public class GameLoopState : IState
    {
        private readonly SceneStateMachine _sceneStateMachine;
        private readonly IGameplayTipsService _gameplayTipsService;

        public GameLoopState(
            SceneStateMachine sceneStateMachine,
            IGameplayTipsService gameplayTipsService
        )
        {
            _sceneStateMachine = sceneStateMachine;
            _gameplayTipsService = gameplayTipsService;
        }

        public async UniTask Enter()
        {
        }

        public UniTask Exit()
        {
            _gameplayTipsService.Stop();
            
            return UniTask.CompletedTask;
        }
    }
}
