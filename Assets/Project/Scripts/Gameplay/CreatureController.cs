using System.Collections;
using UnityEngine;

namespace TRPG.Runtime
{
    [System.Flags]
    public enum ActionFlag
    {
        None = 0,
        Moving = 1 << 0
    }

    /// <summary>
    /// 캐릭터 조작 클래스
    /// </summary>
    [DisallowMultipleComponent]
    public abstract partial class CreatureController : MonoBehaviour, ISelectable
    {
        [Header("CreatureController.Comp")]

        [SerializeField, ReadOnly] private SpriteRenderer spriteRenderer = null;

        [SerializeField, ReadOnly] private Animator animator = null;

        [SerializeField, ReadOnly] private CreatureModel model = null;

        [Header("CreatureController.View")]

        [SerializeField] private Color hitFlashColor = Color.red;

        [SerializeField] private float hitFlashDuration = 0.08f;

        [Header("CreatureController.Runtime")]

        [SerializeField, ReadOnly] private ActionFlag actionFlags;

        /// <summary>
        /// 목표 타일 위에 살짝 띄운 도착 위치 오프셋
        /// </summary>
        [SerializeField] private Vector3 preLandingOffset = new Vector3(0f, 0.5f, 0f);

        /// <summary>
        /// 크리쳐를 이동할 때, 해당 타일 칸 위로 이동하는데 걸리는 시간(애니메이션)
        /// </summary>
        [SerializeField] private float moveDelay = 0.01f;

        /// <summary>
        /// 크리쳐를 이동할 때, 해당 타일 칸 위에서 정중앙으로 내려찍는데 걸리는 시간(애니메이션)
        /// </summary>
        [SerializeField] private float stompDelay = 0.05f;

        /// <summary>
        /// 맞았을 때 효과
        /// </summary>
        private Coroutine hitFlashCoroutine;

        private Coroutine moving;



        public bool CanSelect { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public CreatureModel Model => model;



        private void OnValidate()
        {
            Init();
        }

        protected virtual void Awake()
        {
            Init();
        }

        private void Init()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
            model = GetComponent<CreatureModel>();
        }

        private void OnEnable()
        {
            actionFlags = ActionFlag.None;
        }

        private void OnDisable()
        {
            actionFlags = ActionFlag.None;
        }

        /// <summary>
        /// 좌표가 이 크리처의 렌더러 영역 안에 있는지 검사합니다.
        /// </summary>
        public bool Contains(Vector3 position)
        {
            position.z = spriteRenderer.bounds.center.z;

            return spriteRenderer.bounds.Contains(position);
        }

        /// <summary>
        /// 현재 선택 상태를 저장합니다.
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
        }

        /// <summary>
        /// 피해량만큼 HP를 감소시킵니다.
        /// </summary>
        public void Hit(CreatureController attacker, float damage)
        {
            // 로직
            float calculatedDamage = Mathf.Clamp(damage - model.Armor, 0f, 99999);
            model.SetHp(model.Hp - calculatedDamage);

            // 뷰
            // 맞는 효과
            // 플레이어와 대상의 사이에서 Damage UI콜
            PlayHitFlash();
            Vector3 center = (attacker.transform.position + transform.position) * 0.5f;
            UIManager.GetInstance().ShowDamage(center, calculatedDamage);

            if (CheckIsDead()) WorldManager.GetInstance().Despawn(GetInstanceID());
        }

        public bool CheckIsDead()
        {
            return model.Hp <= 0;
        }

        /// <summary>
        /// 화면 좌표가 Tilemap 레이어의 유효한 셀이면 해당 셀로 이동합니다.
        /// </summary>
        protected void Move
        (
            Vector3 targetWorldPos,
            Vector3Int targetCellPos,
            Quaternion targetRot,
            bool usePreLanding = true
        )
        {
            // 이동 시작했으니 이동 플래그, 이동 가능한 지역 해제
            actionFlags |= ActionFlag.Moving;
            WorldManager.GetInstance().ClearMoveRange();

            StartCoroutine(MoveCo(targetWorldPos, targetCellPos, usePreLanding));
            StartCoroutine(RotCo(targetRot));
        }

