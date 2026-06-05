using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// NPC 크리처의 런타임 행동을 담당합니다.
    /// </summary>
    public class NPCController : CreatureController
    {
        private void OnEnable()
        {
            PlaySpawnAnim();
        }

        /// <summary>
        /// NPC 스폰 상태로 전환합니다.
        /// </summary>
        private void PlaySpawnAnim()
        {
            animator.SetTrigger(UnityConstant.Animator.Parameters.AC_Gameplay_Creature.Trigger.OnSpawn);
        }
    }
}
