using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [CreateAssetMenu(fileName = "SO_Faction", menuName = "Scriptable Objects/Faction/Default")]
    public class FactionData : ScriptableObject
    {
        [SerializeField] private FactionType type;

        [SerializeField] private List<Relation> entries = new();

#if UNITY_EDITOR
        private void Reset()
        {
            entries = new List<Relation>();

            foreach (FactionType factionType in Enum.GetValues(typeof(FactionType)))
            {
                if (factionType == FactionType.None) continue;

                entries.Add(new Relation(factionType, 0));
            }
        }
#endif
    }
}
