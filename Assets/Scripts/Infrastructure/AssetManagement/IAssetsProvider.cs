using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Infrastructure.AssetManagement
{
    public interface IAssetsProvider
    {
        UniTask InitializeAsync();
        UniTask<TAsset> Load<TAsset>(AssetReference assetReference, Type user) where TAsset : class;
        UniTask<TAsset> Load<TAsset>(string key, Type user) where TAsset : class;
        UniTask<TAsset> LoadPrefab<TAsset>(string key, Type user) where TAsset : class;
        UniTask<List<string>> GetAssetsListByLabel<TAsset>(string label, Type user);
        UniTask<List<string>> GetAssetsListByLabel(string label, Type user, Type type = null);
        UniTask<TAsset[]> LoadAll<TAsset>(List<string> keys, Type user) where TAsset : class;
        void Cleanup();
        void ReleaseAsset(string name, Type user);
        bool TryReleaseAsset(string name, Type user); 
        bool IsAssetLoaded(string key);
        void UnloadUnusedAssets();
    }
}
