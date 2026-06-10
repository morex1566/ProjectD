using UnityEditor;
using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureController : MonoBehaviour, ISelectable, IWorldObject
    {
        [SerializeField] private CreatureData data;

        [SerializeField] private SpriteRenderer spriter;



        public bool CanSelect { get; set; } = true;

        public bool IsSelected { get; set; } = false;

        public Bounds SelectionBounds => spriter.bounds;

        public string DataId => data.DataId;

        public int InstanceId => GetInstanceID();



        public void Init(CreatureData creatureData)
        {
            data = creatureData;

            ClearSpritePf();
            ComposeSpritePf();
        }

        public bool Contains(Vector3 worldPosition)
        {
            return SelectionBounds.Contains(worldPosition);
        }

        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
        }

        private void ClearSpritePf()
        {
            if (spriter == null) return;

            if (spriter.gameObject == gameObject) return;

            Destroy(spriter.gameObject);
            spriter = null;
        }

        private void ComposeSpritePf()
        {
            GameObject spriteObj = Instantiate(data.SpritePf, transform);
            spriteObj.transform.localPosition = Vector3.zero;
            spriteObj.transform.localRotation = Quaternion.identity;
            spriteObj.transform.localScale = Vector3.one;

            spriter = spriteObj.GetComponentInChildren<SpriteRenderer>();
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
#endif
        }
    }
}
