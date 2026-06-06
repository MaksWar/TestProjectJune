using Cysharp.Threading.Tasks;

namespace Gameplay.Level
{
    public interface ILevelLoader
    {
        UniTask<FigureComponent> LoadLevel(FigureType type, string id);
    }
}
