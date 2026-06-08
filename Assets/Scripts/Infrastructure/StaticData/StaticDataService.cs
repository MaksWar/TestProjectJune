using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using UnityEngine;

namespace Infrastructure.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private const string CategoryNamesConfigPath = "StaticData/CategoryNamesConfig.asset";
        private const string GameplayTipsEntryPath = "StaticData/GameplayTipsEntry.asset";

        private readonly IAssetsProvider _assetsProvider;
        private CategoryNamesConfig _categoryNamesConfig;
        private GameplayTipsEntry _gameplayTipsEntry;

        public StaticDataService(IAssetsProvider assetsProvider)
        {
            _assetsProvider = assetsProvider;
        }

        public async UniTask LoadAllAsync()
        {
            await LoadCategoryNamesConfigAsync();
            await LoadGameplayTipsEntryAsync();
        }

        public CategoryNamesConfig GetCategoryNameConfig() =>
            _categoryNamesConfig;

        public GameplayTipsEntry GetGameplayTipsEntry() =>
            _gameplayTipsEntry;

        private async UniTask LoadCategoryNamesConfigAsync()
        {
            _categoryNamesConfig = await _assetsProvider.Load<CategoryNamesConfig>(CategoryNamesConfigPath, GetType());
            if (_categoryNamesConfig == null)
            {
                Debug.LogError($"{nameof(StaticDataService)}: failed to load category names config at '{CategoryNamesConfigPath}'.");
            }
        }

        private async UniTask LoadGameplayTipsEntryAsync()
        {
            _gameplayTipsEntry = await _assetsProvider.Load<GameplayTipsEntry>(GameplayTipsEntryPath, GetType());
            if (_gameplayTipsEntry == null)
            {
                Debug.LogError($"{nameof(StaticDataService)}: failed to load gameplay tips entry at '{GameplayTipsEntryPath}'.");
            }
        }
    }
}
