using Cysharp.Threading.Tasks;
using Gameplay.Level;
using Gameplay.MetaGameplay.Player.Models.Private;
using Infrastructure.AssetManagement;
using Infrastructure.CompositionRoot.SubInstallers;
using Infrastructure.Factories;
using Infrastructure.Gameplay;
using Infrastructure.SceneMenegment;
using Infrastructure.Services.Log;
using Infrastructure.Services.SaveLoadSystem;
using Infrastructure.Services.SaveLoadSystem.AuthService;
using Infrastructure.Services.SoundService;
using Infrastructure.Services.SpriteAtlassService;
using Infrastructure.StaticData;
using Infrastructure.UI;
using Infrastructure.UI.LoadingCurtain;
using Infrastructure.UI.LoadingCurtain.Proxy;
using Zenject;

namespace Infrastructure.CompositionRoot
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindInfrastructureComponents();
            
            Container.Bind<ISpriteAtlasService>().To<SpriteAtlasService>().AsSingle();
            Container.Bind<ISoundService>().To<SoundService>().AsSingle();
        }

        private void BindInfrastructureComponents()
        {
            BindSaveLoad();
            
            GameStateMachineInstaller.Install(Container);

            Container.BindInterfacesTo<AssetsProvider>().AsSingle();
            
            Container.BindInterfacesTo<StaticDataService>().AsSingle();
            Container.Bind<ILevelCatalogService>().To<LevelCatalogService>().AsSingle();

            Container.BindInterfacesTo<LogService>().AsSingle();

            Container.BindInterfacesTo<SceneLoader>().AsSingle();
            Container.Bind<IGameplayContextService>().To<GameplayContextService>().AsSingle();
            Container.Bind<IUIService>().To<UIService>().AsSingle();
            
            BindGameBootstrapperFactory();

            BindLoadingCurtains();
            
            BindGameFactory();
        }

        private void BindSaveLoad()
        {
            // Models
            Container.Bind<IPrivateModel>().To<PlayerPrivateModel>().AsSingle();
            
            // Services
            Container.Bind<IPrivateModelProvider>().To<PrivateModelProvider>().AsSingle();
            Container.Bind<ISaveLoadService>().To<SaveLoadService>().AsSingle();
            Container.Bind<ISaveLoadManager>().To<SaveLoadManager>().AsSingle();
            Container.Bind<IAuthService>().To<AuthService>().AsSingle();
        }
        
        private void BindGameFactory()
        {
            Container
                .Bind<IUIFactory>()
                .FromSubContainerResolve()
                .ByInstaller<GameFactoryInstaller>()
                .AsSingle();
        }
        
        private void BindGameBootstrapperFactory()
        {
            Container
                .BindFactory<GameBootstrapper, GameBootstrapper.Factory>()
                .FromComponentInNewPrefabResource(InfrastructureAssetPath.GameBootstraper);
        }
        
        private void BindLoadingCurtains()
        {
            Container.BindFactory<string, UniTask<LoadingCurtain>, LoadingCurtain.Factory>()
                .FromFactory<PrefabFactoryAsync<LoadingCurtain>>();

            Container.BindInterfacesAndSelfTo<LoadingCurtainProxy>().AsSingle();
        }

    }
}
