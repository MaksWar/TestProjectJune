using Infrastructure.Factories;
using Gameplay.Level;
using Infrastructure.Services.Input;
using UnityEngine;
using Zenject;

namespace Infrastructure.Gameplay
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [SerializeField] private Camera gameplayCamera;

        public override void InstallBindings()
        {
            Debug.Log("Start game scene installer");

            Container
                .BindInterfacesAndSelfTo<GameplaySceneBootstraper>()
                .AsSingle()
                .NonLazy();

            Container.Bind<StatesFactory>().AsSingle();
            Container.Bind<SceneStateMachine>().AsSingle();
            Container.Bind<LevelService>().AsSingle();
            Container.Bind<ILevelFiguresFactory>().To<LevelFiguresFactory>().AsSingle();
            Container.Bind<IFigurePointersFactory>().To<FigurePointersFactory>().AsSingle();
            Container.Bind<ILevelLoader>().To<LevelLoader>().AsSingle();
            BindInputService();
        }

        private void BindInputService()
        {
            Camera inputCamera = gameplayCamera != null ? gameplayCamera : Camera.main;
            if (inputCamera != null)
            {
                Container.Bind<Camera>().FromInstance(inputCamera).AsSingle();
            }

#if UNITY_EDITOR
            Container.BindInterfacesAndSelfTo<EditorInputService>().AsSingle();
#else
            Container.BindInterfacesAndSelfTo<MobileInputService>().AsSingle();
#endif
        }
    }
}
