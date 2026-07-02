#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// CreatureContext AI의 이동/행동 타입입니다.
    /// </summary>
    public enum CreatureAIType
    {
        Ground
    }

    /// <summary>
    /// Creature의 런타임 상태, 선택 상태, Job 큐를 관리하는 월드 컴포넌트입니다.
    /// </summary>
    public class CreatureController : MonoBehaviour, ISelectable
    {
        [SerializeField] private CreatureData data = null;

        [SerializeField] private SpriteRenderer spriter = null;

        private CreatureJobQueue jobQueue = null;

        private CreatureContext context = null;



        /// <summary>
        /// 현재 이 생명체가 조종받는 대상
        /// </summary>
        public GameObject Owner { get; private set; } = null;

        public bool CanSelect { get; set; } = true;

        public bool IsSelected { get; set; } = false;

        public Bounds SelectionBounds => spriter.bounds;

        public string DataId => data.DataId;

        public int InstanceId => GetInstanceID();

        public SpriteRenderer Spriter => spriter;



        /// <summary>
        /// 매 프레임 현재 큐의 CreatureJob을 실행합니다.
        /// </summary>
        private void Update()
        {
            jobQueue.Execute();
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
        public void Init(CreatureData creatureData)
        {
            data = creatureData;
            jobQueue = new CreatureJobQueue();
            context = new CreatureContext(data);

            ClearSprite();
            SetSprite();
        }

        /// <summary>
        /// 사용자가 조작하는 상태인지?
        /// </summary>
        public void SetOwner(GameObject owner)
        {
            this.Owner = owner;
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
            ShowSelectionIndicator(isSelected);
        }



        /// <summary>
        /// 선택 표시 오브젝트를 켜거나 끕니다.
        /// </summary>
        private void ShowSelectionIndicator(bool isVisible)
        {
            //if (selectionIndicator == null) return;

            //selectionIndicator.SetActive(isVisible);
        }

        /// <summary>
        /// 이전에 붙어 있던 외부 스프라이트 프리팹 인스턴스를 제거합니다.
        /// </summary>
        private void ClearSprite()
        {
            if (spriter == null) return;

            if (spriter.gameObject == gameObject) return;

            Destroy(spriter.gameObject);
            spriter = null;
        }

        /// <summary>
        /// CreatureData에 지정된 스프라이트 프리팹을 생성하고 SpriteRenderer를 캐싱합니다.
        /// </summary>
        private void SetSprite()
        {
            GameObject spriteObj = Instantiate(data.SpritePf, transform);
            spriteObj.transform.localPosition = Vector3.zero;
            spriteObj.transform.localRotation = Quaternion.identity;
            spriteObj.transform.localScale = Vector3.one;

            spriter = spriteObj.GetComponentInChildren<SpriteRenderer>();
        }
    }
}
