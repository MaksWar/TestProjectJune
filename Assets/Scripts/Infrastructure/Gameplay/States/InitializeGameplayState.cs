using Gameplay.Level;
using Infrastructure.States;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.Input;
using Infrastructure.UI.LoadingCurtain;
using UnityEngine;

namespace Infrastructure.Gameplay.States
{
    public class InitializeGameplayState : IState
    {
        private readonly ILevelLoader _levelLoader;
        private readonly IInputService _inputService;
        private readonly SceneStateMachine _stateMachine;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly IGameplayContextService _gameplayContextService;

        public InitializeGameplayState(
            SceneStateMachine stateMachine,
            ILoadingCurtain loadingCurtain,
            ILevelLoader levelLoader,
            IGameplayContextService gameplayContextService,
            IInputService inputService
            )
        {
            _levelLoader = levelLoader;
            _inputService = inputService;
            _stateMachine = stateMachine;
            _loadingCurtain = loadingCurtain;
            _gameplayContextService = gameplayContextService;
        }

        public async UniTask Enter()
        {
            GameplayLevelPayload payload = _gameplayContextService.LevelPayload;

            _inputService.Disable();
            
            FigureComponent figureComponent = await LoadLevel(payload);

            _loadingCurtain.Hide();
            
            await _stateMachine.Enter<PresentationGameplayState, GameplayLevelPayload>(
                new GameplayLevelPayload(payload.FigureType, payload.LevelId, figureComponent));
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        private async UniTask<FigureComponent> LoadLevel(GameplayLevelPayload payload)
        {
            return await _levelLoader.LoadLevel(payload.FigureType, payload.LevelId);
        }
    }
}
