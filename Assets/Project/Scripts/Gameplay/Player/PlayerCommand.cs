using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 새 명령을 크리쳐가 Job 큐에 어떻게 넣을지 결정합니다.
    /// </summary>
    public enum PlayerCommandEnqueueType
    {
        Replace,
        Append
    }

    /// <summary>
    /// 플레이어가 내린 명령의 공통 실행 계약입니다.
    /// </summary>
    public abstract class PlayerCommand
    {
        public PlayerCommandEnqueueType EnqueueType;

        protected PlayerCommand(PlayerCommandEnqueueType enqueueType)
        {
            EnqueueType = enqueueType;
        }

        /// <summary>
        /// 선택된 Creature에게 이 명령을 적용합니다.
        /// </summary>
        public abstract void ApplyTo(CreatureController creature);
    }



    /// <summary>
    /// 선택된 Creature를 목표 타일로 이동시키는 명령입니다.
    /// </summary>
    public sealed class AStarMoveCommand : PlayerCommand
    {
        public Vector3Int TargetCellPos;

        public AStarMoveCommand(Vector3Int targetCellPos, PlayerCommandEnqueueType enqueueType) : base(enqueueType)
        {
            TargetCellPos = targetCellPos;
        }

        public override void ApplyTo(CreatureController creature)
        {

        }
    }
}
