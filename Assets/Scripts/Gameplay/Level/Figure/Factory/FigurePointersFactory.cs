using Cysharp.Threading.Tasks;
using UnityEngine;
using Utilities.Pool;

namespace Gameplay.Level
{
    public class FigurePointersFactory : IFigurePointersFactory
    {
        private const string PointersPath = "LevelPointers/";
        private const string DefaultPointer = "circle_pointer";
        private const string FinalPointer = "star_pointer";

        private readonly IObjectPool<FigurePointerComponent> _pointersPool;

        public FigurePointersFactory(IObjectPool<FigurePointerComponent> pointersPool)
        {
            _pointersPool = pointersPool;
        }

        public async UniTask<FigurePointerComponent> CreatePointer(PointerType type, Vector2 position, Transform parent)
        {
            string pointerPath = GetPointerPath(type);
            FigurePointerComponent pointerComponent = await _pointersPool.Pop(pointerPath);

            if (pointerComponent == null)
            {
                Debug.LogError($"{nameof(FigurePointersFactory)}: pointer prefab '{pointerPath}' was not found.");
                return null;
            }

            pointerComponent.transform.SetParent(parent, false);
            pointerComponent.transform.localPosition = position;

            return pointerComponent;
        }

        public void ReleasePointer(FigurePointerComponent pointer)
        {
            if (pointer == null)
            {
                return;
            }

            _pointersPool.Push(pointer);
        }

        private static string GetPointerPath(PointerType type) =>
            PointersPath + GetPointerName(type) + ".prefab";

        private static string GetPointerName(PointerType type) =>
            type switch
            {
                PointerType.Default => DefaultPointer,
                PointerType.Final => FinalPointer,
                _ => DefaultPointer
            };
    }
}
