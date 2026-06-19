using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// WorldManager가 생성/제거 대상으로 추적하는 CreatureContext 식별 계약입니다.
    /// </summary>
    public interface IWorldCreature
    {
        public string DataId { get; }

        public int InstanceId { get; }
    }
}
