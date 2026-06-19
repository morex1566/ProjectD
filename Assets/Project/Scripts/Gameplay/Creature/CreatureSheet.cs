using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 엑셀 CreatureSheet에서 생성되는 CreatureData 목록과 조회 캐시를 보관합니다.
    /// </summary>
    [ExcelAsset(AssetPath = "Project/Datas/Gen", ExcelName = "CreatureSheet")]
    public class CreatureSheet : ScriptableObject
    {
        /// <summary>
        /// 원본
        /// </summary>
        public List<CreatureData> Entities;

        private Dictionary<string, CreatureData> entityMap;

        public IReadOnlyDictionary<string, CreatureData> EntityMap => entityMap;

        /// <summary>
        /// DataId에 해당하는 CreatureData를 반환합니다.
        /// </summary>
        public CreatureData GetCreatureData(string dataId)
        {
            if (entityMap == null)
            {
                Init();
            }

            return entityMap[dataId];
        }

        /// <summary>
        /// 엑셀에서 로드된 CreatureData 목록을 DataId 조회용 Dictionary로 재구성합니다.
        /// </summary>
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

        /// <summary>
        /// ScriptableObject가 로드될 때 조회 캐시를 갱신합니다.
        /// </summary>
        private void OnEnable()
        {
            Init();
        }
    }
}
