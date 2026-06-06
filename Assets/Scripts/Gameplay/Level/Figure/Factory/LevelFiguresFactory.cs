using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Level.Models.Public;
using Infrastructure.AssetManagement;
using Infrastructure.Services.SpriteAtlassService;
using UnityEngine;
using Zenject;

namespace Gameplay.Level
{
    public class LevelFiguresFactory : ILevelFiguresFactory
    {
        private const string BaseFigurePrefabPath = "Figures/FigureBase.prefab";

        private readonly IAssetsProvider _assetsProvider;
        private readonly ISpriteAtlasService _spriteAtlasService;
        private readonly IInstantiator _instantiator;

        public LevelFiguresFactory(
            IAssetsProvider assetsProvider,
            ISpriteAtlasService spriteAtlasService,
            IInstantiator instantiator)
        {
            _assetsProvider = assetsProvider;
            _spriteAtlasService = spriteAtlasService;
            _instantiator = instantiator;
        }

        public async UniTask<FigureComponent> CreateFigure(FigureType type, string id)
        {
            LevelEntry levelEntry = await LoadLevelEntry(type, id);
            GameObject prefab = await _assetsProvider.Load<GameObject>(BaseFigurePrefabPath, GetType());

            if (levelEntry == null || prefab == null)
            {
                return null;
            }

            GameObject figureObject = _instantiator.InstantiatePrefab(prefab);
            figureObject.transform.position = Vector2.zero;
            figureObject.transform.rotation = Quaternion.identity;
            figureObject.transform.localScale = Vector3.one;
            
            FigureComponent figureComponent = figureObject.GetComponent<FigureComponent>();

            if (figureComponent == null)
            {
                Debug.LogError($"{nameof(LevelFiguresFactory)}: prefab '{BaseFigurePrefabPath}' has no {nameof(FigureComponent)}.");
                return null;
            }

            InitializeView(figureComponent, levelEntry);

            List<PathComponent> pathComponents = CreatePaths(figureObject.transform, levelEntry);
            figureComponent.Initialize(levelEntry, pathComponents);

            return figureComponent;
        }

        private async UniTask<LevelEntry> LoadLevelEntry(FigureType type, string id)
        {
            string levelPath = GetLevelPath(type, id);
            TextAsset levelAsset = await _assetsProvider.Load<TextAsset>(levelPath, GetType());

            if (levelAsset == null)
            {
                Debug.LogError($"{nameof(LevelFiguresFactory)}: level JSON '{levelPath}' was not found.");
                
                return null;
            }

            LevelEntry levelEntry = JsonUtility.FromJson<LevelEntry>(levelAsset.text);

            if (levelEntry == null)
            {
                Debug.LogError($"{nameof(LevelFiguresFactory)}: level JSON '{levelPath}' has invalid {nameof(LevelEntry)} data.");
                
                return null;
            }

            return levelEntry;
        }

        private void InitializeView(FigureComponent figureComponent, LevelEntry levelEntry)
        {
            Sprite sprite = _spriteAtlasService.GetSprite(levelEntry.FigureId, SpriteAtlasService.LevelsLabel);

            figureComponent.View.Initialize(sprite);
        }

        private static List<PathComponent> CreatePaths(Transform root, LevelEntry levelEntry)
        {
            List<PathEntry> pathEntries = levelEntry.PathEntries ?? new List<PathEntry>();
            pathEntries.Sort((left, right) => left.Order.CompareTo(right.Order));

            List<PathComponent> pathComponents = new(pathEntries.Count);

            foreach (PathEntry pathEntry in pathEntries)
            {
                GameObject pathObject = new($"Path_{pathEntry.Order}");
                pathObject.transform.SetParent(root, false);

                PathComponent pathComponent = pathObject.AddComponent<PathComponent>();
                List<PathPointComponent> pointComponents = CreatePoints(pathObject.transform, pathEntry);
                
                pathComponent.Initialize(pathEntry, pointComponents);

                pathComponents.Add(pathComponent);
            }

            return pathComponents;
        }

        private static List<PathPointComponent> CreatePoints(Transform pathRoot, PathEntry pathEntry)
        {
            List<Vector2> path = pathEntry.Path ?? new List<Vector2>();
            List<PathPointComponent> pointComponents = new(path.Count);

            for (int i = 0; i < path.Count; i++)
            {
                GameObject pointObject = new($"Point_{i}");
                pointObject.transform.SetParent(pathRoot, false);
                pointObject.transform.localPosition = path[i];

                PathPointComponent pointComponent = pointObject.AddComponent<PathPointComponent>();
                pointComponents.Add(pointComponent);
            }

            return pointComponents;
        }

        private string GetLevelPath(FigureType type, string id) =>
            $"Levels/{type}/{id}.json";
    }
}
