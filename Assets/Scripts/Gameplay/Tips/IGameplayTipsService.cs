using Gameplay.Level;

namespace Gameplay.Tips
{
    public interface IGameplayTipsService
    {
        void Start(FigureType figureType, FigureComponent figureComponent);
        void Stop();
    }
}
