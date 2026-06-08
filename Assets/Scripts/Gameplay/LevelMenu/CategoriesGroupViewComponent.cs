using System.Collections.Generic;
using Gameplay.Level;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.LevelMenu
{
    public class CategoriesGroupViewComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI categoryNameText;
        [SerializeField] private Transform levelViewContainer;
        [SerializeField] private NestedScrollRectDirectionRouter scrollDirectionRouter;
        
        private List<LevelViewComponent> _levelViewComponents = new();
        private FigureType _figureType;

        public Transform LevelViewContainer => levelViewContainer;
        public FigureType FigureType => _figureType;
        public IReadOnlyList<LevelViewComponent> LevelViewComponents => _levelViewComponents;

        public void Initialize(FigureType figureType, string categoryName, List<LevelViewComponent> levelViewComponents)
        {
            _figureType = figureType;
            _levelViewComponents = levelViewComponents;
            
            categoryNameText.text = categoryName;
        }

        public void SetParentScrollRect(ScrollRect scrollRect) =>
            scrollDirectionRouter.SetParentScrollRect(scrollRect);
    }
}
