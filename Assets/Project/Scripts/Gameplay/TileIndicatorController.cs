using System;
using System.Collections;
using UnityEngine;

namespace TRPG.Runtime
{
    public class TileIndicatorController : MonoBehaviour
    {
        private const string MovableStateName = "A_TileIndicator_Open";

        private const string DespawnStateName = "A_TileIndicator_Close";

        private const float MovableAlpha = 100f / 255f;

        private const float MoveToAlpha = 255f / 255f;

        [Header(nameof(TileIndicatorController))]

        [SerializeField, ReadOnly] private CreatureController owner = null;

        [SerializeField, ReadOnly] private SpriteRenderer spriter = null;

        [SerializeField, ReadOnly] private Vector3Int cellPos = Vector3Int.zero;

        [SerializeField] private Animator animator = null;

        private Coroutine movableCoroutine = null;

        private Coroutine despawnCoroutine = null;

        private Action onDespawnCompleted = null;

        private bool isHover = false;

        private bool isDespawning = false;


        public CreatureController Owner => owner;


        /// <summary>
        /// owner 기준 삭제를 위해 생성 시점의 소유자와 CellPos를 기록합니다.
        /// </summary>
        public void Init(CreatureController owner, Vector3Int cellPos)
        {
            this.owner = owner;
            this.cellPos = cellPos;
        }

        private void Awake()
        {
            Bind();
        }

        private void Reset()
        {
            Bind();
        }

        private void OnValidate()
        {
            Bind();
        }

        private void OnEnable()
        {
            PlaySpawn();
        }

        private void OnDisable()
        {
            StopRunningCoroutines();
            onDespawnCompleted = null;
            isHover = false;
            isDespawning = false;
        }

        private void LateUpdate()
        {
            if (isDespawning || movableCoroutine != null) return;

            SetAlpha(isHover ? MoveToAlpha : MovableAlpha);
        }

        /// <summary>
        /// 좌표가 이 타일의 렌더러 영역 안에 있는지 검사합니다.
        /// </summary>
        public bool Contains(Vector3 position)
        {
            if (spriter == null) return false;

            position.z = spriter.bounds.center.z;

            return spriter.bounds.Contains(position);
        }

        /// <summary>
        /// 처음 인스턴싱 되었을 때 이동가능한 부분을 알파값 100 정도로 보여줌
        /// </summary>
        private void PlaySpawn()
        {
            StopRunningCoroutines();

            isHover = false;
            isDespawning = false;

            movableCoroutine = StartCoroutine(PlayMovableCoroutine());
        }

        private IEnumerator PlayMovableCoroutine()
        {
            if (animator == null)
            {
                SetAlpha(MovableAlpha);
                movableCoroutine = null;
                yield break;
            }

            animator.SetBool(UnityConstant.Animator.Parameters.AC_TIleIndicator.Bool.IsHover, false);
            animator.SetTrigger(UnityConstant.Animator.Parameters.AC_TIleIndicator.Trigger.OnSpawn);

            yield return AnimatorEx.WaitForStateExit(animator, MovableStateName);

            if (!isHover && !isDespawning)
            {
                SetAlpha(MovableAlpha);
            }

            movableCoroutine = null;
        }

        /// <summary>
        /// 이동 가능한 기본 상태로 되돌립니다.
        /// </summary>
        public void PlayMovable()
        {
            if (isDespawning) return;

            isHover = false;

            if (animator != null)
            {
                animator.SetBool(UnityConstant.Animator.Parameters.AC_TIleIndicator.Bool.IsHover, false);
            }

            SetAlpha(MovableAlpha);
        }

        /// <summary>
        /// 이동가능한 부분이면 알파값을 255로 진하게 해서 보여줌
        /// </summary>
        public void PlayMoveTo()
        {
            if (isDespawning || isHover) return;

            isHover = true;

            if (animator != null)
            {
                animator.SetBool(UnityConstant.Animator.Parameters.AC_TIleIndicator.Bool.IsHover, true);
            }

            SetAlpha(MoveToAlpha);
        }

        /// <summary>
        /// 삭제되고있는 것을 알파값을 줄여가면서 보여줌
        /// </summary>
        public void PlayDespawn(Action onCompleted)
        {
            if (isDespawning) return;

            isHover = false;
            isDespawning = true;
            onDespawnCompleted = onCompleted;

            if (movableCoroutine != null)
            {
                StopCoroutine(movableCoroutine);
                movableCoroutine = null;
            }

            if (despawnCoroutine != null)
            {
                StopCoroutine(despawnCoroutine);
            }

            despawnCoroutine = StartCoroutine(PlayDespawnCoroutine());
        }

        private IEnumerator PlayDespawnCoroutine()
        {
            if (animator == null)
            {
                CompleteDespawn();
                yield break;
            }

            animator.SetBool(UnityConstant.Animator.Parameters.AC_TIleIndicator.Bool.IsHover, false);
            animator.SetTrigger(UnityConstant.Animator.Parameters.AC_TIleIndicator.Trigger.OnDespawn);

            yield return AnimatorEx.WaitForStateExit(animator, DespawnStateName);

            CompleteDespawn();
        }

        /// <summary>
        /// 현재 스프라이트의 알파값을 변경
        /// </summary>
        public void SetAlpha(float alpha)
        {
            if (spriter == null) return;

            Color color = spriter.color;
            color.a = alpha;
            spriter.color = color;
        }

        private void Bind()
        {
            spriter = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
        }

        private void StopRunningCoroutines()
        {
            if (movableCoroutine != null)
            {
                StopCoroutine(movableCoroutine);
                movableCoroutine = null;
            }

            if (despawnCoroutine != null)
            {
                StopCoroutine(despawnCoroutine);
                despawnCoroutine = null;
            }
        }

        private void CompleteDespawn()
        {
            despawnCoroutine = null;

            Action completed = onDespawnCompleted;
            onDespawnCompleted = null;
            completed?.Invoke();
        }
    }
}
