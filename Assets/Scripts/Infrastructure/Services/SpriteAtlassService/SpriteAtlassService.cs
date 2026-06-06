using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace Infrastructure.Services.SpriteAtlassService
{
    public class SpriteAtlasService : ISpriteAtlasService
    {
        public const string LevelsLabel = "levels";
        
        private readonly Dictionary<string, List<SpriteAtlas>> _atlases = new();

        private string _noneIconName = "none";
        private Sprite _noneIcon;

        private readonly List<string> _atlasesTypes = new()
        {
            LevelsLabel
        };

        private readonly Dictionary<string, List<SpriteAtlas>> _loadedRemoteLocationsAtlasses = new();
        
        public async UniTask InitializeAsync()
        {
            foreach (string type in _atlasesTypes)
            {
                var locations = await Addressables.LoadResourceLocationsAsync(new List<string> { "sprite_atlas", type },
                    Addressables.MergeMode.Intersection, typeof(SpriteAtlas));

                foreach (var location in locations)
                {
                    SpriteAtlas atlas = await Addressables.LoadAssetAsync<SpriteAtlas>(location).ToUniTask();
                    RegisterAtlas(type, atlas);
                }
            }
        }

        public Sprite GetSprite(string name, string type)
        {
            if (TryGetSprite(name, type, out Sprite sprite))
            {
                return sprite;
            }
            else
            {
                return _noneIcon;
            }
        }

        public Sprite GetSprite(string name, List<string> atlasesToSearchLabels)
        {
            for (int i = 0; i < atlasesToSearchLabels.Count; i++)
            {
                if (TryGetSprite(name, atlasesToSearchLabels[i], out Sprite sprite))
                {
                    return sprite;
                }
            }

            return _noneIcon;
        }

        public bool TryGetSprite(string name, string type, out Sprite sprite)
        {
            sprite = null;
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogWarning("Request sprite with empty name");
                return false;
            }

            if (!_atlases.ContainsKey(type))
            {
                Debug.LogError(
                    $"Atlas with type '{type}' not found. Available atlas types: {string.Join(", ", _atlases.Keys)}");
                return false;
            }

            foreach (var atlas in _atlases[type])
            {
                Debug.Log($"count sprites in atlas {atlas.name} is {atlas.spriteCount}");

                Sprite[] sprites = new Sprite[atlas.spriteCount];
                atlas.GetSprites(sprites);
                foreach (Sprite variable in sprites)
                {
                    Debug.Log($"sprite name in atlas {atlas.name} is {variable.name}");
                }
                
                sprite = atlas.GetSprite(name);
                if (sprite != null)
                {
                    return true;
                }
            }

            //Search in Loaded remoteAtlasses if still not found
            //TODO: getCurrentLocation and search only in that atlasses. Now it searches in all locations atlasses which is not optimized.
            foreach (var locationAtlassesPair in _loadedRemoteLocationsAtlasses)
            {
                foreach (SpriteAtlas remoteAtlas in locationAtlassesPair.Value)
                {
                    sprite = remoteAtlas.GetSprite(name);
                    if (sprite != null)
                    {
                        return true;
                    }
                }
            }

            Debug.LogWarning($"Sprite {name} didn't find in any atlases with type {type}");
            return false;
        }

        public bool IsNoneSprite(Sprite noneSprite) =>
            noneSprite == _noneIcon;

        public Sprite GetDefaultNoneSprite() =>
            _noneIcon;


        public void Dispose()
        {
        }

        private void RegisterAtlas(string type, SpriteAtlas atlas)
        {
            if (atlas == null)
            {
                return;
            }

            if (!_atlases.ContainsKey(type))
            {
                _atlases.Add(type, new List<SpriteAtlas>());
            }

            if (_atlases[type].Contains(atlas))
            {
                return;
            }

            _atlases[type].Add(atlas);

            var sprite = atlas.GetSprite(_noneIconName);
            if (sprite != null)
            {
                _noneIcon = sprite;
            }
        }
    }
}
