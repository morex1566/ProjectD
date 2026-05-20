using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TRPG.Runtime
{
    public class GameDatabase
    {
        private const string CreatureDataLabel = "CreatureData";

        private readonly Dictionary<string, MonsterData> monsterDatas = new();

        private AsyncOperationHandle<IList<CreatureData>> creatureDataHandle;

        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Addressables 라벨로 등록된 크리처 데이터를 동기로 로드하고 몬스터 데이터 조회 테이블을 구성합니다.
        /// </summary>
        public void Load()
        {
            if (IsLoaded) return;

            monsterDatas.Clear();
            creatureDataHandle = Addressables.LoadAssetsAsync<CreatureData>(CreatureDataLabel, null);
            IList<CreatureData> creatureDatas = creatureDataHandle.WaitForCompletion();

            if (creatureDataHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"GameDatabase load failed. Label: {CreatureDataLabel}");
                return;
            }

            foreach (CreatureData creatureData in creatureDatas)
            {
                OnCreatureDataLoaded(creatureData);
            }

            IsLoaded = true;
        }

        public bool TryGetMonsterData(string id, out MonsterData monsterData)
        {
            return monsterDatas.TryGetValue(id, out monsterData);
        }

        public MonsterData GetMonsterData(string id)
        {
            if (monsterDatas.TryGetValue(id, out MonsterData monsterData)) return monsterData;

            Debug.LogWarning($"MonsterData not found. Id: {id}");
            return null;
        }

        private void OnCreatureDataLoaded(CreatureData creatureData)
        {
            if (creatureData is not MonsterData monsterData) return;
            if (string.IsNullOrWhiteSpace(monsterData.Id))
            {
                Debug.LogWarning($"MonsterData has empty id. Asset: {monsterData.name}");
                return;
            }

            if (monsterDatas.ContainsKey(monsterData.Id))
            {
                Debug.LogWarning($"Duplicate MonsterData id ignored. Id: {monsterData.Id}, Asset: {monsterData.name}");
                return;
            }

            monsterDatas.Add(monsterData.Id, monsterData);
        }
    }
}
