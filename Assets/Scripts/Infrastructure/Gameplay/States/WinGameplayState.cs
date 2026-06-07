using Cysharp.Threading.Tasks;
using Enixan.GoldenDust.Extensions;
using Gameplay.Level.Presentation;
using Infrastructure.Services.SoundService;
using Infrastructure.States;

namespace Infrastructure.Gameplay.States
{
    public class WinGameplayState : IState
    {
        private readonly SceneStateMachine _sceneStateMachine;
        private readonly ISoundService _soundService;

        public WinGameplayState(
            SceneStateMachine sceneStateMachine,
            ISoundService soundService
        )
        {
            _sceneStateMachine = sceneStateMachine;
            _soundService = soundService;
        }

        public async UniTask Enter()
        {
            await _soundService.PlaySoundAsync(PresentationSoundsMap.WinSounds.GetRandomElement());
            
            await _sceneStateMachine.Enter<TransitionToNextLevelState>();
        }

        public UniTask Exit() =>
            UniTask.CompletedTask;
    }
}