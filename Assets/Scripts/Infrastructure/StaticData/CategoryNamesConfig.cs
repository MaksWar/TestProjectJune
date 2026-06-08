using System;
using System.Collections.Generic;
using Gameplay.Level;
using UnityEngine;

namespace Infrastructure.StaticData
{
    [CreateAssetMenu(fileName = "CategoryNamesConfig", menuName = "Static Data/Category Names Config")]
    public class CategoryNamesConfig : ScriptableObject
    {
        [SerializeField] private List<CategoryNameData> categories = new();

        private Dictionary<FigureType, string> _categoryNamesByType;

        public bool TryGetCategoryName(FigureType figureType, out string categoryName)
        {
            _categoryNamesByType ??= CreateCategoryNamesMap();

            return _categoryNamesByType.TryGetValue(figureType, out categoryName) && string.IsNullOrWhiteSpace(categoryName) == false;
        }

        private Dictionary<FigureType, string> CreateCategoryNamesMap()
        {
            Dictionary<FigureType, string> categoryNamesByType = new();

            foreach (CategoryNameData category in categories)
            {
                if (category == null)
                {
                    continue;
                }

                categoryNamesByType[category.Type] = category.Name;
            }

            return categoryNamesByType;
        }
    }

    [Serializable]
    public class CategoryNameData
    {
        public FigureType Type;
        public string Name;
    }
}
