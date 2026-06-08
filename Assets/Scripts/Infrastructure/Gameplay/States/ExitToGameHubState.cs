using Cysharp.Threading.Tasks;
using Infrastructure.States;

namespace Infrastructure.Gameplay.States
{
    public class ExitToGameHubState : IState
    {
        private readonly GameStateMachine _gameStateMachine;

        public ExitToGameHubState(GameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }
        
        public async UniTask Enter()
        {
        }

        public UniTask Exit()
        {
            return default;
        }
    }
}
