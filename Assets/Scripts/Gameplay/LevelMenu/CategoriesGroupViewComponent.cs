using System.Collections.Generic;
using Gameplay.Level;
using UnityEngine;

namespace Gameplay.LevelMenu
{
    public class CategoriesGroupViewComponent : MonoBehaviour
    {
        [SerializeField] private Transform levelViewContainer;
        
        private List<LevelViewComponent> _levelViewComponents = new();
        private FigureType _figureType;

        public Transform LevelViewContainer => levelViewContainer;
        public FigureType FigureType => _figureType;
        public IReadOnlyList<LevelViewComponent> LevelViewComponents => _levelViewComponents;

        public void Initialize(FigureType figureType, List<LevelViewComponent> levelViewComponents)
        {
            _figureType = figureType;
            _levelViewComponents = levelViewComponents;
        }
    }
}
