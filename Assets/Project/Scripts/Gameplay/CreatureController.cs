using System.Collections;
using System.Collections.Generic;
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

        [SerializeField, ReadOnly] private CreatureModel model = null;

        [SerializeField, ReadOnly] protected Animator animator = null;

        [SerializeField] protected SpriteRenderer spriter = null;

        [SerializeField] protected SpriteRenderer outliner = null;

        [Header("CreatureController.View")]

        [SerializeField] private Color outlineColor = Color.green;

        [SerializeField, Min(1)] private int outlinePixelWidth = 1;

        private static readonly Vector3[] OutlineDirections =
        {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right,
            new Vector3(-1f, 1f, 0f),
            new Vector3(1f, 1f, 0f),
            new Vector3(-1f, -1f, 0f),
            new Vector3(1f, -1f, 0f)
        };

        private readonly List<SpriteRenderer> outliners = new();

        [Header("CreatureController.Runtime")]

        [SerializeField, ReadOnly] protected ActionFlag actionFlags;

        [SerializeField, ReadOnly] protected List<Vector3Int> movableCellPosList = new();

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
            model = GetComponent<CreatureModel>();
            animator = GetComponentInChildren<Animator>();
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
            position.z = spriter.bounds.center.z;

            return spriter.bounds.Contains(position);
        }

        /// <summary>
        /// 현재 선택 상태를 저장합니다.
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
        }

        /// <summary>
        /// 화면 좌표가 Tilemap 레이어의 유효한 셀이면 해당 셀로 이동합니다.
        /// </summary>
        protected void Move(Vector3 targetWorldPos, Vector3Int targetCellPos, Quaternion targetRot, bool usePreLanding = true)
        {
            // 이동 시작했으니 이동 플래그, 이동 가능한 지역 해제
            actionFlags |= ActionFlag.Moving;

            WorldManager.GetInstance().RemoveTileIndicators(this);

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
        protected void PlayPick()
        {
            animator.SetTrigger(UnityConstant.Animator.Parameters.AC_Gameplay_Creature.Trigger.OnPick);
        }

        protected void PlayDrop()
        {
            animator.SetTrigger(UnityConstant.Animator.Parameters.AC_Gameplay_Creature.Trigger.OnDrop);
        }

        public void SetOutline(bool active)
        {
            EnsureOutlineRenderers();

            foreach (SpriteRenderer outlineRenderer in outliners)
            {
                outlineRenderer.gameObject.SetActive(active);
            }

            if (!active || spriter.sprite == null)
            {
                return;
            }

            float pixelSize = Mathf.Max(1, outlinePixelWidth) / spriter.sprite.pixelsPerUnit;
            for (int i = 0; i < outliners.Count; i++)
            {
                ApplyOutlineRenderer(outliners[i], OutlineDirections[i] * pixelSize);
            }
        }

        private void EnsureOutlineRenderers()
        {
            outliners.RemoveAll(outlineRenderer => outlineRenderer == null);

            if (outliners.Count == 0)
            {
                outliners.Add(outliner);
            }

            for (int i = outliners.Count; i < OutlineDirections.Length; i++)
            {
                SpriteRenderer outlineRenderer = Instantiate(outliner, outliner.transform.parent);

                outlineRenderer.name = $"{outliner.name}_{i}";
                outliners.Add(outlineRenderer);
            }
        }

        private void ApplyOutlineRenderer(SpriteRenderer outlineRenderer, Vector3 localOffset)
        {
            outlineRenderer.sprite = spriter.sprite;
            outlineRenderer.color = Color.white;

            // Scale 확대 대신 PPU 기준 픽셀 단위 offset으로 외곽선을 맞춥니다.
            outlineRenderer.transform.localPosition = localOffset;
            outlineRenderer.transform.localScale = Vector3.one;

            outlineRenderer.sortingLayerID = spriter.sortingLayerID;
            outlineRenderer.sortingOrder = spriter.sortingOrder - 1;
            outlineRenderer.flipX = spriter.flipX;
            outlineRenderer.flipY = spriter.flipY;
            outlineRenderer.material.SetColor("_Color", outlineColor);
        }
    }
}
