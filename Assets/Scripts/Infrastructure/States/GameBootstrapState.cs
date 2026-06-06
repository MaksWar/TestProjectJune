using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using Infrastructure.Services.Log;
using Infrastructure.Services.SaveLoadSystem;
using Infrastructure.Services.SaveLoadSystem.AuthService;
using Infrastructure.Services.SoundService;
using Infrastructure.Services.SpriteAtlassService;
using Infrastructure.StaticData;
using Infrastructure.UI.LoadingCurtain.Proxy;
using UnityEngine.Device;

namespace Infrastructure.States
{
    public class GameBootstrapState : IState
    {
        private readonly ILogService _logService;
        private readonly IAuthService _authService;
        private readonly ISoundService _soundService;
        private readonly IAssetsProvider _assetsProvider;
        private readonly ISaveLoadManager _saveLoadManager;
        private readonly GameStateMachine _gameStateMachine;
        private readonly IStaticDataService _staticDataService;
        private readonly ISpriteAtlasService _spriteAtlasService;
        private readonly ILoadingCurtainProxy _loadingCurtainProxy;
        private readonly IPrivateModelProvider _privateModelProvider;

        public GameBootstrapState(
            GameStateMachine gameStateMachine,
            IAssetsProvider assetsProvider,
            ILoadingCurtainProxy loadingCurtainProxy,
            ILogService logService,
            IStaticDataService staticDataService,
            ISaveLoadManager saveLoadManager,
            IPrivateModelProvider privateModelProvider,
            IAuthService authService,
            ISpriteAtlasService spriteAtlasService,
            ISoundService soundService
        )
        {
            _logService = logService;
            _authService = authService;
            _soundService = soundService;
            _assetsProvider = assetsProvider;
            _saveLoadManager = saveLoadManager;
            _gameStateMachine = gameStateMachine;
            _staticDataService = staticDataService;
            _spriteAtlasService = spriteAtlasService;
            _loadingCurtainProxy = loadingCurtainProxy;
            _privateModelProvider = privateModelProvider;
        }

        public async UniTask Enter()
        {
            _logService.Log("GameBootstrapState Enter");

            Application.targetFrameRate = 60;
            
            await InitSaveLoadServices();
            await InitServices();
            
            _gameStateMachine.Enter<LevelsMenuLoadState>().Forget();
        }

        private async UniTask InitSaveLoadServices()
        {
            await _privateModelProvider.InitializeAsync();
            await _saveLoadManager.InitializeAsync();

            if (_authService.IsNewUser())
            {
                _authService.SetIsNewUserValue(false);
            }
        }

        private async UniTask InitServices()
        {
            // Инициализация сервисов
            await _assetsProvider.InitializeAsync();
            await _staticDataService.LoadAllAsync();
            await _loadingCurtainProxy.InitializeAsync();
            await _privateModelProvider.InitializeAsync();
            await _soundService.InitializeAsync();
            await _spriteAtlasService.InitializeAsync();
        }


        public UniTask Exit() => default;
    }
}
