using Infrastructure.Factories;
using UnityEngine;
using Zenject;

namespace Infrastructure.Gameplay
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [SerializeField] private Camera backgroundCamera;

        public override void InstallBindings()
        {
            Debug.Log("Start game scene installer");

            Container
                .BindInterfacesAndSelfTo<GameplaySceneBootstraper>()
                .AsSingle()
                .NonLazy();

            Container.Bind<StatesFactory>().AsSingle();
            Container.Bind<SceneStateMachine>().AsSingle();
        }
    }
}
