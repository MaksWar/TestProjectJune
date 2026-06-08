using Cysharp.Threading.Tasks;
using Infrastructure.SceneMenegment;
using Infrastructure.Services.Log;
using Infrastructure.UI.LoadingCurtain;

namespace Infrastructure.States
{
    public class GameLoadDataState : IState
    {
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly ISceneLoader _sceneLoader;
        private readonly ILogService _logService;

        public GameLoadDataState(ILoadingCurtain loadingCurtain, 
            ISceneLoader sceneLoader, 
            ILogService logService)
        {
            _loadingCurtain = loadingCurtain;
            _sceneLoader = sceneLoader;
            _logService = logService;
        }
        
        public async UniTask Enter()
        {
            _logService.Log("GameLoadingState Enter");
            await _sceneLoader.Load(InfrastructureAssetPath.GameLoadingScene);
            
            _loadingCurtain.Show();
        }

        public async UniTask Exit() => _loadingCurtain.Hide();
    }
}