using Infrastructure.States;
using Cysharp.Threading.Tasks;
using Infrastructure.UI.LoadingCurtain;

namespace Infrastructure.Gameplay.States
{
    public class InitializeGameplayState : IState
    {

        private readonly SceneStateMachine _stateMachine;
        private ILoadingCurtain _loadingCurtain;

        public InitializeGameplayState(SceneStateMachine stateMachine, ILoadingCurtain loadingCurtain)
        {
            _stateMachine = stateMachine;
            _loadingCurtain = loadingCurtain;
        }

        public async UniTask Enter()
        {
            _loadingCurtain.Hide();
            
            await _stateMachine.Enter<GameLoopState>();
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}
