using Gameplay.Level;

namespace Infrastructure.Gameplay.Tips
{
    public readonly struct GameplayTipContext
    {
        public readonly FigureType FigureType;
        public readonly FigureComponent FigureComponent;

        public GameplayTipContext(FigureType figureType, FigureComponent figureComponent)
        {
            FigureType = figureType;
            FigureComponent = figureComponent;
        }
    }
}
