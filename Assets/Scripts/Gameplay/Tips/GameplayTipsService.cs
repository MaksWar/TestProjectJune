using System;
using System.Collections.Generic;
using Gameplay.Level;
using Gameplay.Level.Presentation;
using Gameplay.Tips.GameplayTips;
using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
using Infrastructure.Services.SoundService;
using Infrastructure.StaticData;
using UnityEngine;
using Zenject;

namespace Gameplay.Tips
{
    public class GameplayTipsService : IGameplayTipsService, ITickable, IDisposable
    {
        private readonly LevelService _levelService;
        private readonly IInputService _inputService;
        private readonly ISoundService _soundService;
        private readonly IInstantiator _instantiator;
        private readonly GameplayTipSettings _settings;
        private readonly IAssetsProvider _assetsProvider;
        private readonly IStaticDataService _staticDataService;

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
            IStaticDataService staticDataService,
            GameplayTipSettings settings)
        {
            _inputService = inputService;
            _soundService = soundService;
            _levelService = levelService;
            _assetsProvider = assetsProvider;
            _instantiator = instantiator;
            _staticDataService = staticDataService;
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
            GameplayTipsEntry gameplayTipsEntry = _staticDataService.GetGameplayTipsEntry();
            if (gameplayTipsEntry == null)
            {
                Debug.LogError($"{nameof(GameplayTipsService)}: {nameof(GameplayTipsEntry)} is not loaded.");

                return;
            }

            float soundTipInactiveTime = gameplayTipsEntry.SoundTipInactiveTime;
            float fingerTipInactiveTime = gameplayTipsEntry.FingerTipInactiveTime;

            _tipsByInactivityTime.Clear();
            _tipsByInactivityTime[soundTipInactiveTime] = new SoundGameplayTip(
                _soundService,
                PresentationSoundsMap.StartPresentationSoundByType[figureType]);

            _tipsByInactivityTime[fingerTipInactiveTime] = new FingerGameplayTip(
                _levelService,
                _assetsProvider,
                _instantiator,
                _settings);

            _timer = new GameplayTipTimer(_tipsByInactivityTime);
        }
    }
}
