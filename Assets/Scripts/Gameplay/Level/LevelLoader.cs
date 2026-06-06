using Cysharp.Threading.Tasks;

namespace Gameplay.Level
{
    public class LevelLoader : ILevelLoader
    {
        private readonly ILevelFiguresFactory _levelFiguresFactory;
        private readonly LevelService _levelService;

        public LevelLoader(ILevelFiguresFactory levelFiguresFactory, LevelService levelService)
        {
            _levelFiguresFactory = levelFiguresFactory;
            _levelService = levelService;
        }

        public async UniTask<FigureComponent> LoadLevel(FigureType type, string id)
        {
            FigureComponent figureComponent = await _levelFiguresFactory.CreateFigure(type, id);

            await _levelService.Activate(figureComponent);

            return figureComponent;
        }
    }
}
