using Cysharp.Threading.Tasks;
using Gameplay.Level;
using Gameplay.Level.Presentation;
using Infrastructure.Services.Input;
using Infrastructure.Services.SoundService;
using Infrastructure.States;

namespace Infrastructure.Gameplay.States
{
    public class PresentationGameplayState : IPaylodedState<GameplayLevelPayload>
    {
        private readonly ISoundService _soundService;
        private readonly IInputService _inputService;
        private readonly SceneStateMachine _sceneStateMachine;
        private readonly LevelService _levelService;

        public PresentationGameplayState(
            SceneStateMachine sceneStateMachine,
            ISoundService soundService,
            IInputService inputService,
            LevelService levelService
        )
        {
            _sceneStateMachine = sceneStateMachine;
            _soundService = soundService;
            _inputService = inputService;
            _levelService = levelService;
        }

        public async UniTask Enter(GameplayLevelPayload payload)
        {
            await _soundService.PlaySoundAsync(PresentationSoundsMap.PresentationSoundByType[payload.FigureType]);
            await _levelService.Activate(payload.FigureComponent);

            _sceneStateMachine.Enter<GameLoopState>().Forget();
        }

        public UniTask Exit()
        {
            _inputService.Enable();
            
            return UniTask.CompletedTask;
        }
    }
}
