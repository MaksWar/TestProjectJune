using System;
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
            if (await LoadCatalogIfNeeded() == false)
            {
                return Array.Empty<LevelGroupData>();
            }

            return _catalog?.Groups != null
                ? _catalog.Groups
                : Array.Empty<LevelGroupData>();
        }

        public async UniTask<LevelData> GetNextLevel(FigureType type, string currentId)
        {
            if (!await LoadCatalogIfNeeded())
            {
                return null;
            }

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
            if (await LoadCatalogIfNeeded() == false)
            {
                return null;
            }

            LevelData levelData = _catalog?.GetLevelData(type, id);
            TextAsset levelJson = levelData?.Json;

            return DeserializeLevelEntry(type, id, levelJson);
        }

        private async UniTask<bool> LoadCatalogIfNeeded()
        {
            if (_isCatalogLoaded && _catalog != null)
            {
                return true;
            }

            _catalog = await _assetsProvider.Load<LevelCatalog>(LevelCatalogPath, GetType());

            if (_catalog == null)
            {
                Debug.LogError($"{nameof(LevelCatalogService)}: failed to load level catalog at '{LevelCatalogPath}'.");

                return false;
            }

            _isCatalogLoaded = true;

            return true;
        }

        private LevelEntry DeserializeLevelEntry(FigureType type, string id, TextAsset levelJson)
        {
            if (levelJson == null)
            {
                Debug.LogError($"{nameof(LevelCatalogService)}: level JSON for '{type}/{id}' was not found.");
                
                return null;
            }

            LevelEntry levelEntry;
            try
            {
                levelEntry = JsonUtility.FromJson<LevelEntry>(levelJson.text);
            }
            catch (Exception exception)
            {
                Debug.LogError($"{nameof(LevelCatalogService)}: failed to deserialize level JSON for '{type}/{id}'. {exception.Message}");

                return null;
            }

            if (levelEntry == null)
            {
                Debug.LogError($"{nameof(LevelCatalogService)}: level JSON for '{type}/{id}' is invalid.");

                return null;
            }

            levelEntry.LevelID = id;
            levelEntry.FigureType = type;

            return levelEntry;
        }

        private IReadOnlyList<LevelData> GetLevels(FigureType type)
        {
            LevelGroupData group = _catalog?.GetLevelGroupByType(type);
            
            return group?.Levels != null ? group.Levels : Array.Empty<LevelData>();
        }
    }
}
