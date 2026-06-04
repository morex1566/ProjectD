using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TRPG.Runtime
{
    /// <summary>
    /// 게임 전역 이벤트 시스템의 진입점을 관리하는 매니저입니다.
    /// </summary>
    public class EventManager : MonoBehaviourSingleton<EventManager>
    {
        private static EventManagerSettingsData settings;

        private static readonly Dictionary<int, Event> eventInsts = new();

        /// <summary>
        /// EventManager 싱글톤 인스턴스를 보장합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
            settings = Resources.Load<EventManagerSettingsData>("SO_EventManagerSettings");

            // 시작 씬이여야만 
            if (SceneManager.GetActiveScene().name == "SCN_Title") Trigger<TitleEvent>().Forget();
        }

        /// <summary>
        /// 설정에 등록된 이벤트 프리팹을 GameObject로 생성하고 실행합니다.
        /// </summary>
        public static UniTask Trigger<T>() where T : Event
        {
            return GetInstance().PlayInternal<T>();
        }

        public static void Close(int instanceId)
        {
            Destroy(eventInsts[instanceId]);
            eventInsts.Remove(instanceId);
        }

        private async UniTask PlayInternal<T>() where T : Event
        {
            if (settings == null)
            {
                Debug.LogError($"[{nameof(EventManager)}] SO_EventManagerSettings를 찾지 못했습니다.");
                return;
            }

            GameObject eventPb = settings.Get<T>();
            if (eventPb == null)
            {
                Debug.LogError($"[{nameof(EventManager)}] {typeof(T).Name} 이벤트 프리팹을 찾지 못했습니다.");
                return;
            }

            GameObject eventObj = Instantiate(eventPb, transform);
            T eventInst = eventObj.GetComponent<T>();
            if (eventInst == null)
            {
                Debug.LogError($"[{nameof(EventManager)}] 생성된 프리팹에 {typeof(T).Name} 컴포넌트가 없습니다.", eventObj);
                Destroy(eventObj);
                return;
            }

            eventInsts.Add(eventObj.GetInstanceID(), eventInst);

            try
            {
                await eventInst.ExecuteAsync();
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                Close(eventObj.GetInstanceID());
            }
        }
    }
}
