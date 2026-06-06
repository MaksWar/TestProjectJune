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
        private void Construct(IFigurePointersFactory figurePointersFactory)
        {
            _pointersFactory = figurePointersFactory;
        }

        public UniTask Activate(List<Vector2> positions) =>
            ActivateAsync(positions);

        private async UniTask ActivateAsync(List<Vector2> positions)
        {
            Clear();

            if (positions == null || positions.Count == 0)
            {
                return;
            }

            await CreateAndStorePointer(positions.Last(), PointerType.Final);
            for (int i = 0; i < positions.Count - 1; i++)
            {
                await CreateAndStorePointer(positions[i], PointerType.Default);
            }
        }

        private async UniTask CreateAndStorePointer(Vector2 position, PointerType pointerType)
        {
            FigurePointerComponent pointer = await _pointersFactory.CreatePointer(pointerType, position, transform);
            if (pointer != null)
            {
                _pointers.Add(pointer);
            }
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
