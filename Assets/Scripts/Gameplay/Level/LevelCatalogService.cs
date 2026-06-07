using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Level.Models.Public;
using Infrastructure.AssetManagement;
using UnityEngine;

namespace Gameplay.Level
{
    public class LevelCatalogService : ILevelCatalogService
    {
        private const string LevelCatalogPath = "Levels/LevelCatalog.asset";

        private readonly IAssetsProvider _assetsProvider;
        private LevelCatalog _catalog;
        private bool _isCatalogLoaded;

        public LevelCatalogService(IAssetsProvider assetsProvider)
        {
            _assetsProvider = assetsProvider;
        }

        public async UniTask<IReadOnlyList<LevelGroupData>> GetGroupsAsync()
        {
            await LoadCatalogIfNeeded();

            return _catalog?.Groups != null
                ? _catalog.Groups
                : System.Array.Empty<LevelGroupData>();
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

        public async UniTask<LevelData> GetNextLevel(FigureType type, string currentId)
        {
            await LoadCatalogIfNeeded();

            IReadOnlyList<LevelData> levels = GetLevels(type);
            if (levels.Count == 0)
            {
                Debug.LogError($"{nameof(LevelCatalogService)}: no levels were found for '{type}'.");

                return null;
            }

            int currentIndex = -1;
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i] != null && levels[i].Id == currentId)
                {
                    currentIndex = i;

                    break;
                }
            }

            int nextIndex = (currentIndex + 1) % levels.Count;

            return levels[nextIndex];
        }

        public async UniTask<LevelEntry> LoadLevelEntry(FigureType type, string id)
        {
            await LoadCatalogIfNeeded();

            LevelData levelData = _catalog?.GetLevelData(type, id);
            TextAsset levelJson = levelData?.Json;

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

        private LevelEntry DeserializeLevelEntry(FigureType type, string id, TextAsset levelJson)
        {
            if (levelJson == null)
            {
                Debug.LogError($"{nameof(LevelCatalogService)}: level JSON for '{type}/{id}' was not found.");
                
                return null;
            }

            LevelEntry levelEntry = JsonUtility.FromJson<LevelEntry>(levelJson.text);

            levelEntry.LevelID = id;
            levelEntry.FigureType = type;

            return levelEntry;
        }
    }
}
