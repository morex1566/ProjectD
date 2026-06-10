using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [ExcelAsset(AssetPath = "Project/Datas/Gen", ExcelName = "CreatureSheet")]
    public class CreatureDataSheet : ScriptableObject
    {
        /// <summary>
        /// 원본
        /// </summary>
        public List<CreatureData> Entities;

        private Dictionary<string, CreatureData> entityMap;

        public IReadOnlyDictionary<string, CreatureData> EntityMap => entityMap;

        public CreatureData GetCreatureData(string dataId)
        {
            if (entityMap == null)
            {
                Init();
            }

            return entityMap[dataId];
        }

        private void Init()
        {
            entityMap = new();

            if (Entities == null)
            {
                Debug.LogWarning($"{name}: Entities is null.");
                return;
            }

            foreach (CreatureData entity in Entities)
            {
                // 엑셀 비었음?
                if (entity == null)
                {
                    Debug.LogWarning($"{name}: Entities contains null.");
                    continue;
                }

                // 아이디가 비었는지?
                if (string.IsNullOrWhiteSpace(entity.DataId))
                {
                    Debug.LogWarning($"{name}: CreatureData has empty DataId. Entity name: {entity.NameKey}");
                    continue;
                }

                // 이미 등록됨?
                if (entityMap.ContainsKey(entity.DataId))
                {
                    Debug.LogError($"{name}: Duplicate CreatureData DataId detected: {entity.DataId}");
                    continue;
                }

                entityMap.Add(entity.DataId, entity);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Init();
        }
#endif

        private void OnEnable()
        {
            Init();
        }
    }
}
