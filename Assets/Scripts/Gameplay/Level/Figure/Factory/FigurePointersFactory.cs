using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace Gameplay.Level
{
    public class FigurePointersFactory : IFigurePointersFactory
    {
        private const string PointersPath = "LevelPointers/";
        private const string DefaultPointer = "circle_pointer";
        private const string FinalPointer = "star_pointer";

        private readonly IAssetsProvider _assetsProvider;
        private readonly IInstantiator _instantiator;

        public FigurePointersFactory(IAssetsProvider assetsProvider, IInstantiator instantiator)
        {
            _assetsProvider = assetsProvider;
            _instantiator = instantiator;
        }

        public async UniTask<FigurePointerComponent> CreatePointer(PointerType type, Vector2 position, Transform parent)
        {
            string pointerPath = GetPointerPath(type);
            GameObject prefab = await _assetsProvider.Load<GameObject>(pointerPath, GetType());

            if (prefab == null)
            {
                Debug.LogError($"{nameof(FigurePointersFactory)}: pointer prefab '{pointerPath}' was not found.");
                return null;
            }

            GameObject pointerObject = _instantiator.InstantiatePrefab(prefab);
            pointerObject.transform.SetParent(parent, false);
            pointerObject.transform.localPosition = position;

            FigurePointerComponent pointerComponent = pointerObject.GetComponent<FigurePointerComponent>();

            return pointerComponent;
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
