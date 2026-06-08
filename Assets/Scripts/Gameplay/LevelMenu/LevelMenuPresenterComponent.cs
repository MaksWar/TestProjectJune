using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Level;
using Gameplay.Level.Models.Public;
using Infrastructure.Gameplay;
using Infrastructure.StaticData;
using Infrastructure.Services.SpriteAtlassService;
using Infrastructure.States;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.LevelMenu
{
    public class LevelMenuPresenterComponent : MonoBehaviour
    {
        public const string PrefabName = "MenuLevelsWindow";
        
        [SerializeField] private Transform CategoriesGroupViewContainer;
        [SerializeField] private ScrollRect CategoriesScrollRect;
        [Header("Prefabs")]
        [SerializeField] private CategoriesGroupViewComponent CategoriesGroupViewComponentPrefab;
        [SerializeField] private LevelViewComponent LevelViewComponentPrefab;

        private ILevelCatalogService _levelCatalogService;
        private GameStateMachine _gameStateMachine;
        private ISpriteAtlasService _spriteAtlasService;
        private IStaticDataService _staticDataService;
        private readonly List<CategoriesGroupViewComponent> _categoryGroups = new();

        public async UniTask InitializeAsync(
            ILevelCatalogService levelCatalogService,
            GameStateMachine gameStateMachine,
            ISpriteAtlasService spriteAtlasService,
            IStaticDataService staticDataService)
        {
            _levelCatalogService = levelCatalogService;
            _gameStateMachine = gameStateMachine;
            _spriteAtlasService = spriteAtlasService;
            _staticDataService = staticDataService;

            Clear();

            IReadOnlyList<LevelGroupData> groups = await _levelCatalogService.GetGroupsAsync();
            List<UniTask<CategoriesGroupViewComponent>> createCategoryGroupTasks = new();

            foreach (LevelGroupData group in groups)
            {
                if (group?.Levels == null || group.Levels.Count == 0)
                {
                    continue;
                }

                createCategoryGroupTasks.Add(CreateCategoryGroupAsync(group));
            }

            CategoriesGroupViewComponent[] categoryGroups = await UniTask.WhenAll(createCategoryGroupTasks);
            foreach (CategoriesGroupViewComponent categoryGroup in categoryGroups)
            {
                if (categoryGroup != null)
                {
                    _categoryGroups.Add(categoryGroup);
                }
            }
        }

        private async UniTask<CategoriesGroupViewComponent> CreateCategoryGroupAsync(LevelGroupData group)
        {
            AsyncInstantiateOperation<CategoriesGroupViewComponent> operation = InstantiateAsync(
                CategoriesGroupViewComponentPrefab,
                CategoriesGroupViewContainer);

            await operation.ToUniTask();

            CategoriesGroupViewComponent categoryGroup = operation.Result[0];
            categoryGroup.SetParentScrollRect(CategoriesScrollRect);

            List<UniTask<LevelViewComponent>> createLevelViewTasks = new();

            foreach (LevelData levelData in group.Levels)
            {
                if (levelData == null || string.IsNullOrWhiteSpace(levelData.Id))
                {
                    continue;
                }

                createLevelViewTasks.Add(CreateLevelViewAsync(group.Type, levelData.Id, categoryGroup.LevelViewContainer));
            }

            List<LevelViewComponent> levelViews = new();
            LevelViewComponent[] createdLevelViews = await UniTask.WhenAll(createLevelViewTasks);
            foreach (LevelViewComponent levelView in createdLevelViews)
            {
                if (levelView != null)
                {
                    levelViews.Add(levelView);
                }
            }

            string categoryName = GetConfiguredCategoryName(group, _staticDataService.GetCategoryNameConfig());

            categoryGroup.Initialize(group.Type, categoryName, levelViews);

            return categoryGroup;
        }

        private void Clear()
        {
            _categoryGroups.Clear();

            if (CategoriesGroupViewContainer == null)
            {
                return;
            }

            for (int i = CategoriesGroupViewContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(CategoriesGroupViewContainer.GetChild(i).gameObject);
            }
        }

        private async UniTask<LevelViewComponent> CreateLevelViewAsync(LevelEntry levelEntry, Transform parent)
        {
            AsyncInstantiateOperation<LevelViewComponent> operation = InstantiateAsync(LevelViewComponentPrefab, parent);
            await operation.ToUniTask();

            Sprite sprite = _spriteAtlasService.GetSprite(levelEntry.FigureId, SpriteAtlasService.LevelsLabel);
  
            LevelViewComponent levelView = operation.Result[0];
            levelView.Initialize(levelEntry.FigureType, levelEntry.LevelID, sprite, levelEntry.ViewColor);
            levelView.OnClick += OnLevelSelected;

            return levelView;
        }

        private async UniTask<LevelViewComponent> CreateLevelViewAsync(FigureType type, string id, Transform parent)
        {
            LevelEntry levelEntry = await _levelCatalogService.LoadLevelEntry(type, id);
            if (levelEntry == null)
            {
                return null;
            }

            return await CreateLevelViewAsync(levelEntry, parent);
        }

        private static string GetConfiguredCategoryName(LevelGroupData group, CategoryNamesConfig categoryNamesConfig)
        {
            string categoryName = group.Type.ToString();
            if (categoryNamesConfig == null)
            {
                return categoryName;
            }

            categoryNamesConfig.TryGetCategoryName(group.Type, out string configuredCategoryName);
            
            return configuredCategoryName ?? categoryName;
        }

        private void OnLevelSelected(FigureType type, string id)
        {
            GameplayLevelPayload payload = new(type, id);
            _gameStateMachine.Enter<GameplayLoadState, GameplayLevelPayload>(payload).Forget();
        }
    }
}
