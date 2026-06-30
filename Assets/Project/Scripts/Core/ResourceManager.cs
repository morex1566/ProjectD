using System.Collections.Generic;
using Type = System.Type;
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
        // Key : Labelname, Value : primarykey
        private static readonly Dictionary<string, List<string>> cachedLabelPrimaryKeys = new();

        // 게임 오브젝트의 AssetReference가 찾을 실제 리소스 캐시입니다. Key는 location.PrimaryKey입니다.
        private static readonly Dictionary<string, Object> cachedResources = new();

        // Addressables.Release는 로드 때 받은 handle 기준으로 처리합니다.
        private static readonly Dictionary<string, AsyncOperationHandle<Object>> cachedHandles = new();




        /// <summary>
        /// 기존 초기화 호출부와의 호환성을 유지합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
        }

        /// <summary>
        /// 지정한 Addressables label의 리소스를 동기 로드합니다.
        /// </summary>
        public static IList<Object> Load(string label)
        {
            if (cachedLabelPrimaryKeys.TryGetValue(label, out List<string> cachedPrimaryKeys))
            {
                return GetCachedAssets(cachedPrimaryKeys);
            }

            // 리소스 경로 로드하기
            AsyncOperationHandle<IList<IResourceLocation>> locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(Object));
            List<string> loadedPrimaryKeys = new();
            List<Object> loadedAssets = new();
            cachedLabelPrimaryKeys[label] = loadedPrimaryKeys;

            try
            {
                // 리소스 경로에서 리소스를 로드하기
                IList<IResourceLocation> locations = locationsHandle.WaitForCompletion();
                foreach (IResourceLocation location in locations)
                {
                    AsyncOperationHandle<Object> assetHandle = Addressables.LoadAssetAsync<Object>(location);
                    Object asset = assetHandle.WaitForCompletion();

                    // 로드 실패
                    if (asset == null)
                    {
                        ReleaseResource(assetHandle);
                        continue;
                    }

                    // 로드 성공
                    cachedResources[location.PrimaryKey] = asset;
                    cachedHandles[location.PrimaryKey] = assetHandle;
                    loadedPrimaryKeys.Add(location.PrimaryKey);
                    loadedAssets.Add(asset);
                }

                return loadedAssets;
            }
            catch
            {
                Debug.LogError($"Load Error: {label}");

                // 일부만 로드된 상태에서 실패해도 이미 등록한 handle은 모두 해제합니다.
                Unload(label);
                throw;
            }
            finally
            {
                // 사용한 리소스 경로 삭제하기
                ReleaseResource(locationsHandle);
            }
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
            if (!cachedLabelPrimaryKeys.Remove(label, out List<string> primaryKeys))
            {
                return;
            }

            foreach (string primaryKey in primaryKeys)
            {
                if (cachedHandles.TryGetValue(primaryKey, out AsyncOperationHandle<Object> handle))
                {
                    ReleaseResource(handle);
                }

                cachedHandles.Remove(primaryKey);
                cachedResources.Remove(primaryKey);
            }
        }




        /// <summary>
        /// primaryKey 목록을 현재 캐시된 Unity Object 목록으로 변환합니다.
        /// </summary>
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

        /// <summary>
        /// AssetReference runtimeKey와 타입에 맞는 Addressables primaryKey를 찾습니다.
        /// </summary>
        private static bool TryGetPrimaryKey(object runtimeKey, Type type, out string primaryKey)
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

        /// <summary>
        /// 유효한 Addressables handle만 Release합니다.
        /// </summary>
        private static void ReleaseResource<T>(AsyncOperationHandle<T> handle)
        {
            if (handle.IsValid() == false)
            {
                return;
            }

            Addressables.Release(handle);
        }
    }
}
