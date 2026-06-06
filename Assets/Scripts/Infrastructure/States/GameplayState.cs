using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using Infrastructure.SceneMenegment;
using Infrastructure.Services.Log;
using Infrastructure.UI.LoadingCurtain;

namespace Infrastructure.States
{
    public class GameplayState : IPaylodedState<string>
    {
        private readonly ILogService _logService;
        private readonly ISceneLoader _sceneLoader;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly IAssetsProvider _assetsProvider;

        public GameplayState(
            ILoadingCurtain loadingCurtain,
            ILogService logService,
            IAssetsProvider assetsProvider,
            ISceneLoader sceneLoader
        )
        {
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _logService = logService;
            _assetsProvider = assetsProvider;
        }

        public async UniTask Enter(string voxelModelId)
        {
            _logService.Log("GamePlayState Enter");
            _loadingCurtain.Show();

            await _assetsProvider.WarmupAssetsByLabel(AssetsLabels.GameplayState, GetType());
            await _sceneLoader.Load(InfrastructureAssetPath.GameplayScene);
        }

        public async UniTask Exit()
        {
            _loadingCurtain.Show();

            await _assetsProvider.ReleaseAssetsByLabel(AssetsLabels.GameplayState, GetType());
        }
    }
}
