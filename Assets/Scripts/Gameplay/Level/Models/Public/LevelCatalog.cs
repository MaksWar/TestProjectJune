using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Level.Models.Public
{
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Static Data/Level Catalog")]
    public class LevelCatalog : ScriptableObject
    {
        public List<LevelGroupData> Groups = new();

        public LevelGroupData GetLevelGroupByType(FigureType type)
        {
            return Groups?.Find(group => group != null && group.Type == type);
        }

        public LevelData GetLevelData(FigureType type, string id)
        {
            LevelGroupData group = GetLevelGroupByType(type);
            return group?.Levels?.Find(level => level != null && level.Id == id);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (LevelGroupData group in Groups)
            {
                if (group?.Levels == null)
                {
                    continue;
                }

                foreach (LevelData level in group.Levels)
                {
                    level?.RefreshIdFromJsonAsset();
                }
            }
        }
#endif
    }

    [Serializable]
    public class LevelGroupData
    {
        public FigureType Type;
        public List<LevelData> Levels = new();
    }

    [Serializable]
    public class LevelData
    {
        public string Id;
        public TextAsset Json;

        public bool HasJsonReference => Json != null;

#if UNITY_EDITOR
        public void RefreshIdFromJsonAsset()
        {
            if (HasJsonReference == false)
            {
                return;
            }
            
            Id = Path.GetFileNameWithoutExtension(Json.name);
        }
#endif
    }
}
