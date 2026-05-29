using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [System.Flags]
    public enum ActionFlag
    {
        None = 0,
        Moving = 1 << 0,
        Attacking = 1 << 1,
    }

    /// <summary>
    /// 캐릭터 조작 클래스
    /// </summary>
    [DisallowMultipleComponent]
    public abstract partial class CreatureController : MonoBehaviour, ISelectable
    {
        [Header(nameof(CreatureController) + ".Setup")]

        [SerializeField, ReadOnly] private CreatureModel model = null;

        [SerializeField, ReadOnly] protected Animator animator = null;

        [SerializeField, ReadOnly] protected CreatureStatsUI statsUI = null;

        [SerializeField] protected DragPendulum2D dragger;

        [SerializeField] protected SpriteRenderer spriter = null;

        [SerializeField] protected SpriteRenderer outliner = null;

        [SerializeField] protected PixelBreaker breaker = null;


        [SerializeField] private Transform statsPivot;

        [SerializeField] private Color outlineColor = Color.green;

        [SerializeField, Min(1)] private int outlinePixelWidth = 1;

        /// <summary>
        /// 목표 타일 위에 살짝 띄운 도착 위치 오프셋
        /// </summary>
        [SerializeField] private Vector3 preLandingOffset = new Vector3(0f, 0.5f, 0f);

        [Header(nameof(CreatureController) + ".Runtime")]

        [SerializeField, ReadOnly] protected ActionFlag actionFlags;

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

        public bool CanSelect { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public CreatureModel Model => model;

        private CreatureMotionSettingsData BattleMotionSettings => Model != null ? Model.BattleMotionSettings : null;

        private float MoveDelay => BattleMotionSettings != null ? BattleMotionSettings.MoveDelay : CreatureMotionSettingsData.DefaultMoveDelay;

        private float StompDelay => BattleMotionSettings != null ? BattleMotionSettings.StompDelay : CreatureMotionSettingsData.DefaultStompDelay;

        private float CollideDelay => BattleMotionSettings != null ? BattleMotionSettings.CollideDelay : CreatureMotionSettingsData.DefaultCollideDelay;

        private float BattleMoveDelay => BattleMotionSettings != null ? BattleMotionSettings.BattleMoveDelay : CreatureMotionSettingsData.DefaultBattleMoveDelay;

        private float BattleStompDelay => BattleMotionSettings != null ? BattleMotionSettings.BattleStompDelay : CreatureMotionSettingsData.DefaultBattleStompDelay;



        protected virtual void Awake()
        {
            model = GetComponent<CreatureModel>();
            animator = GetComponentInChildren<Animator>();
        }

        protected virtual void Start()
        {
            // UI 생성
            //statsUI = UIManager.GetInstance().Open<CreatureStatsUI>(UIManager.RenderSpace.Overlay);
            //statsUI.rectTransform.localPosition = UIManager.GetInstance().WorldPosToUIPos(statsPivot.position, statsUI.rectTransformParent);
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
        /// 크리처 이동 코루틴을 시작합니다.
        /// </summary>
        protected void Move(Vector3 targetWorldPos, Vector3Int targetCellPos)
        {
            // 이동 시작했으니 이동 플래그, 이동 가능한 지역 해제
            actionFlags |= ActionFlag.Moving;

            StartCoroutine(MoveDampCo(targetWorldPos, targetCellPos, MoveDelay, StompDelay));
            StartCoroutine(RotCo(Quaternion.identity));
        }

        protected IEnumerator MoveDampCo(Vector3 targetWorldPos, Vector3Int targetCellPos, float moveDelay, float stompDelay)
        {
            targetWorldPos.z = transform.position.z;

            Vector3 preLandingWorldPos = targetWorldPos + preLandingOffset;

            // 크리쳐가 목표 CellPos 위로 이동
            yield return MovePartCo(preLandingWorldPos, moveDelay);

            // 크리쳐가 목표 CellPos에 스톰핑
            yield return StompPartCo(targetWorldPos, stompDelay);

            // 크리쳐가 전진 끝, 나머지 설정 후처리
            transform.position = targetWorldPos;
            Model.SetCellPos(targetCellPos);
            actionFlags &= ~ActionFlag.Moving;
        }

        protected IEnumerator MoveDampDirectCo(Vector3 targetWorldPos, Vector3Int targetCellPos, float moveDelay, float stompDelay)
        {
            targetWorldPos.z = transform.position.z;

            // preLanding/stomp 없이 목표 위치로 바로 이동하되, 전체 이동 시간은 기존 이동+착지 시간과 맞춥니다.
            yield return MovePartCo(targetWorldPos, moveDelay + stompDelay);

            transform.position = targetWorldPos;
            Model.SetCellPos(targetCellPos);
            actionFlags &= ~ActionFlag.Moving;
        }

        protected IEnumerator MoveAccelerateDirectCo(Vector3 targetWorldPos, Vector3Int targetCellPos, float moveDelay, float stompDelay)
        {
            targetWorldPos.z = transform.position.z;

            Vector3 startWorldPos = transform.position;
            float moveDuration = moveDelay + stompDelay;
            float elapsedTime = 0f;

            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;

                // 충돌 직전으로 갈수록 더 빠르게 붙도록 ease-in quadratic 보간을 사용합니다.
                float linearTime = Mathf.Clamp01(elapsedTime / moveDuration);
                float accelerateTime = linearTime * linearTime * linearTime;
                transform.position = Vector3.LerpUnclamped(startWorldPos, targetWorldPos, accelerateTime);

                yield return null;
            }

            transform.position = targetWorldPos;
            Model.SetCellPos(targetCellPos);
            actionFlags &= ~ActionFlag.Moving;
        }

        private IEnumerator MovePartCo(Vector3 targetWorldPos, float moveDelay)
        {
            Vector3 moveVelocity = Vector3.zero;

            float elapsedTime = 0f;
            while (elapsedTime < moveDelay)
            {
                elapsedTime += Time.deltaTime;

                transform.position = Vector3.SmoothDamp(transform.position, targetWorldPos, ref moveVelocity, moveDelay);

                yield return null;
            }
        }

        private IEnumerator StompPartCo(Vector3 targetWorldPos, float stompDelay)
        {
            Vector3 stompVelocity = Vector3.zero;
            float elapsedTime = 0f;

            while (elapsedTime < stompDelay)
            {
                elapsedTime += Time.deltaTime;

                transform.position = Vector3.SmoothDamp(transform.position, targetWorldPos, ref stompVelocity, stompDelay);

                yield return null;
            }
        }

        protected IEnumerator RotCo(Quaternion targetWorldRot)
        {
            Vector3 rotateVelocity = Vector3.zero;
            Vector3 targetEuler = targetWorldRot.eulerAngles;

            // 크리쳐가 worldRot으로 회전
            float moveDelay = MoveDelay;
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

        public void Attack(Vector3 battleCellWorldPos, Vector3Int battleCellPos, MonsterController targetController)
        {
            // 타겟도 같은 전투 CellPos 기준으로 방어 이동을 시작합니다.
            targetController.Defend(battleCellWorldPos, battleCellPos, this);

            StartCoroutine(AttackCo(battleCellWorldPos, battleCellPos, BattleMoveDelay, BattleStompDelay, CollideDelay));
            StartCoroutine(RotCo(Quaternion.identity));
        }

        public void Defend(Vector3 battleCellWorldPos, Vector3Int battleCellPos, CreatureController attackerController)
        {
            StartCoroutine(DefendCo(battleCellWorldPos, battleCellPos, BattleMoveDelay, BattleStompDelay, CollideDelay));
        }

        /// <summary>
        /// 공격/방어 공통 이동/충돌 연출
        /// </summary>
        public IEnumerator AttackCo(Vector3 battleCellWorldPos, Vector3Int battleCellPos, float moveDelay, float stompDelay, float collideDelay)
        {
            // 공격자는 전투 CellPos의 한쪽으로 이동합니다.
            Vector3 battleStartCellWorldPos = battleCellWorldPos - WorldManager.TileSize / 2;
            Vector3 battleEndCellWorldPos = battleCellWorldPos;

            // 전투 준비 위치로 이동해 양쪽이 잠깐 벌어지는 구도를 만듭니다.
            yield return MoveDampDirectCo(battleStartCellWorldPos, battleCellPos, moveDelay, stompDelay);

            // 대기
            yield return new WaitForSeconds(collideDelay);

            // 전투 시작, 양쪽에서 서로 부딪힘
            yield return MoveAccelerateDirectCo(battleEndCellWorldPos, battleCellPos, moveDelay, stompDelay);
        }

        public IEnumerator DefendCo(Vector3 battleCellWorldPos, Vector3Int battleCellPos, float moveDelay, float stompDelay, float collideDelay)
        {
            // 방어자는 공격자와 반대 방향의 전투 CellPos 위치로 이동합니다.
            Vector3 battleStartCellWorldPos = battleCellWorldPos + WorldManager.TileSize / 2;
            Vector3 battleEndCellWorldPos = battleCellWorldPos;
            Vector3 hitDirection = battleStartCellWorldPos - battleEndCellWorldPos;

            // 전투 준비 위치로 이동해 양쪽이 잠깐 벌어지는 구도를 만듭니다.
            yield return MoveDampDirectCo(battleStartCellWorldPos, battleCellPos, moveDelay, stompDelay);

            // 대기
            yield return new WaitForSeconds(collideDelay);

            // 전투 시작, 양쪽에서 서로 부딪힘
            yield return MoveAccelerateDirectCo(battleEndCellWorldPos, battleCellPos, moveDelay, stompDelay);

            breaker.Break(hitDirection);
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
