using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    public abstract class MoveRangeData : ScriptableObject
    {
        [SerializeField] protected bool isRepeatable;
        [SerializeField] protected List<Vector3Int> directions = new();

        /// <summary>
        /// King, Knight, Pawn은 쭉 뻗는 기물이 아니라 한 번만 이동하는 패턴이여서 false
        /// Rook, Bishop, Queen은 쭉 뻗는 기물이라 true
        /// </summary>
        public bool IsRepeatable => isRepeatable;

        public List<Vector3Int> Directions
        {
            get
            {
                directions ??= new List<Vector3Int>();

                return directions;
            }
        }

        protected void SetMoveRange(bool repeatable, params Vector3Int[] moveDirections)
        {
            isRepeatable = repeatable;
            directions ??= new List<Vector3Int>();
            directions.Clear();
            directions.AddRange(moveDirections);
        }
    }
}
