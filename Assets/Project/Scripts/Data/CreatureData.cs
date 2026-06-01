using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public enum CreatureType
    {
        None = 0,

        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King,
    }

    [Serializable]
    public class CreatureData : ScriptableObject
    {
        private static readonly Dictionary<string, CreatureType> typeByName = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Pawn", CreatureType.Pawn },
            { "Knight", CreatureType.Knight },
            { "Bishop", CreatureType.Bishop },
            { "Rook", CreatureType.Rook },
            { "Queen", CreatureType.Queen },
            { "King", CreatureType.King },
        };

        private static readonly IReadOnlyDictionary<string, MoveRangeData> moveRangeByName = new Dictionary<string, MoveRangeData>(StringComparer.OrdinalIgnoreCase)
        {
            { "King", new MoveRangeData(false, Vector3Int.up, new Vector3Int(1, 1, 0), Vector3Int.right, new Vector3Int(1, -1, 0), Vector3Int.down, new Vector3Int(-1, -1, 0), Vector3Int.left, new Vector3Int(-1, 1, 0)) },
            { "Queen", new MoveRangeData(true, Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left, new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, -1, 0), new Vector3Int(-1, 1, 0)) },
            { "Rook", new MoveRangeData(true, Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left) },
            { "Bishop", new MoveRangeData(true, new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, -1, 0), new Vector3Int(-1, 1, 0)) },
            { "Knight", new MoveRangeData(false, new Vector3Int(1, 2, 0), new Vector3Int(2, 1, 0), new Vector3Int(2, -1, 0), new Vector3Int(1, -2, 0), new Vector3Int(-1, -2, 0), new Vector3Int(-2, -1, 0), new Vector3Int(-2, 1, 0), new Vector3Int(-1, 2, 0)) },
            { "Pawn", new MoveRangeData(false, Vector3Int.up) },
        };

        [ReadOnly] public string Id;

        [ReadOnly] public string PfId;

        [ReadOnly] public string Description;

        [ReadOnly] public string DisplayName;

        [ReadOnly] public string Type;

        [ReadOnly] public string DefaultSkillId;

        [ReadOnly] public int Hp;

        [ReadOnly] public int Damage;

        [ReadOnly] public int Armor;

        [ReadOnly] public MoveRangeData MoveRangeData;

        [ReadOnly] public CreatureType CreatureType;

        [ReadOnly] public GameObject creaturePf;

        public void OnEnable()
        {
            RefreshDerivedData();
        }

        public void RefreshDerivedData()
        {
            if (string.IsNullOrWhiteSpace(Type)) return;

            if (TryGetMoveRangeData(Type, out MoveRangeData moveRangeData))
            {
                MoveRangeData = moveRangeData;
            }
            else
            {
                Debug.LogError("MoveRangeData not found for type: " + Type);
            }

            if (TryGetCreatureType(Type, out CreatureType creatureType))
            {
                CreatureType = creatureType;
            }
            else
            {
                Debug.LogError("Invalid CreatureType: " + Type);
            }
        }

        public static bool TryGetMoveRangeData(string typeName, out MoveRangeData moveRangeData)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                moveRangeData = default;
                return false;
            }

            return moveRangeByName.TryGetValue(typeName, out moveRangeData);
        }

        public static bool TryGetCreatureType(string typeName, out CreatureType creatureType)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                creatureType = CreatureType.None;
                return false;
            }

            return typeByName.TryGetValue(typeName, out creatureType);
        }
    }
}
