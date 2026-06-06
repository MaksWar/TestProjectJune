using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using Infrastructure.UI.HUD;
using UnityEngine;
using Zenject;

namespace Infrastructure.Factories
{
    public class UIFactory : IUIFactory
    {
        private readonly DiContainer _diContainer;
        private readonly IAssetsProvider _assetsProvider;

        public UIFactory(
            IAssetsProvider assetsProvider,
            DiContainer diContainer
            )
        {
            _diContainer = diContainer;
            _assetsProvider = assetsProvider;
        }

        public async UniTask<IHUDRoot> CreateHUD()
        {
            var hudRootObj = await _assetsProvider.Load<GameObject>(InfrastructureAssetPath.HUDRoot, GetType());

            return _diContainer
                .InstantiatePrefab(hudRootObj)
                .GetComponent<IHUDRoot>();
        }
    }
}