using Cysharp.Threading.Tasks;

namespace Gameplay.Level
{
    public class LevelLoader : ILevelLoader
    {
        private readonly ILevelFiguresFactory _levelFiguresFactory;

        public LevelLoader(ILevelFiguresFactory levelFiguresFactory)
        {
            _levelFiguresFactory = levelFiguresFactory;
        }

        public UniTask<FigureComponent> LoadLevel(FigureType type, string id)
        {
            return _levelFiguresFactory.CreateFigure(type, id);
        }
    }
}
