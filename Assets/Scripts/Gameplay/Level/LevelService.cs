using Cysharp.Threading.Tasks;

namespace Gameplay.Level
{
    public class LevelService
    {
        public async UniTask Activate(FigureComponent figureComponent)
        {
            PointersHandlerComponent pointersHandler = figureComponent.HandlerComponent;
            
            await pointersHandler.Activate(figureComponent.Paths[0].Path);
        }
    }
}
