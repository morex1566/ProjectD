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

        private readonly Dictionary<string, CreatureData> entityMap = new Dictionary<string, CreatureData>();

        public IReadOnlyDictionary<string, CreatureData> EntityMap => entityMap;

        public void OnEnable()
        {
            entityMap.Clear();

            foreach (CreatureData entity in Entities)
            {
                if (entity == null || string.IsNullOrEmpty(entity.DataId)) continue;

                // DataId가 중복되면 엑셀에서 뒤에 있는 데이터를 최종 값으로 사용합니다.
                entityMap[entity.DataId] = entity;
            }
        }

        /// <summary>
        /// DataId로 CreatureData를 조회합니다.
        /// </summary>
        public bool TryGetEntity(string dataId, out CreatureData entity)
        {
            return entityMap.TryGetValue(dataId, out entity);
        }
    }
}
