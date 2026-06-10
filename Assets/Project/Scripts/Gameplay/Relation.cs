using System;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    public class Relation
    {
        [SerializeField] private FactionType target;

        [SerializeField, Range(-100, 100)] private int value;

        public Relation(FactionType target, int value)
        {
            this.target = target;
            this.value = value;
        }
    }
}
