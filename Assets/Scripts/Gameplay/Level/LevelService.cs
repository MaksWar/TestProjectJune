using Cysharp.Threading.Tasks;

namespace Gameplay.Level
{
    public class LevelService
    {
        private FigureComponent _currentFigure;

        private int _currentPathIndex;
        private int _currentPointerIndex;

        public async UniTask Activate(FigureComponent figureComponent)
        {
            UnsubscribeCurrentFigure();

            _currentFigure = figureComponent;
            _currentPathIndex = 0;
            _currentPointerIndex = 0;

            SubscribeCurrentFigure();

            await ActivateCurrentPath();
        }

        public async UniTask ActivateCurrentPath()
        {
            if (HasCurrentPath() == false)
            {
                return;
            }

            PointersHandlerComponent pointersHandler = _currentFigure.HandlerComponent;
            PathComponent currentPath = _currentFigure.Paths[_currentPathIndex];

            _currentFigure.LetterTracingController?.InitializePath(currentPath.Path);

            await pointersHandler.CreatePathPointers(currentPath.Path);
            pointersHandler.ShowCurrentPath();

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
            _currentPathIndex < _currentFigure.Paths.Count;

        private void ActivateCurrentPointer() =>
            _currentFigure.HandlerComponent.ActivatePointer(_currentPointerIndex);

        private void UpdateTracingProgress(int completedPointerIndex)
        {
            int pointerCount = _currentFigure.HandlerComponent.Pointers.Count;
            float progress = pointerCount <= 1 ? 1f : (float)completedPointerIndex / (pointerCount - 1);

            _currentFigure.LetterTracingController?.SetProgress(progress);
        }

        private void SubscribeCurrentFigure() =>
            _currentFigure.InteractionHandlerComponent.Interacted += OnPointerInteracted;
    }
}
