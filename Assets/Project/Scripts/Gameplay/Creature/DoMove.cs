using UnityEngine;
using MBT;
using System.Collections.Generic;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - DoMove")]
    public class DoMove : Leaf
    {
        private const float ArriveDistance = 0.01f;

        [SerializeField, ReadOnly] private CreatureController controller = null;
        [SerializeField, ReadOnly] private int pathIndex = 1;

        private CreatureMoveJob currentMoveJob = null;
        private IReadOnlyList<AStarNode> path = null;

        private void Awake()
        {
            controller = GetComponentInParent<CreatureController>();
        }

        public override NodeResult Execute()
        {


            return NodeResult.running;
        }

        public override void OnExit()
        {
            // Leaf 실행이 끝나면 다음 이동 명령을 받을 수 있도록 실행 상태를 비웁니다.
            currentMoveJob = null;
            path = null;
            pathIndex = 1;

            base.OnExit();
        }

        public override void DrawGizmos()
        {
            
        }
    }
}
