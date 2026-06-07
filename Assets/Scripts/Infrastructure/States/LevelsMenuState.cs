using Cysharp.Threading.Tasks;
using Gameplay.LevelMenu;
using Infrastructure.UI;

namespace Infrastructure.States
{
    public class LevelsMenuState : IState
    {
        private readonly IUIService _uiService;

        public LevelsMenuState(IUIService uiService)
        {
            _uiService = uiService;
        }

        public UniTask Enter() =>
            UniTask.CompletedTask;

        public UniTask Exit()
        {
            _uiService.CloseUIEntity(LevelMenuPresenterComponent.PrefabName);

            return UniTask.CompletedTask;
        }
    }
}
