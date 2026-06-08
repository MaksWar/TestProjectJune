using Cysharp.Threading.Tasks;
using Gameplay.Level;
using Gameplay.LevelMenu;
using Infrastructure.AssetManagement;
using Infrastructure.SceneMenegment;
using Infrastructure.Services.Log;
using Infrastructure.Services.SaveLoadSystem;
using Infrastructure.Services.SpriteAtlassService;
using Infrastructure.StaticData;
using Infrastructure.UI;
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
        private readonly IUIService _uiService;
        private readonly ILevelCatalogService _levelCatalogService;
        private readonly ISpriteAtlasService _spriteAtlasService;
        private readonly IStaticDataService _staticDataService;

        private LevelMenuPresenterComponent _levelMenu;

        public LevelsMenuLoadState(
            ILoadingCurtain loadingCurtain,
            ISceneLoader sceneLoader,
            ILogService logService,
            IAssetsProvider assetsProvider,
            GameStateMachine gameStateMachine,
            IUIService uiService,
            ILevelCatalogService levelCatalogService,
            ISpriteAtlasService spriteAtlasService,
            IStaticDataService staticDataService
        )
        {
            _loadingCurtain = loadingCurtain;
            _sceneLoader = sceneLoader;
            _logService = logService;
            _assetsProvider = assetsProvider;
            _gameStateMachine = gameStateMachine;
            _uiService = uiService;
            _levelCatalogService = levelCatalogService;
            _spriteAtlasService = spriteAtlasService;
            _staticDataService = staticDataService;
        }

        public async UniTask Enter()
        {
            _logService.Log("MetaGameplayLoadState Enter");
            _loadingCurtain.Show();
            
            await _sceneLoader.Load(InfrastructureAssetPath.LevelsMenuScene);

            await InitializeLevelMenu();

            _loadingCurtain.Hide();
            
            await _gameStateMachine.Enter<LevelsMenuState>();
        }

        public UniTask Exit() =>
            UniTask.CompletedTask;

        private async UniTask InitializeLevelMenu()
        {
            _levelMenu = await _uiService.OpenUIEntity<LevelMenuPresenterComponent>(LevelMenuPresenterComponent.PrefabName);

            await _levelMenu.InitializeAsync(_levelCatalogService, _gameStateMachine, _spriteAtlasService, _staticDataService);
        }
    }
}
