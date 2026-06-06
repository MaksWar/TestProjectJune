using Cysharp.Threading.Tasks;
using Infrastructure;
using UnityEngine;

namespace Utilities
{
    public static class GameObjectExtensions
    {
        public static async UniTask<T> InstantiateWithInjectAsync<T>(this T prefab, Transform parent = null) where T : Component
        {
            GameObject instance = await prefab.gameObject.InstantiateWithInjectAsync(parent);

            return instance.GetComponent<T>();
        }

        public static async UniTask<T> InstantiateWithInjectAsync<T>(this GameObject prefab, Transform parent = null) where T : Component
        {
            GameObject instance = await prefab.InstantiateWithInjectAsync(parent);

            return instance.GetComponent<T>();
        }

        private static async UniTask<GameObject> InstantiateWithInjectAsync(this GameObject prefab, Transform parent = null)
        {
            var operation = Object.InstantiateAsync(prefab, parent);

            await operation;

            GameObject instance = operation.Result[0];
            GameBootstrapper.DiContainer.InjectGameObject(instance);

            return instance;
        }
    }
}