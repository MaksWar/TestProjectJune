using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Utilities.Pool
{
    public abstract class ObjectPool<T> : IObjectPool<T> where T : Component, IPoolableObject
    {
        protected Func<T, UniTask<T>> CreateObjectFunc;

        private readonly Dictionary<T, Queue<T>> _pools = new();
        private readonly Dictionary<T, T> _instanceToPrefab = new();
        private readonly IAssetsProvider _assetsProvider;
        private readonly DiContainer container;

        public ObjectPool(IAssetsProvider assetsProvider, DiContainer container)
        {
            _assetsProvider = assetsProvider;
            this.container = container;

            CreateObjectFunc = CreateObject;
        }

        public async UniTask<T> Pop(string path)
        {
            var prefab = await _assetsProvider.LoadPrefab<T>(path, GetType());
            
            return await Pop(prefab);
        }

        public async UniTask<T> Pop(T prefab)
        {
            if (!_pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<T>();
                _pools[prefab] = queue;
            }

            if (queue.Count > 0)
            {
                var instance = queue.Dequeue();
                instance.gameObject.SetActive(true);

                instance.OnPop();
                return instance;
            }

            var newInstance = await CreateObjectFunc(prefab);
            _instanceToPrefab[newInstance] = prefab;

            newInstance.OnPop();
            return newInstance;
        }

        public void Push(T instance)
        {
            instance.OnPush();
            instance.gameObject.SetActive(false);

            if (!_instanceToPrefab.TryGetValue(instance, out var prefab))
            {
                Debug.LogWarning($"[GenericPrefabPool] Returned object not created by this pool: {instance.name}");
                Object.Destroy(instance.gameObject); // або ігнорувати

                return;
            }

            _pools[prefab].Enqueue(instance);
        }

        public void Unload()
        {
            foreach (var pool in _pools)
            {
                foreach (var pooledObject in pool.Value)
                {
                    if (pooledObject.gameObject)
                    {
                        Object.Destroy(pooledObject.gameObject);
                    }
                }
            }
            
            _pools.Clear();
        }

        public void Preload(string path, int count)
        {
        }

        private UniTask<T> CreateObject(T prefab)
        {
            var newInstance = container.InstantiatePrefabForComponent<T>(prefab);

            return UniTask.FromResult(newInstance);
        }
    }
}