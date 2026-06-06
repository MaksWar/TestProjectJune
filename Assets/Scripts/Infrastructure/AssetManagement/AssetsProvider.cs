using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Infrastructure.AssetManagement
{
    public class AssetsProvider : IAssetsProvider
    {
        private readonly Dictionary<string, AsyncOperationHandle> assetRequests = new ();

        private readonly AssetModel _assetModel = new();

        public async UniTask InitializeAsync() => 
            await Addressables.InitializeAsync().ToUniTask();

        public async UniTask<TAsset> Load<TAsset>(string key, Type user) where TAsset : class
        {
            AsyncOperationHandle handle;

            try
            {
                if (_assetModel.ContainsAsset(key))
                {
                    AsyncOperationHandle assetHandle = _assetModel.GetAssetHandle(key);

                    if (_assetModel.IsPending(key))
                    {
                        await assetHandle.ToUniTask();
                    }

                    _assetModel.AddAssetUser(key, user);

                    return assetHandle.Result as TAsset;
                }
                
                if (!TryGetValidAssetRequest(key, out handle))
                {
                    handle = Addressables.LoadAssetAsync<TAsset>(key);

                    assetRequests[key] = handle;
                    _assetModel.AddAsset(key, handle, user);
                }

                await handle.ToUniTask();
                
                return handle.Result as TAsset;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load asset with key {key}, error : {e}");
                
                return null;
            }
        }
        
        public async UniTask<TAsset> LoadPrefab<TAsset>(string key, Type user) where TAsset : class
        {
            AsyncOperationHandle handle;

            try
            {
                if (_assetModel.ContainsAsset(key))
                {
                    AsyncOperationHandle assetHandle = _assetModel.GetAssetHandle(key);
                    if (_assetModel.IsPending(key))
                    {
                        await assetHandle.ToUniTask();
                    }
                    
                    _assetModel.AddAssetUser(key, user);

                    return (assetHandle.Result as GameObject)?.GetComponent<TAsset>();
                }
                
                if (!TryGetValidAssetRequest(key, out handle))
                {
                    handle = Addressables.LoadAssetAsync<GameObject>(key);
                    assetRequests[key] = handle;
                    _assetModel.AddAsset(key, handle, user);
                }

                await handle.ToUniTask();
                
                var prefab = handle.Result as GameObject;
                
                return prefab.GetComponent<TAsset>();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load asset with key {key}, error : {e}");
                
                return null;
            }
        }

        public async UniTask<TAsset> Load<TAsset>(AssetReference assetReference, Type user) where TAsset : class
        {
            try
            {
                return await Load<TAsset>(assetReference.AssetGUID, user);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load asset with reference {assetReference}, error : {e}");
                
                return null;
            }
        }

        public async UniTask<List<string>> GetAssetsListByLabel<TAsset>(string label, Type user)
        {
            try
            {
                return await GetAssetsListByLabel(label, user, typeof(TAsset));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get assets list by label {label}, error : {e}");
                
                return new List<string>();
            }
        }

        public async UniTask<List<string>> GetAssetsListByLabel(string label, Type user, Type type = null)
        {
            try
            {
                var operationHandle = Addressables.LoadResourceLocationsAsync(label, type);

                var locations = await operationHandle.ToUniTask();

                List<string> assetKeys = new List<string>(locations.Count);

                foreach (var location in locations)
                    assetKeys.Add(location.PrimaryKey);

                Addressables.Release(operationHandle);

                return assetKeys;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get assets list by label {label}, error : {e}");
                
                return new List<string>();
            }
        }

        public async UniTask<TAsset[]> LoadAll<TAsset>(List<string> keys, Type user) where TAsset : class
        {
            try
            {
                List<UniTask<TAsset>> tasks = new List<UniTask<TAsset>>(keys.Count);

                foreach (var key in keys)
                    tasks.Add(Load<TAsset>(key, user));

                return await UniTask.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load all assets, error : {e}");
                
                return Array.Empty<TAsset>();
            }
        }
        
        public async UniTask WarmupAssetsByLabel(string label, Type user)
        {
            try
            {
                var assetsList = await GetAssetsListByLabel(label, user);
                await LoadAll<object>(assetsList, user);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to warmup assets by label {label}, error : {e}");
            }
        }

        public async UniTask ReleaseAssetsByLabel(string label, Type user)
        {
            var assetsList = await GetAssetsListByLabel(label, user);
            
            foreach (var assetKey in assetsList)
            {
                if (assetRequests.TryGetValue(assetKey, out var handler))
                {
                    Addressables.Release(handler);
                    assetRequests.Remove(assetKey);
                }
            }
        }

        public void Cleanup()
        {
            foreach (var assetRequest in assetRequests) 
                Addressables.Release(assetRequest.Value);
            
            assetRequests.Clear();
        }

        public void ReleaseAsset(string name, Type user)
        {
            if (!_assetModel.ContainsAsset(name) || !_assetModel.ContainsAssetUser(name, user))
            {
                Debug.LogError($"AddressableAssetService: Fail to release asset {name}, user {user}");
                return;
            }

            var removeLastUser = _assetModel.RemoveAssetUser(name, user);

            if (removeLastUser)
            {
                var handle = _assetModel.GetAssetHandle(name);
                Addressables.Release(handle);
                _assetModel.RemoveAssetData(name);
                assetRequests.Remove(name);
            }
        }

        public bool TryReleaseAsset(string name, Type user)
        {
            if (_assetModel.ContainsAsset(name) == false || _assetModel.ContainsAssetUser(name, user) == false)
            {
                return false;
            }

            ReleaseAsset(name, user);

            return true;
        }

        public bool IsAssetLoaded(string key)
        {
            if (_assetModel.ContainsAsset(key) == false)
            {
                return false;
            }

            AsyncOperationHandle handle = _assetModel.GetAssetHandle(key);
            return handle.IsValid() && handle.IsDone;
        }

        public void UnloadUnusedAssets()
        {
            foreach (AsyncOperationHandle handle in _assetModel.GetAssetHandleWithoutInstances())
            {
                Addressables.Release(handle);
            }

            _assetModel.RemoveDataWithoutUser();
        }

        private bool TryGetValidAssetRequest(string key, out AsyncOperationHandle handle)
        {
            if (assetRequests.TryGetValue(key, out handle) == false)
            {
                return false;
            }

            if (handle.IsValid())
            {
                return true;
            }

            assetRequests.Remove(key);
            handle = default;

            return false;
        }
    }
}
