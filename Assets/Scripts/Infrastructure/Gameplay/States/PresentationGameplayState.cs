using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        public PresentationGameplayState(
            SceneStateMachine sceneStateMachine,
            ISoundService soundService,
            IInputService inputService
        )
        {
            _sceneStateMachine = sceneStateMachine;
            _soundService = soundService;
            _inputService = inputService;
        }

        public async UniTask Enter(GameplayLevelPayload payload)
        {
            await _soundService.PlaySoundAsync(PresentationSoundsMap.PresentationSoundByType[payload.FigureType]);

            _sceneStateMachine.Enter<GameLoopState>().Forget();
        }

        public UniTask Exit()
        {
            _inputService.Enable();
            
            return UniTask.CompletedTask;
        }
    }
}