using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Level.Models.Public;
using Infrastructure.AssetManagement;
using UnityEngine;

namespace Gameplay.Level
{
    public class LevelCatalogService : ILevelCatalogService
    {
        private const string LevelCatalogPath = AssetsPath.LevelCatalog;

        private readonly IAssetsProvider _assetsProvider;
        private LevelCatalog _catalog;
        private bool _isCatalogLoaded;

        public LevelCatalogService(IAssetsProvider assetsProvider)
        {
            _assetsProvider = assetsProvider;
        }

        public IReadOnlyList<LevelData> GetLevels(FigureType type)
        {
            LevelGroupData group = _catalog?.GetLevelGroupByType(type);
            return group?.Levels != null ? group.Levels : System.Array.Empty<LevelData>();
        }

        public bool TryGetLevel(FigureType type, string id, out LevelData levelData)
        {
            levelData = _catalog?.GetLevelData(type, id);
            return levelData != null;
        }

        public async UniTask<LevelEntry> LoadLevelEntry(FigureType type, string id)
        {
            await LoadCatalogIfNeeded();

            LevelData levelData = _catalog?.GetLevelData(type, id);
            TextAsset levelJson = levelData?.Json;
            
            if (levelJson == null)
            {
                levelJson = await LoadLevelAssetByPath(type, id);
            }

            return DeserializeLevelEntry(type, id, levelJson);
        }

        private async UniTask LoadCatalogIfNeeded()
        {
            if (_isCatalogLoaded)
            {
                return;
            }

            _catalog = await _assetsProvider.Load<LevelCatalog>(LevelCatalogPath, GetType());
            _isCatalogLoaded = true;

            if (_catalog == null)
            {
                Debug.LogWarning($"{nameof(LevelCatalogService)}: '{LevelCatalogPath}' was not found. Direct level address loading will be used.");
            }
        }

        private async UniTask<TextAsset> LoadLevelAssetByPath(FigureType type, string id)
        {
            return await _assetsProvider.Load<TextAsset>(GetLevelPath(type, id), GetType());
        }

        private LevelEntry DeserializeLevelEntry(FigureType type, string id, TextAsset levelJson)
        {
            if (levelJson == null)
            {
                Debug.LogError($"{nameof(LevelCatalogService)}: level JSON for '{type}/{id}' was not found.");
                return null;
            }

            LevelEntry levelEntry = JsonUtility.FromJson<LevelEntry>(levelJson.text);

            if (levelEntry == null)
            {
                Debug.LogError($"{nameof(LevelCatalogService)}: level JSON for '{type}/{id}' has invalid {nameof(LevelEntry)} data.");
                return null;
            }

            levelEntry.LevelID = id;
            levelEntry.FigureType = type;

            return levelEntry;
        }

        private static string GetLevelPath(FigureType type, string id) =>
            $"Configs/Levels/{type}/{id}.json";
    }
}
