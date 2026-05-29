using System;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    public class StageData : ScriptableObject
    {
        [ReadOnly] public string Id;

        [ReadOnly] public string MapId;

        [ReadOnly] public string Description;

        [ReadOnly] public int Pawn;

        [ReadOnly] public int Knight;

        [ReadOnly] public int Bishop;

        [ReadOnly] public int Rook;

        [ReadOnly] public int Queen;

        [ReadOnly] public int King;

        [ReadOnly] public int Total;
    }
}
