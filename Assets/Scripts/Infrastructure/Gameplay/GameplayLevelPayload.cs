using Gameplay.Level;

namespace Infrastructure.Gameplay
{
    public readonly struct GameplayLevelPayload
    {
        public readonly FigureType FigureType;
        public readonly string LevelId;
        public readonly FigureComponent FigureComponent;

        public GameplayLevelPayload(FigureType figureType, string levelId, FigureComponent figureComponent = null)
        {
            FigureType = figureType;
            LevelId = levelId;
            FigureComponent = figureComponent;
        }
    }
}
