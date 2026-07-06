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

        [SerializeField, ReadOnly] private GroundChecker groundChecker = null;

        [SerializeField] private CreatureContext context = null;

        private CreatureJobQueue jobQueue = null;

        private CreatureStateMachine stateMahcine = null;


        public bool CanSelect { get; set; } = true;


        public bool IsSelected { get; set; } = false;

        public Bounds SelectionBounds => spriter.bounds;

        public GameObject SelectedInst => gameObject;

        public int InstanceId => GetInstanceID();

        public SpriteRenderer Spriter => spriter;

        public GroundChecker GroundChecker => groundChecker;

        public CreatureJobQueue JobQueue => jobQueue;

        public CreatureContext Context => context;

        public CreatureStateMachine StateMahcine => stateMahcine;



        private void OnValidate()
        {
            Init();
        }

        private void Awake()
        {
            Init();
        }

        /// <summary>
        /// 매 프레임 현재 큐의 CreatureJob을 실행합니다.
        /// </summary>
        private void Update()
        {
            jobQueue.Update();
            stateMahcine.Update();
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

            // jobqueue and statemachine
            jobQueue?.DrawGizmos();
            stateMahcine?.DrawGizmos();

//#if UNITY_EDITOR
//            // Scene View에 텍스트 표시
//            Vector3 labelPos = transform.position + Vector3.up * 0.75f;
//            string label = $"InstanceId: {InstanceId}\n" + $"DataId: {DataId}";
//            Handles.Label(labelPos, label);

            //            jobQueue.DrawGizmos();
            //#endif
        }



        /// <summary>
        /// CreatureData를 런타임 상태로 변환하고 표시용 스프라이트 프리팹을 연결합니다.
        /// </summary>
        public void Init()
        {
            spriter = GetComponentInChildren<SpriteRenderer>();
            groundChecker = GetComponentInChildren<GroundChecker>();
            jobQueue ??= new CreatureJobQueue(this);
            stateMahcine ??= new CreatureStateMachine(this);
            context ??= new CreatureContext();
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
    }
}
