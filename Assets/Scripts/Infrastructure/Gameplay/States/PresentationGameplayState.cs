using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Level;
using Gameplay.Level.Presentation;
using Gameplay.Tips;
using Infrastructure.Services.Input;
using Infrastructure.Services.SoundService;
using Infrastructure.States;
using UnityEngine;

namespace Infrastructure.Gameplay.States
{
    public class PresentationGameplayState : IPayloadedState<GameplayLevelPayload>
    {
        private readonly ISoundService _soundService;
        private readonly IInputService _inputService;
        private readonly IGameplayTipsService _gameplayTipsService;
        private readonly SceneStateMachine _sceneStateMachine;
        private readonly LevelService _levelService;
        private readonly IGameplaySceneLifetime _gameplaySceneLifetime;

        private CancellationTokenSource _presentationCancellationTokenSource;

        public PresentationGameplayState(
            SceneStateMachine sceneStateMachine,
            ISoundService soundService,
            IInputService inputService,
            LevelService levelService,
            IGameplaySceneLifetime gameplaySceneLifetime,
            IGameplayTipsService gameplayTipsService
        )
        {
            _sceneStateMachine = sceneStateMachine;
            _soundService = soundService;
            _inputService = inputService;
            _levelService = levelService;
            _gameplaySceneLifetime = gameplaySceneLifetime;
            _gameplayTipsService = gameplayTipsService;
        }

        public async UniTask Enter(GameplayLevelPayload payload)
        {
            CancelPresentation();

            CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_gameplaySceneLifetime.CancellationToken);
            _presentationCancellationTokenSource = cancellationTokenSource;
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            try
            {
                string soundKey = PresentationSoundsMap.StartPresentationSoundByType[payload.FigureType];

                await _soundService.PlaySoundAsync(soundKey, cancellationToken: cancellationToken);

                if (TryStopPresentationIfCancelled(cancellationToken, payload.FigureComponent))
                {
                    return;
                }

                await _levelService.Activate(payload.FigureComponent);

                if (TryStopPresentationIfCancelled(cancellationToken, payload.FigureComponent))
                {
                    return;
                }

                _gameplayTipsService.Start(payload.FigureType, payload.FigureComponent);

                if (TryStopPresentationIfCancelled(cancellationToken, payload.FigureComponent))
                {
                    return;
                }

                _sceneStateMachine.Enter<GameLoopState>().Forget();
            }
            finally
            {
                if (ReferenceEquals(_presentationCancellationTokenSource, cancellationTokenSource))
                {
                    _presentationCancellationTokenSource = null;
                }

                cancellationTokenSource.Dispose();
            }
        }

        public UniTask Exit()
        {
            CancelPresentation();

            return UniTask.CompletedTask;
        }

        private bool TryStopPresentationIfCancelled(CancellationToken cancellationToken, FigureComponent figureComponent)
        {
            if (cancellationToken.IsCancellationRequested == false)
            {
                return false;
            }

            _gameplayTipsService.Stop();

            if (figureComponent != null)
            {
                Object.Destroy(figureComponent.gameObject);
            }

            return true;
        }

        private void CancelPresentation()
        {
            if (_presentationCancellationTokenSource == null ||
                _presentationCancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            _presentationCancellationTokenSource.Cancel();
        }
    }
}
