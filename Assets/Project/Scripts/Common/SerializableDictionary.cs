using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TRPG.Runtime
{
    ///<summary>
    /// Unity 인스펙터에서 편집 가능한 Dictionary 래퍼입니다.
    ///</summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        ///<summary>
        /// 인스펙터에서 실제로 편집되는 Key-Value 목록입니다.
        ///</summary>
        [SerializeField] private List<Entry> entries = new();

        ///<summary>
        /// 런타임에서 빠르게 조회하기 위한 Dictionary 캐시입니다.
        ///</summary>
        private Dictionary<TKey, TValue> dictionary = new();

        ///<summary>
        /// 인스펙터에 표시되는 Key-Value 한 쌍입니다.
        ///</summary>
        [Serializable]
        public class Entry
        {
            ///<summary>
            /// Dictionary의 Key입니다.
            ///</summary>
            public TKey Key;

            ///<summary>
            /// Dictionary의 Value입니다.
            ///</summary>
            public TValue Value;
        }

        ///<summary>
        /// Key를 기준으로 Value를 가져오거나 설정합니다.
        ///</summary>
        public TValue this[TKey key]
        {
            get
            {
                SyncDictionaryFromEntries();
                return dictionary[key];
            }
            set
            {
                SetValue(key, value);
            }
        }

        ///<summary>
        /// 저장된 Key-Value 개수입니다.
        ///</summary>
        public int Count => entries.Count;

        ///<summary>
        /// 읽기 전용 Dictionary를 반환합니다.
        ///</summary>
        public IReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary => dictionary;

        ///<summary>
        /// Unity가 직렬화하기 전에 호출합니다.
        ///</summary>
        public void OnBeforeSerialize()
        {
            // 인스펙터에서 추가한 entries를 지우면 안 되므로 여기서는 아무것도 하지 않습니다.
        }

        ///<summary>
        /// Unity가 역직렬화한 뒤 호출합니다.
        ///</summary>
        public void OnAfterDeserialize()
        {
            SyncDictionaryFromEntries();
        }

        ///<summary>
        /// Key와 Value를 추가합니다.
        ///</summary>
        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                Debug.LogWarning($"SerializableDictionary already contains key: {key}");
                return;
            }

            // 인스펙터에 보이는 entries에 먼저 추가합니다.
            entries.Add(new Entry
            {
                Key = key,
                Value = value
            });

            // 런타임 조회용 Dictionary도 갱신합니다.
            dictionary.Add(key, value);
        }

        ///<summary>
        /// Key가 이미 있으면 값을 수정하고, 없으면 새로 추가합니다.
        ///</summary>
        public void SetValue(TKey key, TValue value)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (EqualityComparer<TKey>.Default.Equals(entries[i].Key, key))
                {
                    // 이미 같은 Key가 있으면 Value만 수정합니다.
                    entries[i].Value = value;
                    SyncDictionaryFromEntries();
                    return;
                }
            }

            // 같은 Key가 없으면 새 Entry를 추가합니다.
            entries.Add(new Entry
            {
                Key = key,
                Value = value
            });

            SyncDictionaryFromEntries();
        }

        ///<summary>
        /// 기존 값을 지우고 여러 Key-Value를 한 번에 저장합니다.
        ///</summary>
        public void SetValues(IReadOnlyDictionary<TKey, TValue> values)
        {
            entries.Clear();
            dictionary.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in values)
            {
                entries.Add(new Entry
                {
                    Key = pair.Key,
                    Value = pair.Value
                });

                dictionary.Add(pair.Key, pair.Value);
            }
        }

        ///<summary>
        /// 해당 Key가 존재하는지 확인합니다.
        ///</summary>
        public bool ContainsKey(TKey key)
        {
            SyncDictionaryFromEntries();
            return dictionary.ContainsKey(key);
        }

        ///<summary>
        /// Key로 Value를 가져옵니다.
        ///</summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            SyncDictionaryFromEntries();
            return dictionary.TryGetValue(key, out value);
        }

        ///<summary>
        /// 해당 Key를 제거합니다.
        ///</summary>
        public bool Remove(TKey key)
        {
            bool removed = false;

            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (EqualityComparer<TKey>.Default.Equals(entries[i].Key, key))
                {
                    // 인스펙터에 보이는 entries에서도 제거합니다.
                    entries.RemoveAt(i);
                    removed = true;
                }
            }

            if (removed)
            {
                SyncDictionaryFromEntries();
            }

            return removed;
        }

        ///<summary>
        /// 모든 데이터를 제거합니다.
        ///</summary>
        public void Clear()
        {
            entries.Clear();
            dictionary.Clear();
        }

        ///<summary>
        /// 일반 Dictionary로 복사해서 반환합니다.
        ///</summary>
        public Dictionary<TKey, TValue> ToDictionary()
        {
            SyncDictionaryFromEntries();
            return new Dictionary<TKey, TValue>(dictionary);
        }

        ///<summary>
        /// 인스펙터 List 데이터를 Dictionary 캐시로 동기화합니다.
        ///</summary>
        private void SyncDictionaryFromEntries()
        {
            dictionary.Clear();

            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];

                if (entry == null)
                {
                    continue;
                }

                // Dictionary는 Key 중복이 불가능하므로 중복 Key는 뒤쪽 값으로 덮어씁니다.
                dictionary[entry.Key] = entry.Value;
            }
        }

        ///<summary>
        /// 저장된 모든 Key 목록을 반환합니다.
        ///</summary>
        public IEnumerable<TKey> Keys
        {
            get
            {
                SyncDictionaryFromEntries();
                return dictionary.Keys;
            }
        }

        ///<summary>
        /// 저장된 모든 Value 목록을 반환합니다.
        ///</summary>
        public IEnumerable<TValue> Values
        {
            get
            {
                SyncDictionaryFromEntries();
                return dictionary.Values;
            }
        }

        ///<summary>
        /// Key-Value 쌍을 순회할 수 있는 Enumerator를 반환합니다.
        ///</summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            SyncDictionaryFromEntries();
            return dictionary.GetEnumerator();
        }

        ///<summary>
        /// 비제네릭 Enumerator를 반환합니다.
        ///</summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
