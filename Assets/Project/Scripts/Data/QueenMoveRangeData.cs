using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 퀸의 직선 및 대각선 반복 이동 범위 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_MoveRange_Queen", menuName = "Scriptable Objects/Data/MoveRangeData/Queen")]
    public class QueenMoveRangeData : MoveRangeData
    {
        private void Reset()
        {
            SetMoveRange(true, Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left, new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, -1, 0), new Vector3Int(-1, 1, 0));
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
