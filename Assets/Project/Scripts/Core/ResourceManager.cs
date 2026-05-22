using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public class ResourceManager : MonoBehaviourSingleton<ResourceManager>
    {
        private static ResourceManagerSettingsData settings;

        public static GameDatabase Database = new GameDatabase();

        private static readonly Queue<LoadQueueItem> loadQueue = new();

        private static bool isProcessingLoadQueue;


        public static int LoadQueueCount => loadQueue.Count;

        public static bool IsLoading => isProcessingLoadQueue;

        /// <summary>
        /// 리소스 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
            {
                settings = Resources.Load<ResourceManagerSettingsData>("SO_ResourceManagerSettings");
                EnqueueLoad("GameDatabase", Database.Load);
            }
        }

        /// <summary>
        /// 리소스 로드 작업을 공통 큐에 추가하고, 순서대로 동기 처리합니다.
        /// </summary>
        public static void EnqueueLoad(string name, Action load)
        {
            if (load == null)
            {
                Debug.LogWarning($"Load task is null. Name: {name}");
                return;
            }

            loadQueue.Enqueue(new LoadQueueItem(name, load));

            if (isProcessingLoadQueue) return;

            // 작업 시작
            ProcessLoadQueue();
        }

        private static void ProcessLoadQueue()
        {
            isProcessingLoadQueue = true;

            try
            {
                while (loadQueue.Count > 0)
                {
                    LoadQueueItem loadQueueItem = loadQueue.Peek();

                    try
                    {
                        loadQueueItem.Load();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError($"Resource load failed. Name: {loadQueueItem.Name}\n{exception}");
                    }

                    // 현재 작업이 끝난 뒤 큐에서 제거합니다.
                    loadQueue.Dequeue();
                }
            }
            finally
            {
                isProcessingLoadQueue = false;

                if (loadQueue.Count > 0)
                {
                    ProcessLoadQueue();
                }
            }
        }

        private readonly struct LoadQueueItem
        {
            public readonly string Name;

            public readonly Action Load;

            public LoadQueueItem(string name, Action load)
            {
                Name = name;
                Load = load;
            }
        }
    }
}
