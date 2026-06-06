using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using Infrastructure.Gameplay;
using Infrastructure.Services.Log;
using Infrastructure.UI.LoadingCurtain;

namespace Infrastructure.States
{
    public class GameplayState : IPaylodedState<GameplayLevelPayload>
    {
        private readonly ILogService _logService;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly IAssetsProvider _assetsProvider;

        public GameplayState(
            ILoadingCurtain loadingCurtain,
            ILogService logService,
            IAssetsProvider assetsProvider
        )
        {
            _loadingCurtain = loadingCurtain;
            _logService = logService;
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

            await _assetsProvider.ReleaseAssetsByLabel(AssetsLabels.GameplayState, GetType());
        }
    }
}
