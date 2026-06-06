using Gameplay.Level;

namespace Infrastructure.Gameplay
{
    public readonly struct GameplayLevelPayload
    {
        public readonly FigureType FigureType;
        public readonly string LevelId;

        public GameplayLevelPayload(FigureType figureType, string levelId)
        {
            FigureType = figureType;
            LevelId = levelId;
        }
    }
}
