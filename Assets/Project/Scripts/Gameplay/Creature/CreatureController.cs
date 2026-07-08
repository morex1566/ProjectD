#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Creature의 런타임 상태, 선택 상태, Job 큐를 관리하는 월드 컴포넌트입니다.
    /// </summary>
    public class CreatureController : MonoBehaviour, ISelectable
    {
        [SerializeField, ReadOnly] private SpriteRenderer spriter = null;

        [SerializeField] private BoxCollider2DSizeFitter collider2DSizeFitter = null;

        [SerializeField] private CreatureIdData idData = null;

        [SerializeField] private CreatureContext context = null;

        private CreatureJobQueue jobQueue = null;


        public bool CanSelect { get; set; } = true;


        public bool IsSelected { get; set; } = false;

        public Bounds SelectionBounds => spriter.bounds;

        public GameObject SelectedInst => gameObject;

        public int InstanceId => gameObject.GetInstanceID();

        public SpriteRenderer Spriter => spriter;

        public CreatureJobQueue JobQueue => jobQueue;

        public CreatureContext Context => context;


        private void OnValidate()
        {
            CacheComponents();
        }

        private void Awake()
        {
            CacheComponents();
            Init();
        }

        private void Start()
        {
            JobQueue.Enqueue(new CreatureWanderJob(this));
        }

        /// <summary>
        /// 매 프레임 현재 큐의 CreatureJob을 실행합니다.
        /// </summary>
        private void Update()
        {
            jobQueue.Update();
        }

        /// <summary>
        /// 인스턴스 아이디 표기
        /// </summary>
        private void OnDrawGizmos()
        {
            // 선택 범위 박스
            if (spriter != null)
            {
                Gizmos.color = IsSelected ? Color.green : Color.yellow;
                Gizmos.DrawWireCube(SelectionBounds.center, SelectionBounds.size);
            }

            if (Application.isPlaying == false)
            {
                return;
            }

//#if UNITY_EDITOR
//            // Scene View에 텍스트 표시
//            Vector3 labelPos = transform.position + Vector3.up * 0.75f;
//            string label = $"InstanceId: {InstanceId}\n" + $"DataId: {DataId}";
//            Handles.Label(labelPos, label);
            //#endif
        }


        /// <summary>
        /// CreatureData를 런타임 상태로 변환하고 표시용 스프라이트 프리팹을 연결합니다.
        /// </summary>
        public void Init()
        {
            jobQueue = new CreatureJobQueue(this);
            context = new CreatureContext();
        }

        /// <summary>
        /// DataId로 CreatureData를 찾아 런타임 컨텍스트에 반영합니다.
        /// </summary>
        public bool LoadContext(string dataId)
        {
            if (WorldManager.TryGetCreatureData(dataId, out CreatureData data) == false)
            {
                Debug.LogWarning($"LoadContext failed. CreatureData not found. DataId: {dataId}");
                return false;
            }

            return LoadContext(data);
        }

        /// <summary>
        /// CreatureData를 런타임 컨텍스트에 반영합니다.
        /// </summary>
        public bool LoadContext(CreatureData data)
        {
            if (data == null)
            {
                Debug.LogWarning("CreatureContext load failed. CreatureData is null.");
                return false;
            }

            Init();
            ApplyContext(data);

            return true;
        }

        /// <summary>
        /// 월드 좌표가 선택 판정 Bounds 안에 있는지 확인합니다.
        /// </summary>
        public bool Contains(Vector3 worldPosition)
        {
            return SelectionBounds.Contains(worldPosition);
        }

        /// <summary>
        /// 선택 상태를 갱신하고 선택 표시 오브젝트를 토글합니다.
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
        }

        public bool IsDead()
        {
            return context.Hp <= 0f;
        }

        private void ApplyContext(CreatureData data)
        {
            context.DataId = data.DataId;
            context.NameKey = data.NameKey;
            context.DescKey = data.DescKey;
            context.Faction = data.Faction;
            context.Hp = data.Hp;
            context.Atk = data.Damage;
            context.DetectRange = data.DetectRange;
            context.AttackRange = data.AttackRange;
            context.AttackSpeed = data.AttackSpeed;
            context.MoveSpeed = data.MoveSpeed;
            context.AIType = CreatureContext.ParseAIType(data.AIType);

            context.Sprite = data.Sprite;
            spriter.sprite = data.Sprite;
            collider2DSizeFitter.Fit();

            context.BehaviourTree = Instantiate(data.BehaviourTreePrefab, transform);
            context.BehaviourTree.transform.localPosition = Vector3.zero;
            context.BehaviourTree.transform.localRotation = Quaternion.identity;
        }

        private void CacheComponents()
        {
            spriter = GetComponentInChildren<SpriteRenderer>();
        }
    }
}
