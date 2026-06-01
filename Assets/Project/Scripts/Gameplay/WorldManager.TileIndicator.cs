using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        /// <summary>
        /// 내가 소유한 타일 인디케이터인지?
        /// </summary>
        private bool HasIndicatorInCellPosInternal(Vector3Int cellPos, CreatureController owner)
        {
            return tileIndicators.ContainsKey(cellPos) && tileIndicators[cellPos].Owner == owner;
        }

        /// <summary>
        /// 타일에 표식 넣기
        /// </summary>
        private void AddAllyTileIndicatorInternal(List<Vector3Int> cellPosList, CreatureController owner)
        {
            RemoveTileIndicatorsInternal(owner);

            foreach (Vector3Int cellPos in cellPosList)
            {
                if (!TryGetMapWorldPosInternal(cellPos, out Vector3 indicatorWorldPos)) continue;

                RemoveTileIndicator(cellPos);

                TileIndicator allyTileIndicatorPb = settings.AllyTileIndicatorPb;
                TileIndicator tileIndicator = Instantiate(allyTileIndicatorPb, indicatorWorldPos, Quaternion.identity, transform);
                tileIndicator.Init(owner, cellPos);
                tileIndicators.Add(cellPos, tileIndicator);
            }
        }

        /// <summary>
        /// 적 대상 범위 CellPos마다 타일 인디케이터를 생성합니다.
        /// </summary>
        private void AddEnemyTileIndicatorInternal(List<Vector3Int> cellPosList, CreatureController owner)
        {
            RemoveTileIndicatorsInternal(owner);

            foreach (Vector3Int cellPos in cellPosList)
            {
                if (!TryGetMapWorldPosInternal(cellPos, out Vector3 indicatorWorldPos)) continue;

                RemoveTileIndicator(cellPos);

                TileIndicator enemyTileIndicatorPb = settings.EnemyTileIndicatorPb;
                TileIndicator tileIndicator = Instantiate(enemyTileIndicatorPb, indicatorWorldPos, Quaternion.identity, transform);
                tileIndicator.Init(owner, cellPos);
                tileIndicators.Add(cellPos, tileIndicator);
            }
        }

        /// <summary>
        /// 인디케이터 삭제
        /// </summary>
        private void RemoveTileIndicatorsInternal(CreatureController owner)
        {
            List<Vector3Int> removeCellPosList = new();
            foreach (KeyValuePair<Vector3Int, TileIndicator> pair in tileIndicators)
            {
                if (pair.Value == null)
                {
                    removeCellPosList.Add(pair.Key);
                    continue;
                }

                if (!(pair.Value.Owner == owner)) continue;

                removeCellPosList.Add(pair.Key);
            }

            foreach (Vector3Int cellPos in removeCellPosList)
            {
                RemoveTileIndicator(cellPos);
            }
        }

        /// <summary>
        /// 지정 CellPos의 인디케이터 오브젝트와 조회 항목을 함께 제거합니다.
        /// </summary>
        private void RemoveTileIndicator(Vector3Int cellPos)
        {
            if (!tileIndicators.TryGetValue(cellPos, out TileIndicator tileIndicator)) return;

            // dictionary entry와 Scene object를 함께 제거해야 다음 indicator 생성 시 key가 충돌하지 않습니다.
            if (tileIndicator != null) Destroy(tileIndicator.gameObject);

            tileIndicators.Remove(cellPos);
        }

        /// <summary>
        /// 현재 월드에 등록된 모든 타일 인디케이터를 제거합니다.
        /// </summary>
        private void RemoveAllTileIndicators()
        {
            foreach (KeyValuePair<Vector3Int, TileIndicator> pair in tileIndicators)
            {
                if (pair.Value == null) continue;

                Destroy(pair.Value.gameObject);
            }

            tileIndicators.Clear();
        }
    }
}
