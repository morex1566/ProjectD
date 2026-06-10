using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace TRPG.Runtime
{
    /// <summary>
    /// 런타임 리소스와 게임 데이터 로딩 작업을 처리합니다.
    /// </summary>
    public class ResourceManager : MonoBehaviourSingleton<ResourceManager>
    {
        private static ResourceManagerSettingsData settings;

        // Key : Labelname, Value : primarykey
        private static readonly Dictionary<string, List<string>> cachedLabelPrimaryKeys = new();

        // 게임 오브젝트의 AssetReference가 찾을 실제 리소스 캐시입니다. Key는 location.PrimaryKey입니다.
        private static readonly Dictionary<string, Object> cachedResources = new();

        // Addressables.Release는 로드 때 받은 handle 기준으로 처리합니다.
        private static readonly Dictionary<string, AsyncOperationHandle<Object>> cachedHandles = new();



        /// <summary>
        /// 리소스 매니저 인스턴스와 설정 데이터, Core Addressables 리소스를 동기적으로 준비합니다.
        /// CAUTION : GameManager에서 한번만 사용됨
        /// </summary>
        public static void Init()
        {
            GetInstance();
            settings = Resources.Load<ResourceManagerSettingsData>("SO_ResourceManagerSettings");
            Load(UnityConstant.Addressable.Label.Core);
        }

        /// <summary>
        /// 지정한 Addressables label의 리소스를 동기 로드합니다.
        /// </summary>
        public static IList<Object> Load(string label)
        {
            ResourceManager inst = GetInstance();

            if (cachedLabelPrimaryKeys.TryGetValue(label, out List<string> cachedPrimaryKeys))
            {
                return GetCachedAssets(cachedPrimaryKeys);
            }

            return inst.LoadInternal(label);
        }

        /// <summary>
        /// 선로드된 리소스 캐시에서 AssetReference에 대응하는 리소스를 반환합니다.
        /// </summary>
        public static T GetResource<T>(AssetReferenceT<T> reference) where T : Object
        {
            if (typeof(Component).IsAssignableFrom(typeof(T)))
            {
                Debug.LogWarning($"Use AssetReferenceT<GameObject> instead of AssetReferenceT<{typeof(T).Name}>.");
                return null;
            }

            if (reference == null || !reference.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"GetResource failed. Invalid {nameof(AssetReferenceT<T>)}.");
                return null;
            }

            if (!TryGetPrimaryKey(reference.RuntimeKey, typeof(T), out string primaryKey))
            {
                Debug.LogWarning($"GetResource failed. Location not found. RuntimeKey: {reference.RuntimeKey}");
                return null;
            }

            return cachedResources.TryGetValue(primaryKey, out Object resource) ? resource as T : null;
        }

        /// <summary>
        /// 지정한 label의 캐시와 Addressables handle을 해제합니다.
        /// </summary>
        public static void Unload(string label)
        {
            GetInstance().UnloadInternal(label);
        }

        private IList<Object> LoadInternal(string label)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locationsHandle =
                Addressables.LoadResourceLocationsAsync(label, typeof(Object));

            try
            {
                IList<IResourceLocation> locations = locationsHandle.WaitForCompletion();

                List<string> loadedPrimaryKeys = new();
                List<Object> loadedAssets = new();

                // 중간 실패 시 UnloadInternal(label)로 정리할 수 있도록 먼저 등록합니다.
                cachedLabelPrimaryKeys[label] = loadedPrimaryKeys;

                foreach (IResourceLocation location in locations)
                {
                    if (!TryLoadAndCacheAsset(location, loadedPrimaryKeys, loadedAssets))
                    {
                        continue;
                    }
                }

                return loadedAssets;
            }
            catch
            {
                UnloadInternal(label);

                Debug.LogError($"Load Error: {label}");
                throw;
            }
            finally
            {
                ReleaseIfValid(locationsHandle);
            }
        }

        private static bool TryLoadAndCacheAsset(IResourceLocation location, List<string> loadedPrimaryKeys, List<Object> loadedAssets)
        {
            AsyncOperationHandle<Object> assetHandle = Addressables.LoadAssetAsync<Object>(location);

            Object asset = assetHandle.WaitForCompletion();

            if (asset == null)
            {
                ReleaseIfValid(assetHandle);
                return false;
            }

            CacheLoadedAsset(location.PrimaryKey, asset, assetHandle);

            loadedPrimaryKeys.Add(location.PrimaryKey);
            loadedAssets.Add(asset);

            return true;
        }

        private static void CacheLoadedAsset(string primaryKey, Object asset, AsyncOperationHandle<Object> assetHandle)
        {
            cachedResources[primaryKey] = asset;
            cachedHandles[primaryKey] = assetHandle;
        }

        private static void ReleaseIfValid<T>(AsyncOperationHandle<T> handle)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        private void UnloadInternal(string label)
        {
            if (!cachedLabelPrimaryKeys.Remove(label, out List<string> primaryKeys))
            {
                return;
            }

            foreach (string primaryKey in primaryKeys)
            {
                if (cachedHandles.TryGetValue(primaryKey, out AsyncOperationHandle<Object> handle))
                {
                    ReleaseIfValid(handle);
                }

                cachedHandles.Remove(primaryKey);
                cachedResources.Remove(primaryKey);
            }
        }

        private static List<Object> GetCachedAssets(List<string> primaryKeys)
        {
            List<Object> assets = new();

            foreach (string primaryKey in primaryKeys)
            {
                if (!cachedResources.TryGetValue(primaryKey, out Object cachedAsset)) continue;
                if (cachedAsset == null) continue;

                assets.Add(cachedAsset);
            }

            return assets;
        }

        private static bool TryGetPrimaryKey(object runtimeKey, System.Type type, out string primaryKey)
        {
            foreach (IResourceLocator locator in Addressables.ResourceLocators)
            {
                if (!locator.Locate(runtimeKey, type, out IList<IResourceLocation> locations)) continue;
                if (locations.Count == 0) continue;

                primaryKey = locations[0].PrimaryKey;
                return true;
            }

            primaryKey = null;
            return false;
        }
    }
}
