using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// WeaponSheet 엑셀에서 생성되는 WeaponData 목록과 조회 캐시를 보관합니다.
    /// </summary>
    [ExcelAsset(AssetPath = "Project/Datas/Gen", ExcelName = "WeaponSheet")]
    public class WeaponSheet : ScriptableObject
    {
        /// <summary>
        /// 엑셀에서 생성된 원본 WeaponData 목록입니다.
        /// </summary>
        public List<WeaponData> Entities = new();

        private readonly Dictionary<string, WeaponData> entityMap = new();

        /// <summary>
        /// Id 기준 WeaponData 조회 캐시입니다.
        /// </summary>
        public IReadOnlyDictionary<string, WeaponData> EntityMap => entityMap;

        /// <summary>
        /// ScriptableObject가 로드될 때 조회 캐시를 구성합니다.
        /// </summary>
        private void OnEnable()
        {
            BuildEntityMap();
        }

        /// <summary>
        /// ExcelImporter가 데이터 반영을 마친 뒤 조회 캐시를 갱신합니다.
        /// </summary>
        private void OnCreate()
        {
            BuildEntityMap();
        }

        /// <summary>
        /// 현재 Entities 목록을 기준으로 Id 조회 캐시를 다시 생성합니다.
        /// </summary>
        private void BuildEntityMap()
        {
            entityMap.Clear();

            if (Entities == null)
            {
                return;
            }

            foreach (WeaponData entity in Entities)
            {
                if (entity == null || string.IsNullOrEmpty(entity.Id) == true)
                {
                    continue;
                }

                entityMap[entity.Id] = entity;
            }
        }

        /// <summary>
        /// 지정한 Id의 WeaponData를 반환합니다.
        /// </summary>
        public bool TryGetEntity(string id, out WeaponData entity)
        {
            return entityMap.TryGetValue(id, out entity);
        }
    }
}
