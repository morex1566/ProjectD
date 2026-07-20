using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    ///<summary>
    /// Unity 인스펙터에서 직렬화할 수 있는 고성능 Dictionary 래퍼입니다.
    ///</summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        ///<summary>
        /// Inspector에서 Entry 목록을 표시할지 여부입니다.
        ///</summary>
        [SerializeField] private bool showEntriesInInspector = true;

        ///<summary>
        /// Unity가 직렬화하는 Key-Value 목록입니다.
        ///</summary>
        [SerializeField] private List<Entry> entries = new();

        ///<summary>
        /// 런타임 조회용 Dictionary 캐시입니다.
        ///</summary>
        private Dictionary<TKey, TValue> dictionary = new();

        ///<summary>
        /// Key가 entries의 몇 번째 인덱스에 있는지 저장하는 캐시입니다.
        ///</summary>
        private Dictionary<TKey, int> indexByKey = new();

        ///<summary>
        /// 현재 저장된 Key-Value 개수입니다.
        ///</summary>
        public int Count => dictionary.Count;

        ///<summary>
        /// Inspector에서 Entry 목록을 표시할지 여부입니다.
        ///</summary>
        public bool ShowEntriesInInspector
        {
            get => showEntriesInInspector;
            set => showEntriesInInspector = value;
        }

        ///<summary>
        /// Dictionary의 모든 Key 목록입니다.
        ///</summary>
        public IReadOnlyCollection<TKey> Keys => dictionary.Keys;

        ///<summary>
        /// Dictionary의 모든 Value 목록입니다.
        ///</summary>
        public IReadOnlyCollection<TValue> Values => dictionary.Values;

        ///<summary>
        /// 읽기 전용 Dictionary 인터페이스입니다.
        ///</summary>
        public IReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary => dictionary;

        ///<summary>
        /// Key를 기준으로 Value를 가져오거나 설정합니다.
        ///</summary>
        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => Set(key, value);
        }

        ///<summary>
        /// Unity 인스펙터에 표시되는 Key-Value 한 쌍입니다.
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
        /// Unity가 직렬화하기 전에 호출합니다.
        ///</summary>
        public void OnBeforeSerialize()
        {
            // entries는 AddTarget/Set/Remove 때마다 갱신되므로 여기서 전체 동기화를 하지 않습니다.
        }

        ///<summary>
        /// Unity가 역직렬화한 뒤 호출합니다.
        ///</summary>
        public void OnAfterDeserialize()
        {
            RebuildCacheFromEntries();
        }

        ///<summary>
        /// Key가 존재하는지 확인합니다.
        ///</summary>
        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        ///<summary>
        /// Key를 기준으로 Value를 가져옵니다.
        ///</summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        ///<summary>
        /// Key-Value를 추가합니다.
        ///</summary>
        public void Add(TKey key, TValue value)
        {
            if (ReferenceEquals(key, null) == true)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (dictionary.ContainsKey(key) == true)
            {
                throw new ArgumentException($"이미 존재하는 Key입니다. Key: {key}");
            }

            // Dictionary에 먼저 추가합니다.
            dictionary.Add(key, value);

            // entries의 마지막 위치에 추가합니다.
            int entryIndex = entries.Count;
            entries.Add(new Entry
            {
                Key = key,
                Value = value
            });

            // Key가 entries의 몇 번째에 있는지 캐싱합니다.
            indexByKey.Add(key, entryIndex);
        }

        ///<summary>
        /// Key가 없으면 추가하고, 있으면 값을 덮어씁니다.
        ///</summary>
        public void Set(TKey key, TValue value)
        {
            if (ReferenceEquals(key, null) == true)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // 기존 Key라면 Dictionary와 entries의 값만 갱신합니다.
            if (indexByKey.TryGetValue(key, out int entryIndex) == true)
            {
                dictionary[key] = value;
                entries[entryIndex].Value = value;
                return;
            }

            // 신규 Key라면 새 Entry를 추가합니다.
            dictionary.Add(key, value);

            int newEntryIndex = entries.Count;
            entries.Add(new Entry
            {
                Key = key,
                Value = value
            });

            indexByKey.Add(key, newEntryIndex);
        }

        ///<summary>
        /// Key에 해당하는 값을 제거합니다.
        ///</summary>
        public bool Remove(TKey key)
        {
            if (dictionary.ContainsKey(key) == false)
            {
                return false;
            }

            int removeIndex = indexByKey[key];
            int lastIndex = entries.Count - 1;

            // 제거할 Entry가 마지막이 아니면 마지막 Entry를 제거 위치로 옮깁니다.
            if (removeIndex != lastIndex)
            {
                Entry lastEntry = entries[lastIndex];
                entries[removeIndex] = lastEntry;
                indexByKey[lastEntry.Key] = removeIndex;
            }

            // 마지막 Entry를 제거합니다.
            entries.RemoveAt(lastIndex);

            // 캐시에서도 제거합니다.
            dictionary.Remove(key);
            indexByKey.Remove(key);

            return true;
        }

        ///<summary>
        /// 모든 데이터를 제거합니다.
        ///</summary>
        public void Clear()
        {
            dictionary.Clear();
            indexByKey.Clear();
            entries.Clear();
        }

        ///<summary>
        /// Dictionary 순회자를 반환합니다.
        ///</summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        ///<summary>
        /// Dictionary 순회자를 반환합니다.
        ///</summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        ///<summary>
        /// entries를 기준으로 런타임 캐시를 다시 생성합니다.
        ///</summary>
        private void RebuildCacheFromEntries()
        {
            dictionary.Clear();
            indexByKey.Clear();

            int writeIndex = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];

                // null Key는 Dictionary에 넣을 수 없으므로 무시합니다.
                if (ReferenceEquals(entry.Key, null) == true)
                {
                    continue;
                }

                // 중복 Key가 있으면 뒤쪽 값을 최종 값으로 사용합니다.
                if (indexByKey.TryGetValue(entry.Key, out int existingIndex) == true)
                {
                    entries[existingIndex].Value = entry.Value;
                    dictionary[entry.Key] = entry.Value;
                    continue;
                }

                // 유효한 Entry를 앞쪽으로 압축합니다.
                entries[writeIndex] = entry;
                dictionary.Add(entry.Key, entry.Value);
                indexByKey.Add(entry.Key, writeIndex);
                writeIndex++;
            }

            // null Key나 중복 Key 때문에 남은 뒤쪽 Entry를 제거합니다.
            if (writeIndex < entries.Count)
            {
                entries.RemoveRange(writeIndex, entries.Count - writeIndex);
            }
        }
    }
}
