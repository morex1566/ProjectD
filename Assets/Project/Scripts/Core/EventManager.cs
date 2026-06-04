using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 게임 전역 이벤트 시스템의 진입점을 관리하는 매니저입니다.
    /// </summary>
    public class EventManager : MonoBehaviourSingleton<EventManager>
    {
        private static EventManagerSettingsData settings;

        private readonly List<Event> activeEvents = new();

        /// <summary>
        /// EventManager 싱글톤 인스턴스를 보장합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
            settings = Resources.Load<EventManagerSettingsData>("SO_EventManagerSettings");
        }

        /// <summary>
        /// 설정에 등록된 이벤트 프리팹을 GameObject로 생성하고 실행합니다.
        /// </summary>
        public static UniTask Play<T>() where T : Event
        {
            return GetInstance().PlayInternal<T>();
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

            activeEvents.Add(eventInst);

            try
            {
                await eventInst.ExecuteAsync();
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
            finally
            {
                activeEvents.Remove(eventInst);

                if (eventObj != null)
                {
                    Destroy(eventObj);
                }
            }
        }
    }
}
