using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Unity 벡터 타입 간 변환을 제공하는 확장 메서드 모음입니다.
    /// </summary>
    public static class VectorEx
    {
        /// <summary>
        /// Vector2Int 좌표에 z 값을 더해 Vector3Int 좌표로 변환합니다.
        /// </summary>
        public static Vector3Int ToVector3Int(this Vector2Int vector, int z = 0)
        {
            return new Vector3Int(vector.x, vector.y, z);
        }
    }
}
