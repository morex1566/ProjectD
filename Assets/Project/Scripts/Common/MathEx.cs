using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Unity 벡터 타입 간 변환을 제공하는 확장 메서드 모음입니다.
    /// </summary>
    public static class MathEx
    {
        /// <summary>
        /// Vector2Int 좌표에 z 값을 더해 Vector3Int 좌표로 변환합니다.
        /// </summary>
        public static Vector3Int ToVector3Int(this Vector2Int vector, int z = 0)
        {
            return new Vector3Int(vector.x, vector.y, z);
        }
    }

    /// <summary>
    /// Vector3 최소/최대 범위와 랜덤 선택 기능을 보관합니다.
    /// </summary>
    [System.Serializable]
    public struct Vector3Range
    {
        public Vector3 Min;
        public Vector3 Max;

        /// <summary>
        /// Min과 Max 사이의 임의 Vector3 값을 반환합니다.
        /// </summary>
        public Vector3 Random()
        {
            return new Vector3
            (
                UnityEngine.Random.Range(Min.x, Max.x),
                UnityEngine.Random.Range(Min.y, Max.y),
                UnityEngine.Random.Range(Min.z, Max.z)
            );
        }
    }

    /// <summary>
    /// float 최소/최대 범위와 랜덤 선택 기능을 보관합니다.
    /// </summary>
    [System.Serializable]
    public struct FloatRange
    {
        public float Min;
        public float Max;

        /// <summary>
        /// Min과 Max 사이의 임의 float 값을 반환합니다.
        /// </summary>
        public float Random()
        {
            return UnityEngine.Random.Range(Min, Max);
        }
    }

    /// <summary>
    /// int 최소/최대 범위와 랜덤 선택 기능을 보관합니다.
    /// </summary>
    [System.Serializable]
    public struct IntRange
    {
        public int Min;
        public int Max;

        /// <summary>
        /// Min 이상 Max 미만의 임의 int 값을 반환합니다.
        /// </summary>
        public int Random()
        {
            return UnityEngine.Random.Range(Min, Max);
        }
    }
}
