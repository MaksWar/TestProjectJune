using Cysharp.Threading.Tasks;

namespace Infrastructure.States
{
    public class LevelsMenuState : IState
    {
        public async UniTask Enter()
        {
        }

        public UniTask Exit() =>
            UniTask.CompletedTask;
    }
}