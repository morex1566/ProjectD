using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 룩의 직선 반복 이동 범위 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_MoveRange_Rook", menuName = "Scriptable Objects/Data/MoveRangeData/Rook")]
    public class RookMoveRangeData : MoveRangeData
    {
        private void Reset()
        {
            SetMoveRange(true, Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left);
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
