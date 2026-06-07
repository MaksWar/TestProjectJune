using System;
using System.Collections.Generic;
using Gameplay.Level;
using Gameplay.Level.Presentation;
using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
using Infrastructure.Services.SoundService;
using UnityEngine;
using Zenject;

namespace Infrastructure.Gameplay.Tips
{
    public class GameplayTipsService : IGameplayTipsService, ITickable, IDisposable
    {
        private const float SoundTipInactiveTime = 7f;
        private const float FingerTipInactiveTime = 14f;

        private readonly IInputService _inputService;
        private readonly ISoundService _soundService;
        private readonly LevelService _levelService;
        private readonly IAssetsProvider _assetsProvider;
        private readonly IInstantiator _instantiator;
        private readonly GameplayTipSettings _settings;
        private readonly Dictionary<float, BaseGameplayTip> _tipsByInactivityTime = new();

        private GameplayTipTimer _timer;
        private GameplayTipContext _context;
        private bool _isStarted;

        public GameplayTipsService(
            IInputService inputService,
            ISoundService soundService,
            LevelService levelService,
            IAssetsProvider assetsProvider,
            IInstantiator instantiator,
            GameplayTipSettings settings)
        {
            _inputService = inputService;
            _soundService = soundService;
            _levelService = levelService;
            _assetsProvider = assetsProvider;
            _instantiator = instantiator;
            _settings = settings;

            SubscribeInput();
        }

        public void Start(FigureType figureType, FigureComponent figureComponent)
        {
            StopTips();
            CreateTips(figureType);

            _context = new GameplayTipContext(figureType, figureComponent);
            _isStarted = true;

            _timer.Start();
        }

        public void Stop()
        {
            _isStarted = false;

            _timer?.Stop();

            StopTips();
        }

        public void Tick()
        {
            if (_isStarted == false)
            {
                return;
            }

            _timer?.Tick(Time.deltaTime, _context);
        }

        public void Dispose() =>
            UnsubscribeInput();

        private void SubscribeInput()
        {
            _inputService.Pressed += OnActivity;
            _inputService.Clicked += OnActivity;
            _inputService.DragStarted += OnActivity;
            _inputService.Dragged += OnActivity;
        }

        private void UnsubscribeInput()
        {
            _inputService.Pressed -= OnActivity;
            _inputService.Clicked -= OnActivity;
            _inputService.DragStarted -= OnActivity;
            _inputService.Dragged -= OnActivity;
        }

        private void OnActivity(InputPointerData inputPointerData)
        {
            if (!_isStarted)
            {
                return;
            }

            StopTips();
            _timer.Reset();
        }

        private void StopTips()
        {
            foreach (BaseGameplayTip tip in _tipsByInactivityTime.Values)
            {
                tip.Stop();
            }
        }

        private void CreateTips(FigureType figureType)
        {
            _tipsByInactivityTime.Clear();
            _tipsByInactivityTime[SoundTipInactiveTime] = new SoundGameplayTip(
                SoundTipInactiveTime,
                _soundService,
                PresentationSoundsMap.StartPresentationSoundByType[figureType]);

            _tipsByInactivityTime[FingerTipInactiveTime] = new FingerGameplayTip(
                FingerTipInactiveTime,
                _levelService,
                _assetsProvider,
                _instantiator,
                _settings);

            _timer = new GameplayTipTimer(_tipsByInactivityTime);
        }
    }
}
