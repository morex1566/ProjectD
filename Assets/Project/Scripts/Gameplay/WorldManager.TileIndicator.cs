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
            return tileIndicators.TryGetValue(cellPos, out TileIndicatorController tileIndicator) &&
                   tileIndicator != null &&
                   tileIndicator.Owner == owner;
        }

        /// <summary>
        /// 타일에 표식 넣기
        /// </summary>
        private void AddAllyTileIndicatorInternal(List<Vector3Int> cellPosList, CreatureController owner)
        {
            RemoveTileIndicatorsInternal(owner);
            AddTileIndicatorsInternal(cellPosList, settings.AllyTileIndicatorPb, owner);
        }

        /// <summary>
        /// 적 대상 범위 CellPos마다 타일 인디케이터를 생성합니다.
        /// </summary>
        private void AddEnemyTileIndicatorInternal(List<Vector3Int> cellPosList, CreatureController owner)
        {
            RemoveTileIndicatorsInternal(owner);
            AddTileIndicatorsInternal(cellPosList, settings.EnemyTileIndicatorPb, owner);
        }

        /// <summary>
        /// 이동 가능 CellPos와 공격 가능 CellPos를 함께 표시합니다.
        /// </summary>
        private void AddTileIndicatorsInternal(List<Vector3Int> allyCellPosList, List<Vector3Int> enemyCellPosList, CreatureController owner)
        {
            RemoveTileIndicatorsInternal(owner);

            AddTileIndicatorsInternal(allyCellPosList, settings.AllyTileIndicatorPb, owner);
            AddTileIndicatorsInternal(enemyCellPosList, settings.EnemyTileIndicatorPb, owner);
        }

        /// <summary>
        /// 지정 프리팹으로 CellPos마다 인디케이터를 생성합니다.
        /// </summary>
        private void AddTileIndicatorsInternal(List<Vector3Int> cellPosList, TileIndicatorController indicatorPb, CreatureController owner)
        {
            if (cellPosList == null || indicatorPb == null) return;

            foreach (Vector3Int cellPos in cellPosList)
            {
                if (!TryGetMapWorldPosInternal(cellPos, out Vector3 indicatorWorldPos)) continue;

                RemoveTileIndicator(cellPos);

                TileIndicatorController tileIndicator = Instantiate(indicatorPb, indicatorWorldPos, Quaternion.identity, transform);
                tileIndicator.Init(owner, cellPos);
                tileIndicators.Add(cellPos, tileIndicator);
            }
        }

        /// <summary>
        /// owner의 인디케이터 중 지정 CellPos만 hover 상태로 전환합니다.
        /// </summary>
        private void SetTileIndicatorHoverInternal(CreatureController owner, Vector3Int hoverCellPos)
        {
            foreach (KeyValuePair<Vector3Int, TileIndicatorController> pair in tileIndicators)
            {
                if (pair.Value == null || pair.Value.Owner != owner) continue;

                if (pair.Key == hoverCellPos)
                {
                    pair.Value.PlayMoveTo();
                }
                else
                {
                    pair.Value.PlayMovable();
                }
            }
        }

        /// <summary>
        /// owner의 인디케이터 hover 상태를 모두 해제합니다.
        /// </summary>
        private void ClearTileIndicatorHoverInternal(CreatureController owner)
        {
            foreach (KeyValuePair<Vector3Int, TileIndicatorController> pair in tileIndicators)
            {
                if (pair.Value == null || pair.Value.Owner != owner) continue;

                pair.Value.PlayMovable();
            }
        }

        /// <summary>
        /// 인디케이터 삭제
        /// </summary>
        private void RemoveTileIndicatorsInternal(CreatureController owner)
        {
            List<Vector3Int> removeCellPosList = new();
            foreach (KeyValuePair<Vector3Int, TileIndicatorController> pair in tileIndicators)
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
            if (!tileIndicators.TryGetValue(cellPos, out TileIndicatorController tileIndicator)) return;

            // 조회 항목을 먼저 제거해야 다음 indicator 생성 시 key가 충돌하지 않습니다.
            tileIndicators.Remove(cellPos);

            if (tileIndicator == null) return;

            tileIndicator.PlayDespawn(() =>
            {
                if (tileIndicator != null)
                {
                    Destroy(tileIndicator.gameObject);
                }
            });
        }

        /// <summary>
        /// 현재 월드에 등록된 모든 타일 인디케이터를 제거합니다.
        /// </summary>
        private void RemoveAllTileIndicators()
        {
            foreach (KeyValuePair<Vector3Int, TileIndicatorController> pair in tileIndicators)
            {
                if (pair.Value == null) continue;

                TileIndicatorController tileIndicator = pair.Value;
                tileIndicator.PlayDespawn(() =>
                {
                    if (tileIndicator != null)
                    {
                        Destroy(tileIndicator.gameObject);
                    }
                });
            }

            tileIndicators.Clear();
        }
    }
}
