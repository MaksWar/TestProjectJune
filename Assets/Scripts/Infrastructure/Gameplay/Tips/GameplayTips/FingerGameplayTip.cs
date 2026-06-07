using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Level;
using Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace Infrastructure.Gameplay.Tips
{
    public class FingerGameplayTip : BaseGameplayTip
    {
        private readonly LevelService _levelService;
        private readonly GameplayFingerRouteAnimator _fingerRouteAnimator;

        public FingerGameplayTip(
            float inactivityTime,
            LevelService levelService,
            IAssetsProvider assetsProvider,
            IInstantiator instantiator,
            GameplayTipSettings settings) : base(inactivityTime)
        {
            _levelService = levelService;
            _fingerRouteAnimator = new GameplayFingerRouteAnimator(assetsProvider, instantiator, settings);
        }

        public override async UniTask Play(GameplayTipContext context)
        {
            IReadOnlyList<Vector2> route = _levelService.CurrentPathPositions;
            Transform parent = context.FigureComponent != null ? context.FigureComponent.transform : null;

            await _fingerRouteAnimator.Play(route, parent);
        }

        public override void Stop() =>
            _fingerRouteAnimator.Stop();
    }
}
