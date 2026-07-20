using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureDetector : MonoBehaviour
    {
        private List<CreatureController> detecteds = new();

        public IReadOnlyList<CreatureController> Detecteds => detecteds;
    }
}
