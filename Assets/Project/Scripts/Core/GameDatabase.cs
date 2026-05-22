using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TRPG.Runtime
{
    public class GameDatabase
    {
        private const string CreatureDataLabel = "CreatureData";

        private Dictionary<string, CreatureData> creatureDatas = new();

        private AsyncOperationHandle<IList<CreatureData>> creatureDataHandle;

        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Addressables 라벨로 등록된 크리처 데이터를 동기로 로드하고 몬스터 데이터 조회 테이블을 구성합니다.
        /// </summary>
        public void Load()
        {
            if (IsLoaded) return;

            creatureDatas.Clear();
            creatureDataHandle = Addressables.LoadAssetsAsync<CreatureData>(CreatureDataLabel, null);
            IList<CreatureData> creatureDataList = creatureDataHandle.WaitForCompletion();

            if (creatureDataHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"GameDatabase load failed. Label: {CreatureDataLabel}");
                return;
            }

            foreach (CreatureData creatureData in creatureDataList)
            {
                OnCreatureDataLoaded(creatureData);
            }

            IsLoaded = true;
        }

        public bool TryGetCreatureData(string id, out CreatureData creatureData)
        {
            return creatureDatas.TryGetValue(id, out creatureData);
        }

        public CreatureData GetCreatureData(string id)
        {
            if (creatureDatas.TryGetValue(id, out CreatureData creatureData)) return creatureData;

            Debug.LogWarning($"CreatureData not found. Id: {id}");
            return null;
        }

        public CreatureData GetMonsterData(string id)
        {
            return GetCreatureData(id);
        }

        private void OnCreatureDataLoaded(CreatureData creatureData)
        {
            if (creatureData == null) return;
            if (string.IsNullOrWhiteSpace(creatureData.Id))
            {
                Debug.LogWarning($"CreatureData has empty id. Asset: {creatureData.name}");
                return;
            }

            if (creatureDatas.ContainsKey(creatureData.Id))
            {
                Debug.LogWarning($"Duplicate CreatureData id ignored. Id: {creatureData.Id}, Asset: {creatureData.name}");
                return;
            }

            creatureDatas.Add(creatureData.Id, creatureData);
        }
    }
}
