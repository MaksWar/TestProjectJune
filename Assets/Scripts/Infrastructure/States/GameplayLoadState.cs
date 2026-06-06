using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using Infrastructure.Gameplay;
using Infrastructure.SceneMenegment;
using Infrastructure.Services.Log;
using Infrastructure.UI.LoadingCurtain;

namespace Infrastructure.States
{
    public class GameplayLoadState : IPaylodedState<GameplayLevelPayload>
    {
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly ILogService _logService;
        private readonly IAssetsProvider _assetsProvider;
        private readonly ISceneLoader _sceneLoader;
        private readonly GameStateMachine _gameStateMachine;
        private readonly IGameplayContextService _gameplayContextService;

        public GameplayLoadState(
            ILoadingCurtain loadingCurtain,
            ILogService logService,
            IAssetsProvider assetsProvider,
            ISceneLoader sceneLoader,
            GameStateMachine gameStateMachine,
            IGameplayContextService gameplayContextService
        )
        {
            _loadingCurtain = loadingCurtain;
            _logService = logService;
            _assetsProvider = assetsProvider;
            _sceneLoader = sceneLoader;
            _gameStateMachine = gameStateMachine;
            _gameplayContextService = gameplayContextService;
        }

        public async UniTask Enter(GameplayLevelPayload payload)
        {
            _logService.Log($"GameplayLoadState Enter. Level id: {payload.LevelId}, figure type: {payload.FigureType}");
            _loadingCurtain.Show();
            _gameplayContextService.SetLevelPayload(payload);

            await _assetsProvider.WarmupAssetsByLabel(AssetsLabels.GameplayState, typeof(GameplayState));
            await _sceneLoader.Load(InfrastructureAssetPath.GameplayScene);
            await _gameStateMachine.Enter<GameplayState, GameplayLevelPayload>(payload);
        }

        public UniTask Exit() =>
            UniTask.CompletedTask;
    }
}
