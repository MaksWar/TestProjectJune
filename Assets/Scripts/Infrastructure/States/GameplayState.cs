using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using Infrastructure.Gameplay;
using Infrastructure.Services.Log;
using Infrastructure.UI;
using Infrastructure.UI.LoadingCurtain;

namespace Infrastructure.States
{
    public class GameplayState : IPayloadedState<GameplayLevelPayload>
    {
        private readonly IUIService _uiService;
        private readonly ILogService _logService;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly IAssetsProvider _assetsProvider;

        public GameplayState(
            ILoadingCurtain loadingCurtain,
            ILogService logService,
            IAssetsProvider assetsProvider,
            IUIService uiService
        )
        {
            _uiService = uiService;
            _logService = logService;
            _loadingCurtain = loadingCurtain;
            _assetsProvider = assetsProvider;
        }

        public UniTask Enter(GameplayLevelPayload payload)
        {
            _logService.Log($"GameplayState Enter. Level id: {payload.LevelId}, figure type: {payload.FigureType}");

            return UniTask.CompletedTask;
        }

        public async UniTask Exit()
        {
            _loadingCurtain.Show();
            _uiService.HUDRoot.Hide();
        }
    }
}
