using System;
using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Zenject;

namespace Utilities
{
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class BigImageSpriteLoader : MonoBehaviour
    {
        private IAssetsProvider _assetService;

        public Image image;
        public string spriteKey;

        public bool IsLoaded { get; private set; }

        private const string RemoteAfterLevelPrefix = "Remote";

        [Inject]
        private void Construct(IAssetsProvider assetService) =>
            _assetService = assetService;

        public BigImageSpriteLoader SetSpriteKey(string path, bool isAutoLoad = true)
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
                Debug.LogError("Загружаем уже загруженую графику");
                
                return;
            }

            var sprite = await _assetService.Load<Sprite>(spriteKey, GetType());
            
            image.sprite = sprite;

            IsLoaded = true;
        }

        /// <summary>
        /// Выгрузить спрайт
        /// </summary>
        public void Unload()
        {
            if (!IsLoaded)
                return;

            image.sprite = null;
            IsLoaded = false;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BigImageSpriteLoader))]
    public class BigSpriteLoaderEditor : Editor
    {
        private BigImageSpriteLoader _loader;

        private void OnEnable()
        {
            _loader = (BigImageSpriteLoader) target;

            if (_loader.image == null)
                _loader.image = _loader.gameObject.GetComponent<Image>();

            if (string.IsNullOrEmpty(_loader.spriteKey) && _loader.image.sprite != null)
            {
                _loader.spriteKey = $"BigSprites/{_loader.image.sprite.name}.png";
            }

            if (!Application.isPlaying && _loader.image != null && _loader.image.sprite == null)
            {
                Addressables.LoadAssetAsync<Sprite>(_loader.spriteKey).Completed += handle =>
                {
                    _loader.image.sprite = Instantiate(handle.Result);
                };
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button(new GUIContent("Nulify", "Set sprite to null")))
                _loader.image.sprite = null;
        }
    }
#endif
}