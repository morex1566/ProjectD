using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 나이트의 L자 이동 범위 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_MoveRange_Knight", menuName = "Scriptable Objects/Data/MoveRangeData/Knight")]
    public class KnightMoveRangeData : MoveRangeData
    {
        private void Reset()
        {
            SetMoveRange(false, new Vector3Int(1, 2, 0), new Vector3Int(2, 1, 0), new Vector3Int(2, -1, 0), new Vector3Int(1, -2, 0), new Vector3Int(-1, -2, 0), new Vector3Int(-2, -1, 0), new Vector3Int(-2, 1, 0), new Vector3Int(-1, 2, 0));
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
