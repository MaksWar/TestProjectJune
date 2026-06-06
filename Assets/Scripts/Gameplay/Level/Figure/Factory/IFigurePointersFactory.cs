using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Level
{
    public interface IFigurePointersFactory
    {
        UniTask<FigurePointerComponent> CreatePointer(PointerType type, Vector2 position, Transform parent);
    }
}
