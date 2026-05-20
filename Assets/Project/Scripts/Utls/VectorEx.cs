using UnityEngine;

namespace TRPG.Runtime
{
    public static class VectorEx
    {
        public static Vector3Int ToVector3Int(this Vector2Int vector, int z = 0)
        {
            return new Vector3Int(vector.x, vector.y, z);
        }
    }
}
