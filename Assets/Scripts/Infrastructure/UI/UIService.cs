using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Infrastructure.AssetManagement;
using Infrastructure.UI.GUI;
using Infrastructure.UI.HUD;
using UnityEngine;
using Zenject;

namespace Infrastructure.UI
{
    public class UIService : IUIService
    {
        private readonly IAssetsProvider _assetsProvider;
        private readonly IInstantiator _instantiator;

        private readonly Dictionary<string, GameObject> _uiEntityCache = new();

        private const string RootUIFolder = "UI";
        
        public IHUDRoot HUDRoot { get; private set; }
        public IGUIRoot GUIRoot { get; private set; }

        public UIService(IAssetsProvider assetsProvider, IInstantiator instantiator)
        {
            _assetsProvider = assetsProvider;
            _instantiator = instantiator;
        }

        public async UniTask InitializeAsync()
        {
            HUDRoot ??= await InstantiateRootAsync<IHUDRoot>(InfrastructureAssetPath.HUDRoot);
            GUIRoot ??= await InstantiateRootAsync<IGUIRoot>(InfrastructureAssetPath.GUIRoot);

            await HUDRoot.InitializeAsync();
        }

        public async UniTask<TComponent> OpenUIEntity<TComponent>(string id, bool gui = true) where TComponent : class
        {
            string cacheKey = GetCacheKey(id, gui);
            if (_uiEntityCache.TryGetValue(cacheKey, out GameObject cachedEntity) && cachedEntity != null)
            {
                cachedEntity.SetActive(true);

                return GetComponent<TComponent>(cachedEntity, cacheKey);
            }

            TComponent component = await CreateUIEntity<TComponent>(id, gui);
            if (component is Component unityComponent)
            {
                _uiEntityCache[cacheKey] = unityComponent.gameObject;
            }

            return component;
        }

        public void CloseUIEntity(string id, bool gui = true)
        {
            string cacheKey = GetCacheKey(id, gui);
            if (_uiEntityCache.TryGetValue(cacheKey, out GameObject cachedEntity) == false || cachedEntity == null)
            {
                Debug.LogWarning($"{nameof(UIService)}: UI entity '{cacheKey}' was not opened.");

                return;
            }

            cachedEntity.SetActive(false);
        }

        private async UniTask<TComponent> CreateUIEntity<TComponent>(string id, bool gui = true) where TComponent : class
        {
            Transform parent = gui ? GUIRoot?.Transform : HUDRoot?.Transform;

            string assetKey = GetEntityPath(id);
            GameObject prefab = await _assetsProvider.Load<GameObject>(assetKey, GetType());
            if (prefab == null)
            {
                Debug.LogError($"{nameof(UIService)}: failed to load UI entity prefab '{assetKey}'.");

                return null;
            }

            GameObject instance = _instantiator.InstantiatePrefab(prefab, parent);
            TComponent component = GetComponent<TComponent>(instance, assetKey);

            return component;
        }

        private async UniTask<TRoot> InstantiateRootAsync<TRoot>(string assetKey) where TRoot : class
        {
            GameObject prefab = await _assetsProvider.Load<GameObject>(assetKey, GetType());
            if (prefab == null)
            {
                Debug.LogError($"{nameof(UIService)}: failed to load UI prefab '{assetKey}'.");

                return null;
            }

            TRoot root = _instantiator
                .InstantiatePrefab(prefab)
                .GetComponent<TRoot>();

            if (root == null)
            {
                Debug.LogError($"{nameof(UIService)}: prefab '{assetKey}' has no {typeof(TRoot).Name} component.");
            }

            return root;
        }

        private string GetEntityPath(string id) =>
            $"{RootUIFolder}/{id}.prefab";

        private static string GetCacheKey(string id, bool gui) =>
            $"{(gui ? nameof(GUIRoot) : nameof(HUDRoot))}/{id}";

        private static TComponent GetComponent<TComponent>(GameObject instance, string context) where TComponent : class
        {
            if (instance == null)
            {
                return null;
            }

            TComponent component = instance.GetComponent<TComponent>();
            if (component == null)
            {
                Debug.LogError($"{nameof(UIService)}: UI entity '{context}' has no {typeof(TComponent).Name} component.");
            }

            return component;
        }
    }
}
