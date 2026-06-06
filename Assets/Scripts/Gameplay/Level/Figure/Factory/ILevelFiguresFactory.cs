using Cysharp.Threading.Tasks;

namespace Gameplay.Level
{
    public interface ILevelFiguresFactory
    {
        UniTask<FigureComponent> CreateFigure(FigureType type, string id);
    }
}