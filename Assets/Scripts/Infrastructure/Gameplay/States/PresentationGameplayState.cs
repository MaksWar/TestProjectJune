using Cysharp.Threading.Tasks;
using Gameplay.Level;
using Gameplay.Level.Presentation;
using Gameplay.Tips;
using Infrastructure.Services.Input;
using Infrastructure.Services.SoundService;
using Infrastructure.States;

namespace Infrastructure.Gameplay.States
{
    public class PresentationGameplayState : IPaylodedState<GameplayLevelPayload>
    {
        private readonly ISoundService _soundService;
        private readonly IInputService _inputService;
        private readonly IGameplayTipsService _gameplayTipsService;
        private readonly SceneStateMachine _sceneStateMachine;
        private readonly LevelService _levelService;

        public PresentationGameplayState(
            SceneStateMachine sceneStateMachine,
            ISoundService soundService,
            IInputService inputService,
            LevelService levelService,
            IGameplayTipsService gameplayTipsService
        )
        {
            _sceneStateMachine = sceneStateMachine;
            _soundService = soundService;
            _inputService = inputService;
            _levelService = levelService;
            _gameplayTipsService = gameplayTipsService;
        }

        public async UniTask Enter(GameplayLevelPayload payload)
        {
            string soundKey = PresentationSoundsMap.StartPresentationSoundByType[payload.FigureType];

            await _soundService.PlaySoundAsync(soundKey);
            await _levelService.Activate(payload.FigureComponent);

            _gameplayTipsService.Start(payload.FigureType, payload.FigureComponent);

            _sceneStateMachine.Enter<GameLoopState>().Forget();
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}