        protected IEnumerator MoveCo(Vector3 targetWorldPos, Vector3Int targetCellPos, bool usePreLanding)
        {
            targetWorldPos.z = transform.position.z;

            if (!usePreLanding)
            {
                yield return MoveDirectCo(targetWorldPos);

                transform.position = targetWorldPos;
                Model.SetCellPos(targetCellPos);
                actionFlags &= ~ActionFlag.Moving;

                yield break;
            }

            // 크리쳐가 cellPos 위로 이동
            Vector3 preLandingWorldPos = targetWorldPos + preLandingOffset;
            Vector3 moveVelocity = Vector3.zero;
            float elapsedTime = 0f;
            while (elapsedTime < moveDelay)
            {
                elapsedTime += Time.deltaTime;

                transform.position = Vector3.SmoothDamp(transform.position, preLandingWorldPos, ref moveVelocity, moveDelay);

                yield return null;
            }

            // 크리쳐가 cellPos에 스톰핑
            Vector3 stompVelocity = Vector3.zero;
            elapsedTime = 0f;
            while (elapsedTime < stompDelay)
            {
                elapsedTime += Time.deltaTime;

                transform.position = Vector3.SmoothDamp(transform.position, targetWorldPos, ref stompVelocity, stompDelay);

                yield return null;
            }

            // 크리쳐가 전진 끝, 나머지 설정 후처리
            transform.position = targetWorldPos;
            Model.SetCellPos(targetCellPos);
            actionFlags &= ~ActionFlag.Moving;
        }

        private IEnumerator MoveDirectCo(Vector3 targetWorldPos)
        {
            Vector3 moveVelocity = Vector3.zero;
            float elapsedTime = 0f;
            float returnDelay = moveDelay + stompDelay;

            while (elapsedTime < returnDelay)
            {
                elapsedTime += Time.deltaTime;

                transform.position = Vector3.SmoothDamp
                (
                    transform.position,
                    targetWorldPos,
                    ref moveVelocity,
                    moveDelay
                );

                yield return null;
            }
        }

        protected IEnumerator RotCo(Quaternion targetWorldRot)
        {
            Vector3 rotateVelocity = Vector3.zero;
            Vector3 targetEuler = targetWorldRot.eulerAngles;

            // 크리쳐가 worldRot으로 회전
            float elapsedTime = 0f;
            while (elapsedTime < moveDelay)
            {
                elapsedTime += Time.deltaTime;

                Vector3 currentEuler = transform.rotation.eulerAngles;

                currentEuler.x = Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref rotateVelocity.x, moveDelay);
                currentEuler.y = Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref rotateVelocity.y, moveDelay);
                currentEuler.z = Mathf.SmoothDampAngle(currentEuler.z, targetEuler.z, ref rotateVelocity.z, moveDelay);

                transform.rotation = Quaternion.Euler(currentEuler);

                yield return null;
            }

            // 크리쳐가 회전 끝, 나머지 설정 후처리
            transform.rotation = targetWorldRot;
        }

        protected bool HasActionFlag(ActionFlag flag)
        {
            return (actionFlags & flag) != ActionFlag.None;
        }
    }

    /// <summary>
    /// 크리처 피격 연출을 담당하는 뷰 로직입니다.
    /// </summary>
    public abstract partial class CreatureController : MonoBehaviour, ISelectable
    {
        private void PlayHitFlash()
        {
            if (hitFlashCoroutine != null) StopCoroutine(hitFlashCoroutine);

            hitFlashCoroutine = StartCoroutine(HitFlashCo());
        }

        protected void PlayPick()
        {
            animator.SetTrigger(UnityConstant.Animator.Parameters.AC_Gameplay_Creature.Trigger.OnPick);
        }

        protected void PlayDrop()
        {
            animator.SetTrigger(UnityConstant.Animator.Parameters.AC_Gameplay_Creature.Trigger.OnDrop);
        }

        private IEnumerator HitFlashCo()
        {
            Color originColor = spriteRenderer.color;

            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(hitFlashDuration);

            spriteRenderer.color = originColor;
            hitFlashCoroutine = null;
        }
    }
}
