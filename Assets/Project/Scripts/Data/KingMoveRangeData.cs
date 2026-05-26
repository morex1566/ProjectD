using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 킹의 인접 8방향 이동 범위 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_MoveRange_King", menuName = "Scriptable Objects/Data/MoveRangeData/King")]
    public class KingMoveRangeData : MoveRangeData
    {
        private void Reset()
        {
            SetMoveRange(false, Vector3Int.up, new Vector3Int(1, 1, 0), Vector3Int.right, new Vector3Int(1, -1, 0), Vector3Int.down, new Vector3Int(-1, -1, 0), Vector3Int.left, new Vector3Int(-1, 1, 0));
        }

        private void OnEnable()
        {
            if (directions == null || directions.Count == 0)
            {
                Reset();
            }
        }
    }
}
