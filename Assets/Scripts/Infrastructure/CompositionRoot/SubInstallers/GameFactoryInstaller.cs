using Infrastructure.Factories;
using Zenject;

namespace Infrastructure.CompositionRoot.SubInstallers
{
    public class GameFactoryInstaller : Installer<GameFactoryInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IUIFactory>().To<UIFactory>().AsSingle();
        }
    }
}