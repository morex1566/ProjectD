using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 비숍의 대각선 반복 이동 범위 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_MoveRange_Bishop", menuName = "Scriptable Objects/Data/MoveRangeData/Bishop")]
    public class BishopMoveRangeData : MoveRangeData
    {
        private void Reset()
        {
            SetMoveRange(true, new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, -1, 0), new Vector3Int(-1, 1, 0));
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
