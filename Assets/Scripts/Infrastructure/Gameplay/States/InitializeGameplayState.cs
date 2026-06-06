using Gameplay.Level;
using Infrastructure.States;
using Cysharp.Threading.Tasks;
using Infrastructure.UI.LoadingCurtain;
using UnityEngine;

namespace Infrastructure.Gameplay.States
{
    public class InitializeGameplayState : IState
    {
        private readonly SceneStateMachine _stateMachine;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly ILevelLoader _levelLoader;
        private readonly IGameplayContextService _gameplayContextService;

        public InitializeGameplayState(
            SceneStateMachine stateMachine,
            ILoadingCurtain loadingCurtain,
            ILevelLoader levelLoader,
            IGameplayContextService gameplayContextService)
        {
            _stateMachine = stateMachine;
            _loadingCurtain = loadingCurtain;
            _levelLoader = levelLoader;
            _gameplayContextService = gameplayContextService;
        }

        public async UniTask Enter()
        {
            await LoadLevel();

            _loadingCurtain.Hide();
            
            await _stateMachine.Enter<PresentationGameplayState>();
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        private async UniTask LoadLevel()
        {
            GameplayLevelPayload payload = _gameplayContextService.LevelPayload;

            await _levelLoader.LoadLevel(payload.FigureType, payload.LevelId);
        }
    }
}
