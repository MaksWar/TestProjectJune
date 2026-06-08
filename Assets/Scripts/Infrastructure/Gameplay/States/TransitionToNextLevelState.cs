using Cysharp.Threading.Tasks;
using Gameplay.Level;
using Gameplay.Level.Models.Public;
using Infrastructure.States;
using Infrastructure.UI.LoadingCurtain;
using UnityEngine;

namespace Infrastructure.Gameplay.States
{
    public class TransitionToNextLevelState : IState
    {        
        private readonly SceneStateMachine _stateMachine;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly IGameplayContextService _gameplayContextService;
        private readonly ILevelCatalogService _levelCatalogService;
        private readonly ILevelLoader _levelLoader;

        public TransitionToNextLevelState(
            SceneStateMachine stateMachine,
            ILoadingCurtain loadingCurtain,
            IGameplayContextService gameplayContextService,
            ILevelCatalogService levelCatalogService,
            ILevelLoader levelLoader)
        {
            _stateMachine = stateMachine;
            _loadingCurtain = loadingCurtain;
            _gameplayContextService = gameplayContextService;
            _levelCatalogService = levelCatalogService;
            _levelLoader = levelLoader;
        }
        public async UniTask Enter()
        {
            _loadingCurtain.Show();

            _levelLoader.UnLoadCurrentLevel();
            await SetNextLevelPayload();

            await _stateMachine.Enter<InitializeGameplayState>();
        }

        public UniTask Exit() =>
            UniTask.CompletedTask;

        private async UniTask SetNextLevelPayload()
        {
            GameplayLevelPayload currentPayload = _gameplayContextService.LevelPayload;

            LevelData nextLevel = await _levelCatalogService.GetNextLevel(currentPayload.FigureType, currentPayload.LevelId);
            if (nextLevel == null)
            {
                return;
            }

            _gameplayContextService.SetLevelPayload(new GameplayLevelPayload(currentPayload.FigureType, nextLevel.Id));
        }
    }
}
