using DG.Tweening;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 몬스터 크리처의 런타임 입력과 행동을 담당하는 컨트롤러입니다.
    /// </summary>
    public class MonsterController : CreatureController
    {
        [Header(nameof(MonsterController) + ".Setup")]

        [SerializeField, ReadOnly] private DOTweenAnimation moveAnim;

        [SerializeField] private Vector3Range moveAnimFromOffsetRange;

        [SerializeField] private FloatRange moveAnimDurationRange;


        public new MonsterModel Model => base.Model as MonsterModel;

        public bool CanExecuteAIMove => !HasActionFlag(ActionFlag.Moving | ActionFlag.Attacking);


        private void Reset()
        {
            Bind();
        }

        private void OnValidate()
        {
            Bind();
        }

        private void Bind()
        {
            DOTweenAnimation[] anims = GetComponentsInChildren<DOTweenAnimation>();
            foreach (var anim in anims)
            {
                if (anim.animationType == DOTweenAnimation.AnimationType.Move)
                {
                    moveAnim = anim;
                    continue;
                }
            }
        }

        public void OnEnable()
        {
            if (!Application.isPlaying) return;

            // Move
            if (moveAnim == null) return;
            moveAnim.isFrom = true;
            moveAnim.delay = 1f;
            moveAnim.endValueV3 = moveAnimFromOffsetRange.Random();
            moveAnim.duration = moveAnimDurationRange.Random();
            moveAnim.DORestart(true);
        }

        /// <summary>
        /// MonsterAI가 결정한 한 수를 실제 월드 액션으로 실행합니다.
        /// </summary>
        public bool TryExecuteAIMove(AIMove move)
        {
            if (move.Actor != this) return false;

            if (!CanExecuteAIMove) return false;

            if (!WorldManager.TryGetMapWorldPos(move.To, out Vector3 targetWorldPos)) return false;

            if (move.IsAttack)
            {
                Attack(targetWorldPos, move.To, move.Target);
            }
            else
            {
                Move(targetWorldPos, move.To);
            }

            return true;
        }
    }
}
