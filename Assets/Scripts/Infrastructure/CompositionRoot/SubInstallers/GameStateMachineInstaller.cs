using Infrastructure.Factories;
using Infrastructure.States;
using Zenject;

namespace Infrastructure.CompositionRoot.SubInstallers
{
    public class GameStateMachineInstaller : Installer<GameStateMachineInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<StatesFactory>().AsSingle();
            
            Container.Bind<GameStateMachine>().AsSingle().NonLazy();
        }
    }
}