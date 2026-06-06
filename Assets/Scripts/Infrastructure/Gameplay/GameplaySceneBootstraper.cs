using Cysharp.Threading.Tasks;
using Infrastructure.Factories;
using Infrastructure.Gameplay.States;
using Zenject;

namespace Infrastructure.Gameplay
{
    public class GameplaySceneBootstraper : IInitializable
    {
        private readonly StatesFactory statesFactory;
        private readonly SceneStateMachine sceneStateMachine;

        public GameplaySceneBootstraper(SceneStateMachine sceneStateMachine, StatesFactory statesFactory)
        {
            this.sceneStateMachine = sceneStateMachine;
            this.statesFactory = statesFactory;
        }

        public void Initialize()
        {
            sceneStateMachine.RegisterState(statesFactory.Create<WinGameplayState>());
            sceneStateMachine.RegisterState(statesFactory.Create<ExitToGameHubState>());
            sceneStateMachine.RegisterState(statesFactory.Create<GameLoopState>());
            sceneStateMachine.RegisterState(statesFactory.Create<InitializeGameplayState>());
            sceneStateMachine.RegisterState(statesFactory.Create<AwaitState>());

            sceneStateMachine.Enter<AwaitState>().Forget();
        }
    }
}
