using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    public struct MoveRangeData
    {
        private static readonly Dictionary<string, MoveRangeData> moveRangeByName = new(StringComparer.OrdinalIgnoreCase)
        {
            { "King", Create(false, Vector3Int.up, new Vector3Int(1, 1, 0), Vector3Int.right, new Vector3Int(1, -1, 0), Vector3Int.down, new Vector3Int(-1, -1, 0), Vector3Int.left, new Vector3Int(-1, 1, 0)) },
            { "Queen", Create(true, Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left, new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, -1, 0), new Vector3Int(-1, 1, 0)) },
            { "Rook", Create(true, Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left) },
            { "Bishop", Create(true, new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, -1, 0), new Vector3Int(-1, 1, 0)) },
            { "Knight", Create(false, new Vector3Int(1, 2, 0), new Vector3Int(2, 1, 0), new Vector3Int(2, -1, 0), new Vector3Int(1, -2, 0), new Vector3Int(-1, -2, 0), new Vector3Int(-2, -1, 0), new Vector3Int(-2, 1, 0), new Vector3Int(-1, 2, 0)) },
            { "Pawn", Create(false, Vector3Int.up) },
        };

        [SerializeField] private bool isRepeatable;
        [SerializeField] private List<Vector3Int> directions;

        private MoveRangeData(bool isRepeatable, IEnumerable<Vector3Int> directions)
        {
            this.isRepeatable = isRepeatable;
            this.directions = new List<Vector3Int>(directions);
        }

        /// <summary>
        /// King, Knight, Pawn은 쭉 뻗는 기물이 아니라 한 번만 이동하는 패턴이여서 false
        /// Rook, Bishop, Queen은 쭉 뻗는 기물이라 true
        /// </summary>
        public bool IsRepeatable => isRepeatable;

        public List<Vector3Int> Directions => directions == null ? new List<Vector3Int>() : new List<Vector3Int>(directions);

        public static bool TryGetByName(string moveRangeName, out MoveRangeData moveRangeData)
        {
            if (string.IsNullOrWhiteSpace(moveRangeName))
            {
                moveRangeData = default;
                return false;
            }

            return moveRangeByName.TryGetValue(moveRangeName, out moveRangeData);
        }

        private static MoveRangeData Create(bool isRepeatable, params Vector3Int[] directions)
        {
            return new MoveRangeData(isRepeatable, directions);
        }
    }
}
