using UnityEngine;

namespace TRPG.Runtime
{
    public interface IWorldCreature
    {
        public string DataId { get; }

        public int InstanceId { get; }
    }
}