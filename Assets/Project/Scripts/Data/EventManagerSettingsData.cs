using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// EventManager가 생성할 이벤트 GameObject 프리팹을 보관합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_EventManagerSettings", menuName = "Scriptable Objects/Settings/EventManager")]
    public class EventManagerSettingsData : ScriptableObject
    {
        [Header(nameof(EventManagerSettingsData) + ".Setup")]

        [SerializeField] private GameObject[] eventPfs;

        private readonly Dictionary<Type, GameObject> eventPbs = new();

        private void OnEnable()
        {
            CacheEventPrefabs();
        }

        /// <summary>
        /// 요청한 이벤트 타입을 가진 GameObject 프리팹을 반환합니다.
        /// </summary>
        public GameObject Get<T>() where T : Event
        {
            Type eventType = typeof(T);

            if (!eventPbs.TryGetValue(eventType, out GameObject prefab))
            {
                Debug.LogError($"[{nameof(EventManagerSettingsData)}] {eventType.Name} 타입에 맞는 이벤트 프리팹을 찾지 못했습니다.", this);
            }

            return prefab;
        }

        /// <summary>
        /// Settings에 등록된 GameObject 프리팹을 Event 컴포넌트 타입별 매핑으로 변환합니다.
        /// </summary>
        private void CacheEventPrefabs()
        {
            eventPbs.Clear();

            if (eventPfs == null) return;

            foreach (GameObject eventPf in eventPfs)
            {
                if (eventPf == null) continue;

                Event eventComponent = eventPf.GetComponent<Event>();
                if (eventComponent == null)
                {
                    Debug.LogWarning($"[{nameof(EventManagerSettingsData)}] Event 컴포넌트가 없는 프리팹입니다. Prefab: {eventPf.name}", this);
                    continue;
                }

                Type eventType = eventComponent.GetType();
                if (eventPbs.ContainsKey(eventType))
                {
                    Debug.LogWarning($"[{nameof(EventManagerSettingsData)}] 이미 등록된 이벤트 타입입니다. Type: {eventType.Name}", this);
                    continue;
                }

                eventPbs.Add(eventType, eventPf);
            }
        }
    }
}
