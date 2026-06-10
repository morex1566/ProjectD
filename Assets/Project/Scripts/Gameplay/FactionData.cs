using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [CreateAssetMenu(fileName = "SO_Faction", menuName = "Scriptable Objects/Faction/Default")]
    public class FactionData : ScriptableObject
    {
        [SerializeField] public FactionType Type;

        [SerializeField] public List<Relation> Entries = new();

#if UNITY_EDITOR
        private void Reset()
        {
            Entries = new List<Relation>();

            foreach (FactionType factionType in Enum.GetValues(typeof(FactionType)))
            {
                if (factionType == FactionType.None) continue;

                Entries.Add(new Relation(factionType, 0));
            }
        }
#endif
    }
}
