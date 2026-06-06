using System.Collections.Generic;
using Gameplay.Level.Models.Public;
using UnityEngine;

namespace Gameplay.Level
{
    public class FigureComponent : MonoBehaviour
    {
        [SerializeField] private string levelId;
        [SerializeField] private string figureId;
        [SerializeField] private FigureType figureType;
        [SerializeField] private List<PathComponent> paths;
        [SerializeField] private ViewComponent view;
        
        public string LevelId => levelId;
        public string FigureId => figureId;
        public FigureType FigureType => figureType;
        public List<PathComponent> Paths => paths;
        public ViewComponent View => view;

        public void Initialize(LevelEntry levelEntry, List<PathComponent> pathComponents)
        {
            levelId = levelEntry.LevelID;
            figureId = levelEntry.FigureId;
            figureType = levelEntry.FigureType;
            paths = pathComponents;
        }
    }
}
