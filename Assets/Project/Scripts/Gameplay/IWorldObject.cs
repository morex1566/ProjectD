using UnityEngine;

namespace TRPG.Runtime
{
    public interface IWorldObject
    {
        public string DataId { get; }

        public int InstanceId { get; }
    }
}