using System.Collections.Generic;
using Gameplay.Level.Figure.PaintShader;
using Gameplay.Level.Models.Public;
using UnityEngine;

namespace Gameplay.Level
{
    public class FigureComponent : MonoBehaviour
    {
        [SerializeField] private ViewComponent view;
        [SerializeField] private ViewComponent backgroundView;
        [SerializeField] private PointersHandlerComponent pointersHandlerComponent;
        [SerializeField] private InteractionHandlerComponent interactionHandlerComponent;
        [SerializeField] private LetterTracingController letterTracingController;
        [SerializeField] private FigureCameraHeightFitter cameraHeightFitter;

        private string _levelId;
        private string _figureId;
        private FigureType _figureType;
        private List<PathComponent> _paths;
        
        public List<PathComponent> Paths => _paths;
        public ViewComponent View => view;
        public ViewComponent BackgroundView => backgroundView;
        public PointersHandlerComponent HandlerComponent => pointersHandlerComponent;
        public InteractionHandlerComponent InteractionHandlerComponent => interactionHandlerComponent;
        public LetterTracingController LetterTracingController => letterTracingController;

        public void Initialize(LevelEntry levelEntry, List<PathComponent> pathComponents, Camera gameplayCamera)
        {
            _levelId = levelEntry.LevelID;
            _figureId = levelEntry.FigureId;
            _figureType = levelEntry.FigureType;
            _paths = pathComponents;

            interactionHandlerComponent.Initialize();
            if (cameraHeightFitter == null)
            {
                return;
            }

            cameraHeightFitter.SetCamera(gameplayCamera);
            cameraHeightFitter.Fit();
        }
        
    }
}
