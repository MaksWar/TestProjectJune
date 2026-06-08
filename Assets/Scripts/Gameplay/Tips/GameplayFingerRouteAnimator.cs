using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace Gameplay.Tips
{
    public class GameplayFingerRouteAnimator
    {
        private const string FingerPrefabPath = "Tutorial/TutorialFinger.prefab";

        private readonly IAssetsProvider _assetsProvider;
        private readonly IInstantiator _instantiator;
        private readonly GameplayTipSettings _settings;

        private FingerComponent _fingerComponent;
        private bool _isStopped = true;

        public GameplayFingerRouteAnimator(
            IAssetsProvider assetsProvider,
            IInstantiator instantiator,
            GameplayTipSettings settings)
        {
            _assetsProvider = assetsProvider;
            _instantiator = instantiator;
            _settings = settings;
        }

        public async UniTask Play(IReadOnlyList<Vector2> route, Transform parent)
        {
            if (route == null || route.Count == 0 || parent == null)
            {
                Stop();
                return;
            }

            _isStopped = false;

            FingerComponent fingerComponent = await GetFingerComponent();
            if (_isStopped || fingerComponent == null)
            {
                return;
            }

            fingerComponent.PlayRoute(route, parent, _settings.FingerPointsPerSecond, _settings.FingerOffset);
        }

        public void Stop()
        {
            _isStopped = true;

            _fingerComponent?.Stop();
        }

        private async UniTask<FingerComponent> GetFingerComponent()
        {
            if (_fingerComponent != null)
            {
                return _fingerComponent;
            }

            GameObject prefab = await _assetsProvider.Load<GameObject>(FingerPrefabPath, GetType());
            if (prefab == null)
            {
                Debug.LogError($"{nameof(GameplayFingerRouteAnimator)}: finger prefab '{FingerPrefabPath}' was not found.");

                return null;
            }

            GameObject fingerObject = _instantiator.InstantiatePrefab(prefab);

            _fingerComponent = fingerObject.GetComponent<FingerComponent>();
            if (_fingerComponent == null)
            {
                Debug.LogError($"{nameof(GameplayFingerRouteAnimator)}: prefab '{FingerPrefabPath}' has no {nameof(FingerComponent)}.");
                Object.Destroy(fingerObject);

                return null;
            }

            _fingerComponent.Stop();

            return _fingerComponent;
        }
    }
}
