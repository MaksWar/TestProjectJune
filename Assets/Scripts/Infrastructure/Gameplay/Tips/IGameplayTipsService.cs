using Gameplay.Level;

namespace Infrastructure.Gameplay.Tips
{
    public interface IGameplayTipsService
    {
        void Start(FigureType figureType, FigureComponent figureComponent);
        void Stop();
    }
}
