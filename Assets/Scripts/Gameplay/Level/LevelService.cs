using Cysharp.Threading.Tasks;
using Infrastructure.Gameplay;
using Infrastructure.Gameplay.States;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Level
{
    public class LevelService
    {
        private const float FirstCompletedPointerProgress = 0.0001f;

        private readonly SceneStateMachine _sceneStateMachine;

        private FigureComponent _currentFigure;

        private int _currentPathIndex;
        private int _currentPointerIndex;

        public LevelService(SceneStateMachine sceneStateMachine)
        {
            _sceneStateMachine = sceneStateMachine;
        }

        public IReadOnlyList<Vector2> CurrentPathPositions =>
            HasCurrentPath() ? _currentFigure.Paths[_currentPathIndex].Path : null;

        public async UniTask Activate(FigureComponent figureComponent)
        {
            UnsubscribeCurrentFigure();

            _currentFigure = figureComponent;
            _currentPathIndex = 0;
            _currentPointerIndex = 0;

            SubscribeCurrentFigure();
            _currentFigure.LetterTracingController?.InitializeParts(_currentFigure.Paths);

            await ActivateCurrentPath();
        }

        public async UniTask ActivateCurrentPath()
        {
            if (HasCurrentPath() == false)
            {
                await _sceneStateMachine.Enter<WinGameplayState>();
                
                return;
            }

            PointersHandlerComponent pointersHandler = _currentFigure.HandlerComponent;
            PathComponent currentPath = _currentFigure.Paths[_currentPathIndex];

            _currentFigure.LetterTracingController?.SetActivePart(_currentPathIndex);
            _currentFigure.LetterTracingController?.SetPartProgress(_currentPathIndex, 0f);

            await pointersHandler.CreatePathPointers(currentPath.Path);
            await pointersHandler.ShowCurrentPath();

            _currentPointerIndex = 0;

            ActivateCurrentPointer();
        }

        private void OnPointerInteracted(IDraggingInteractable interactable)
        {
            if (!IsCurrentPointer(interactable))
            {
                return;
            }

            AdvancePointerOrPath().Forget();
        }

        private async UniTask AdvancePointerOrPath()
        {
            FigurePointerComponent pointerComponent = _currentFigure.HandlerComponent.GetPointer(_currentPointerIndex);
            pointerComponent.Deactivate();
            pointerComponent.Hide();

            UpdateTracingProgress(_currentPointerIndex);
            
            _currentPointerIndex++;

            if (_currentPointerIndex < _currentFigure.HandlerComponent.Pointers.Count)
            {
                ActivateCurrentPointer();

                return;
            }

            _currentFigure.HandlerComponent.DeactivateCurrentPath();
            _currentFigure.LetterTracingController?.CompletePart(_currentPathIndex);
            _currentPathIndex++;
            _currentPointerIndex = 0;

            await ActivateCurrentPath();
        }

        private bool IsCurrentPointer(IDraggingInteractable interactable)
        {
            if (_currentFigure == null || interactable == null)
            {
                return false;
            }

            FigurePointerComponent currentPointer = _currentFigure.HandlerComponent.GetPointer(_currentPointerIndex);
            return currentPointer != null && ReferenceEquals(currentPointer.DraggingInteractable, interactable);
        }

        private void UnsubscribeCurrentFigure()
        {
            if (_currentFigure == null)
            {
                return;
            }

            _currentFigure.InteractionHandlerComponent.Interacted -= OnPointerInteracted;
        }

        private bool HasCurrentPath() =>
            _currentFigure?.Paths != null && _currentPathIndex < _currentFigure.Paths.Count;

        private void ActivateCurrentPointer() =>
            _currentFigure.HandlerComponent.ActivatePointer(_currentPointerIndex);

        private void UpdateTracingProgress(int completedPointerIndex)
        {
            int pointerCount = _currentFigure.HandlerComponent.Pointers.Count;
            float progress = pointerCount <= 1
                ? 1f
                : Mathf.Max(FirstCompletedPointerProgress, (float)completedPointerIndex / (pointerCount - 1));

            _currentFigure.LetterTracingController?.SetPartProgress(_currentPathIndex, progress);
        }

        private void SubscribeCurrentFigure() =>
            _currentFigure.InteractionHandlerComponent.Interacted += OnPointerInteracted;
    }
}
