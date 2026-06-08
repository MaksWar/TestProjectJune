using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Level.Models.Public;

namespace Gameplay.Level
{
    public interface ILevelCatalogService
    {
        UniTask<IReadOnlyList<LevelGroupData>> GetGroupsAsync();
        UniTask<LevelData> GetNextLevel(FigureType type, string currentId);
        UniTask<LevelEntry> LoadLevelEntry(FigureType type, string id);
    }
}
