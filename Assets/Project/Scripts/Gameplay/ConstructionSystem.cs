using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public class ConstructionSystem : MonoBehaviour
    {
        public static readonly Queue<ConstructionJob> Queue = new();
    }
}
