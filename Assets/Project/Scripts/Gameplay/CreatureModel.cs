using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureModel : MonoBehaviour
    {
        [Serializable]
        public class Skill
        {
            [field: SerializeField] public int tilemapGridMoveRange;
            [field: SerializeField] public int damage;
        }

        [field: SerializeField] public List<Skill> skills = new List<Skill>();
    }
}
