using System.Collections.Generic;
using Gameplay.Level.Models.Public;
using UnityEngine;

namespace Gameplay.Level
{
    public class FigureComponent : MonoBehaviour
    {
        [SerializeField] private ViewComponent view;
        [SerializeField] private PointersHandlerComponent pointersHandlerComponent;
        [SerializeField] private InteractionHandlerComponent interactionHandlerComponent;

        private string _levelId;
        private string _figureId;
        private FigureType _figureType;
        private List<PathComponent> _paths;
        
        public List<PathComponent> Paths => _paths;
        public ViewComponent View => view;
        public PointersHandlerComponent HandlerComponent => pointersHandlerComponent;
        public InteractionHandlerComponent InteractionHandlerComponent => interactionHandlerComponent;

        public void Initialize(LevelEntry levelEntry, List<PathComponent> pathComponents)
        {
            _levelId = levelEntry.LevelID;
            _figureId = levelEntry.FigureId;
            _figureType = levelEntry.FigureType;
            _paths = pathComponents;

            interactionHandlerComponent.Initialize();
        }
        
    }
}
