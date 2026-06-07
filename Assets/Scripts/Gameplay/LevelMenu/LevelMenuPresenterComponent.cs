using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Level;
using Gameplay.Level.Models.Public;
using Infrastructure.Gameplay;
using Infrastructure.Services.SpriteAtlassService;
using Infrastructure.States;
using UnityEngine;

namespace Gameplay.LevelMenu
{
    public class LevelMenuPresenterComponent : MonoBehaviour
    {
        public const string PrefabName = "MenuLevelsWindow";
        
        [SerializeField] private Transform CategoriesGroupViewContainer;
        [Header("Prefabs")]
        [SerializeField] private CategoriesGroupViewComponent CategoriesGroupViewComponentPrefab;
        [SerializeField] private LevelViewComponent LevelViewComponentPrefab;

        private ILevelCatalogService _levelCatalogService;
        private GameStateMachine _gameStateMachine;
        private ISpriteAtlasService _spriteAtlasService;
        private readonly List<CategoriesGroupViewComponent> _categoryGroups = new();

        public async UniTask InitializeAsync(
            ILevelCatalogService levelCatalogService,
            GameStateMachine gameStateMachine,
            ISpriteAtlasService spriteAtlasService)
        {
            _levelCatalogService = levelCatalogService;
            _gameStateMachine = gameStateMachine;
            _spriteAtlasService = spriteAtlasService;

            Clear();

            IReadOnlyList<LevelGroupData> groups = await _levelCatalogService.GetGroupsAsync();
            foreach (LevelGroupData group in groups)
            {
                if (group?.Levels == null || group.Levels.Count == 0)
                {
                    continue;
                }

                CategoriesGroupViewComponent categoryGroup = await CreateCategoryGroupAsync(group);

                _categoryGroups.Add(categoryGroup);
            }
        }

        private async UniTask<CategoriesGroupViewComponent> CreateCategoryGroupAsync(LevelGroupData group)
        {
            AsyncInstantiateOperation<CategoriesGroupViewComponent> operation = InstantiateAsync(
                CategoriesGroupViewComponentPrefab,
                CategoriesGroupViewContainer);
            await operation.ToUniTask();

            CategoriesGroupViewComponent categoryGroup = operation.Result[0];

            List<LevelViewComponent> levelViews = new();
            foreach (LevelData levelData in group.Levels)
            {
                if (levelData == null || string.IsNullOrWhiteSpace(levelData.Id))
                {
                    continue;
                }

                LevelEntry levelEntry = await _levelCatalogService.LoadLevelEntry(group.Type, levelData.Id);
                if (levelEntry == null)
                {
                    continue;
                }

                LevelViewComponent levelView = await CreateLevelViewAsync(levelEntry, categoryGroup.LevelViewContainer);

                levelViews.Add(levelView);
            }

            categoryGroup.Initialize(group.Type, levelViews);

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

        private void OnLevelSelected(FigureType type, string id)
        {
            GameplayLevelPayload payload = new(type, id);
            _gameStateMachine.Enter<GameplayLoadState, GameplayLevelPayload>(payload).Forget();
        }
    }
}
