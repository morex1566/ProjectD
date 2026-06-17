#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TRPG.Runtime
{
    public enum CreatureAIType
    {
        Ground
    }

    public class CreatureController : MonoBehaviour, ISelectable, IWorldCreature
    {
        [SerializeField] private CreatureData data = null;

        [SerializeField] private SpriteRenderer spriter = null;

        [SerializeField] private GameObject selectionIndicator = null;

        public CreatureJobMachine JobMachine = null;

        public CreatureStatus Status = null;

        public CreatureDetector Detector = null;



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



        private void Update()
        {
            JobMachine.Execute();
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

#if UNITY_EDITOR
            // Scene View에 텍스트 표시
            Vector3 labelPos = transform.position + Vector3.up * 0.75f;
            string label = $"InstanceId: {InstanceId}\n" + $"DataId: {DataId}";
            Handles.Label(labelPos, label);

            JobMachine.DrawGizmos();
#endif
        }



        public void Init(CreatureData creatureData)
        {
            data = creatureData;
            JobMachine = new CreatureJobMachine();
            Status = data.Create();
            Detector = new CreatureDetector(this, Status);

            ClearSpritePf();
            SetSpritePf();
        }

        /// <summary>
        /// 사용자가 조작하는 상태인지?
        /// </summary>
        public void SetOwner(GameObject owner)
        {
            this.Owner = owner;
        }

        public bool Contains(Vector3 worldPosition)
        {
            return SelectionBounds.Contains(worldPosition);
        }

        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
            ShowSelectionIndicator(isSelected);
        }

        public void EnqueueMove(Vector3 targetPos, CommandEnqueueType mode)
        {
            if (mode == CommandEnqueueType.Replace)
            {
                JobMachine.Clear();
            }

            JobMachine.Enqueue(new CreatureMoveJob(targetPos, Status.MoveSpeed, this, JobMachine, 1));
        }

        public void EnqueueConstruct(DigSystem digSystem, CommandEnqueueType mode)
        {
            if (mode == CommandEnqueueType.Replace)
            {
                JobMachine.Clear();
            }

            JobMachine.Enqueue(new CreatureDigJob(digSystem, this, JobMachine, 1));
        }

        private void ShowSelectionIndicator(bool isVisible)
        {
            if (selectionIndicator == null) return;

            selectionIndicator.SetActive(isVisible);
        }

        private void ClearSpritePf()
        {
            if (spriter == null) return;

            if (spriter.gameObject == gameObject) return;

            Destroy(spriter.gameObject);
            spriter = null;
        }

        private void SetSpritePf()
        {
            GameObject spriteObj = Instantiate(data.SpritePf, transform);
            spriteObj.transform.localPosition = Vector3.zero;
            spriteObj.transform.localRotation = Quaternion.identity;
            spriteObj.transform.localScale = Vector3.one;

            spriter = spriteObj.GetComponentInChildren<SpriteRenderer>();
        }
    }
}
