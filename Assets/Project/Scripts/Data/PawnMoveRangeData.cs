using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 폰의 전방 1칸 이동 범위 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_MoveRange_Pawn", menuName = "Scriptable Objects/Data/MoveRangeData/Pawn")]
    public class PawnMoveRangeData : MoveRangeData
    {
        private void Reset()
        {
            SetMoveRange(false, Vector3Int.up);
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
