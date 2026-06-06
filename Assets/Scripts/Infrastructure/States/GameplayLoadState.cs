using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using Infrastructure.SceneMenegment;
using Infrastructure.Services.Log;
using Infrastructure.UI.LoadingCurtain;

namespace Infrastructure.States
{
    public class GameplayLoadState : IPaylodedState<string>
    {
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly ILogService _logService;
        private readonly IAssetsProvider _assetsProvider;
        private readonly ISceneLoader _sceneLoader;
        private readonly GameStateMachine _gameStateMachine;

        public GameplayLoadState(
            ILoadingCurtain loadingCurtain,
            ILogService logService,
            IAssetsProvider assetsProvider,
            ISceneLoader sceneLoader,
            GameStateMachine gameStateMachine
        )
        {
            _loadingCurtain = loadingCurtain;
            _logService = logService;
            _assetsProvider = assetsProvider;
            _sceneLoader = sceneLoader;
            _gameStateMachine = gameStateMachine;
        }

        public async UniTask Enter(string levelID)
        {
            _logService.Log("GameplayLoadState Enter");
            _loadingCurtain.Show();

            await _assetsProvider.WarmupAssetsByLabel(AssetsLabels.GameplayState, typeof(GameplayState));
            await _sceneLoader.Load(InfrastructureAssetPath.GameplayScene);
            await _gameStateMachine.Enter<GameplayState, string>(levelID);
        }

        public UniTask Exit() =>
            UniTask.CompletedTask;
    }
}
