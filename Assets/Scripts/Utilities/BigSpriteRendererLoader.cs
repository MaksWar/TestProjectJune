using System;
using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace Utilities
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public class BigSpriteRendererLoader : MonoBehaviour
    {
        private IAssetsProvider _assetService;

        public SpriteRenderer spriteRenderer;
        public string spriteKey;

        public bool IsLoaded { get; private set; }

        [Inject]
        private void Construct(IAssetsProvider assetService) =>
            _assetService = assetService;

        public BigSpriteRendererLoader SetSpriteKey(string path, bool isAutoLoad = true)
        {
            if (spriteKey == path)
            {
                return this;
            }

            spriteKey = path;
            IsLoaded = false;

            if (isAutoLoad)
            {
                LoadAsync().Forget();
            }

            return this;
        }

        /// <summary>
        /// Загрузить и установить спрайт
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async UniTask LoadAsync()
        {
            if (IsLoaded)
            {
                return;
            }

            var sprite = await _assetService.Load<Sprite>(spriteKey, GetType());
            
            spriteRenderer.sprite = sprite;

            IsLoaded = true;
        }

        public async UniTask LoadAndSwapAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (IsLoaded && spriteKey == path)
            {
                return;
            }

            string previousKey = IsLoaded ? spriteKey : null;
            bool shouldKeepCurrentVisible = _assetService.IsAssetLoaded(path) == false;

            if (shouldKeepCurrentVisible == false &&
                string.IsNullOrEmpty(previousKey) == false &&
                previousKey != path)
            {
                _assetService.ReleaseAsset(previousKey, GetType());
                spriteRenderer.sprite = null;
                IsLoaded = false;
            }

            var sprite = await _assetService.Load<Sprite>(path, GetType());

            spriteRenderer.sprite = sprite;
            spriteKey = path;
            IsLoaded = true;

            if (shouldKeepCurrentVisible &&
                string.IsNullOrEmpty(previousKey) == false &&
                previousKey != path)
            {
                _assetService.ReleaseAsset(previousKey, GetType());
            }
        }

        /// <summary>
        /// Выгрузить спрайт
        /// </summary>
        public void Unload()
        {
            if (!IsLoaded)
                return;

            spriteRenderer.sprite = null;
            IsLoaded = false;
            
            _assetService.ReleaseAsset(spriteKey, GetType());
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BigSpriteRendererLoader))]
    public class BigSpriteRendererLoaderEditor : Editor
    {
        private BigSpriteRendererLoader _loader;

        private void OnEnable()
        {
            _loader = (BigSpriteRendererLoader) target;

            if (_loader.spriteRenderer == null)
                _loader.spriteRenderer = _loader.gameObject.GetComponent<SpriteRenderer>();

            if (string.IsNullOrEmpty(_loader.spriteKey) && _loader.spriteRenderer.sprite != null)
            {
                _loader.spriteKey = $"BigSprites/{_loader.spriteRenderer.sprite.name}.png";
            }

            if (!Application.isPlaying && _loader.spriteRenderer != null && _loader.spriteRenderer.sprite == null)
            {
                Addressables.LoadAssetAsync<Sprite>(_loader.spriteKey).Completed += handle =>
                {
                    _loader.spriteRenderer.sprite = Instantiate(handle.Result);
                };
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button(new GUIContent("Nulify", "Set sprite to null")))
                _loader.spriteRenderer.sprite = null;
        }
    }
#endif
}
