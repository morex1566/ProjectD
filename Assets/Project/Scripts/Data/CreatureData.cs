using System;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    public class CreatureData
    {
        public string DataId;
        public string NameKey;
        public string DescKey;
        public string Faction;
        public float Hp;
        public float Atk;

        public FactionData FactionData;
        public GameObject SpritePf;
        public GameObject CreaturePf;
    }
}
