using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using Infrastructure.SceneMenegment;
using Infrastructure.Services.Log;
using Infrastructure.Services.SaveLoadSystem;
using Infrastructure.UI.LoadingCurtain;

namespace Infrastructure.States
{
    public class LevelsMenuLoadState : IState
    {
        private readonly ILogService _logService;
        private readonly ISceneLoader _sceneLoader;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly IAssetsProvider _assetsProvider;
        private readonly GameStateMachine _gameStateMachine;
        private readonly IPrivateModelProvider _privateModelProvider;

        public LevelsMenuLoadState(
            ILoadingCurtain loadingCurtain,
            ISceneLoader sceneLoader,
            ILogService logService,
            IAssetsProvider assetsProvider,
            GameStateMachine gameStateMachine,
            IPrivateModelProvider privateModelProvider
        )
        {
            _loadingCurtain = loadingCurtain;
            _sceneLoader = sceneLoader;
            _logService = logService;
            _assetsProvider = assetsProvider;
            _gameStateMachine = gameStateMachine;
            _privateModelProvider = privateModelProvider;
        }

        public async UniTask Enter()
        {
            _logService.Log("MetaGameplayLoadState Enter");
            _loadingCurtain.Show();
            
            await _assetsProvider.WarmupAssetsByLabel(AssetsLabels.MetaGameplayState, GetType());
            await _sceneLoader.Load(InfrastructureAssetPath.LevelsMenuScene);

            _loadingCurtain.Hide();
            
            await _gameStateMachine.Enter<LevelsMenuState>();
        }

        public UniTask Exit() =>
            UniTask.CompletedTask;
    }
}
