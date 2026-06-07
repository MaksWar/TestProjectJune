using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Gameplay.Level
{
    public class PointersHandlerComponent : MonoBehaviour
    {
        [SerializeField] private PointersHandlerAnimationComponent pointersHandlerAnimationComponent;
        
        private readonly List<FigurePointerComponent> _pointers = new();

        private IFigurePointersFactory _pointersFactory;

        [Inject]
        private void Construct(IFigurePointersFactory figurePointersFactory) =>
            _pointersFactory = figurePointersFactory;

        public IReadOnlyList<FigurePointerComponent> Pointers => _pointers;

        private void Awake() =>
            EnsurePointersHandlerAnimation(true);

        public UniTask CreatePathPointers(List<Vector2> positions) =>
            CreatePathPointersAsync(positions);

        public UniTask ShowCurrentPath() =>
            pointersHandlerAnimationComponent.ShowCurrentPath(_pointers);

        public void DeactivateCurrentPath()
        {
            foreach (FigurePointerComponent pointer in _pointers)
            {
                pointer?.Deactivate();
            }
        }

        public void ActivatePointer(int pointerIndex)
        {
            DeactivateCurrentPath();

            if (pointerIndex < 0 || pointerIndex >= _pointers.Count)
            {
                return;
            }

            _pointers[pointerIndex]?.Activate();
        }

        public FigurePointerComponent GetPointer(int pointerIndex)
        {
            if (pointerIndex < 0 || pointerIndex >= _pointers.Count)
            {
                return null;
            }

            return _pointers[pointerIndex];
        }

        private async UniTask CreatePathPointersAsync(List<Vector2> positions)
        {
            Clear();

            if (positions == null || positions.Count == 0)
            {
                return;
            }

            List<UniTask<FigurePointerComponent>> createPointerTasks = new(positions.Count);

            for (int i = 0; i < positions.Count; i++)
            {
                PointerType pointerType = i == positions.Count - 1 ? PointerType.Final : PointerType.Default;
                createPointerTasks.Add(CreatePointer(positions[i], pointerType));
            }

            FigurePointerComponent[] pointers = await UniTask.WhenAll(createPointerTasks);
            foreach (FigurePointerComponent pointer in pointers)
            {
                StorePointer(pointer);
            }
        }

        private async UniTask<FigurePointerComponent> CreatePointer(Vector2 position, PointerType pointerType) =>
            await _pointersFactory.CreatePointer(pointerType, position, transform);

        private void StorePointer(FigurePointerComponent pointer)
        {
            if (pointer == null)
            {
                return;
            }

            pointer.Hide();
            pointer.Deactivate();

            _pointers.Add(pointer);
        }

        private void Clear()
        {
            foreach (FigurePointerComponent pointer in _pointers)
            {
                if (pointer != null)
                {
                    Destroy(pointer.gameObject);
                }
            }

            _pointers.Clear();
        }

        #region Editor

        private void OnValidate()
        {
            EnsurePointersHandlerAnimation(false);
        }

        private void EnsurePointersHandlerAnimation(bool addIfMissing)
        {
            if (pointersHandlerAnimationComponent != null)
            {
                return;
            }

            pointersHandlerAnimationComponent = GetComponentInChildren<PointersHandlerAnimationComponent>();
            if (pointersHandlerAnimationComponent == null && addIfMissing)
            {
                pointersHandlerAnimationComponent = gameObject.AddComponent<PointersHandlerAnimationComponent>();
            }
        }

        #endregion
    }
}
