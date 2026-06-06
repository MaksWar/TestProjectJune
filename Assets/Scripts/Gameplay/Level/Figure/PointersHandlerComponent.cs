using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Gameplay.Level
{
    public class PointersHandlerComponent : MonoBehaviour
    {
        private readonly List<FigurePointerComponent> _pointers = new();

        private IFigurePointersFactory _pointersFactory;

        [Inject]
        private void Construct(IFigurePointersFactory figurePointersFactory) =>
            _pointersFactory = figurePointersFactory;

        public IReadOnlyList<FigurePointerComponent> Pointers => _pointers;

        public UniTask CreatePathPointers(List<Vector2> positions) =>
            CreatePathPointersAsync(positions);

        public void ShowCurrentPath()
        {
            foreach (FigurePointerComponent pointer in _pointers)
            {
                pointer?.Show();
            }
        }

        public void HideCurrentPath()
        {
            foreach (FigurePointerComponent pointer in _pointers)
            {
                pointer?.Hide();
            }
        }

        public void ActivateCurrentPath()
        {
            foreach (FigurePointerComponent pointer in _pointers)
            {
                pointer?.Activate();
            }
        }

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

            for (int i = 0; i < positions.Count; i++)
            {
                PointerType pointerType = i == positions.Count - 1 ? PointerType.Final : PointerType.Default;
                await CreateAndStorePointer(positions[i], pointerType);
            }
        }

        private async UniTask CreateAndStorePointer(Vector2 position, PointerType pointerType)
        {
            FigurePointerComponent pointer = await _pointersFactory.CreatePointer(pointerType, position, transform);
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
    }
}
