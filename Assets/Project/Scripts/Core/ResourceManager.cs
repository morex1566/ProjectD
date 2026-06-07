using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace TRPG.Runtime
{
    /// <summary>
    /// 런타임 리소스와 게임 데이터 로딩 작업을 순차 처리합니다.
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

        // 같은 label이 로드 중이면 기존 작업을 공유해 Addressables 중복 호출을 막습니다.
        private static readonly Dictionary<string, AsyncLazy<IList<Object>>> pendingLoadTasks = new();

        private static AsyncLazy<AsyncUnit> initTask;




        /// <summary>
        /// 리소스 매니저 인스턴스와 설정 데이터, Core Addressables 리소스를 준비합니다.
        /// </summary>
        public static async UniTask Init()
        {
            initTask ??= new AsyncLazy<AsyncUnit>(InitAsync);
            await initTask;
        }

        private static async UniTask<AsyncUnit> InitAsync()
        {
            GetInstance();
            settings = Resources.Load<ResourceManagerSettingsData>("SO_ResourceManagerSettings");
            await LoadAsync(UnityConstant.Addressable.Label.Core);

            return AsyncUnit.Default;
        }

        /// <summary>
        /// 지정한 Addressables label의 리소스를 로드하고, 로드 중인 같은 label 요청은 기존 작업을 공유합니다.
        /// </summary>
        public static async UniTask<IList<Object>> LoadAsync(string label)
        {
            ResourceManager inst = GetInstance();

            if (cachedLabelPrimaryKeys.TryGetValue(label, out List<string> cachedPrimaryKeys))
            {
                return GetCachedAssets(cachedPrimaryKeys);
            }

            if (pendingLoadTasks.TryGetValue(label, out AsyncLazy<IList<Object>> pendingLoadTask))
            {
                return await pendingLoadTask;
            }

            AsyncLazy<IList<Object>> loadTask = new AsyncLazy<IList<Object>>(() => inst.LoadInternalAsync(label));
            pendingLoadTasks[label] = loadTask;

            try
            {
                return await loadTask;
            }
            finally
            {
                pendingLoadTasks.Remove(label);
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
        /// 지정한 label의 로드가 진행 중이면 완료를 기다린 뒤 캐시와 Addressables handle을 해제합니다.
        /// </summary>
        public static async UniTask UnloadAsync(string label)
        {
            if (pendingLoadTasks.TryGetValue(label, out AsyncLazy<IList<Object>> pendingLoadTask))
            {
                await pendingLoadTask;
            }

            await GetInstance().UnloadInternalAsync(label);
        }




        /// <summary>
        /// Addressables에서 location과 asset을 실제로 로드하고 캐시에 등록합니다.
        /// </summary>
        private async UniTask<IList<Object>> LoadInternalAsync(string label)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(Object));

            try
            {
                IList<IResourceLocation> locations = await locationsHandle.ToUniTask();
                List<string> primaryKeys = new();
                List<Object> assets = new();

                // 로드 중 실패해도 catch에서 같은 언로드 경로로 정리할 수 있게 먼저 등록합니다.
                cachedLabelPrimaryKeys[label] = primaryKeys;

                foreach (IResourceLocation location in locations)
                {
                    // 실제 리소스 해제는 이 handle을 기준으로 처리합니다.
                    AsyncOperationHandle<Object> assetHandle = Addressables.LoadAssetAsync<Object>(location);
                    Object asset = await assetHandle.ToUniTask();
                    if (asset == null)
                    {
                        if (assetHandle.IsValid())
                        {
                            Addressables.Release(assetHandle);
                        }

                        continue;
                    }

                    // 선로드 캐시는 Addressables location의 PrimaryKey를 기준으로 통일합니다.
                    cachedResources[location.PrimaryKey] = asset;
                    cachedHandles[location.PrimaryKey] = assetHandle;
                    primaryKeys.Add(location.PrimaryKey);
                    assets.Add(asset);
                }

                return assets;
            }
            catch
            {
                await UnloadInternalAsync(label);

                Debug.LogError($"Load Error: {label}");
                throw;
            }
            finally
            {
                if (locationsHandle.IsValid())
                {
                    Addressables.Release(locationsHandle);
                }
            }
        }

        /// <summary>
        /// label에 연결된 primary key 목록을 기준으로 캐시와 handle을 정리합니다.
        /// </summary>
        private UniTask UnloadInternalAsync(string label)
        {
            // 로드된 적 없는 label은 언로드할 리소스가 없습니다.
            if (!cachedLabelPrimaryKeys.TryGetValue(label, out List<string> primaryKeys))
            {
                return UniTask.CompletedTask;
            }

            cachedLabelPrimaryKeys.Remove(label);

            foreach (string primaryKey in primaryKeys)
            {
                // Addressables 로드 참조는 로드 때 받은 handle로 해제합니다.
                if (cachedHandles.TryGetValue(primaryKey, out AsyncOperationHandle<Object> handle) && handle.IsValid())
                {
                    Addressables.Release(handle);
                }

                cachedHandles.Remove(primaryKey);
                cachedResources.Remove(primaryKey);
            }

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// primary key 목록에 대응하는 유효한 캐시 리소스만 모아서 반환합니다.
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
        /// Addressables locator에서 runtime key와 타입에 맞는 primary key를 찾습니다.
        /// </summary>
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
