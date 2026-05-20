using System.Collections;
using UnityEngine;

namespace TRPG.Runtime
{
    [DisallowMultipleComponent]
    public abstract class CreatureController : MonoBehaviour
    {
        [SerializeField, ReadOnly] private CreatureModel creatureModel = null;

        public CreatureModel Model => creatureModel;

        protected Coroutine movement;

        protected Coroutine attack;

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
            creatureModel = GetComponent<CreatureModel>();
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        /// <summary>
        /// 화면 좌표가 Tilemap 레이어의 유효한 셀이면 해당 셀로 이동합니다.
        /// </summary>
        public void Move(Vector3 targetWorldPos, Vector3Int targetCellPos)
        {
            // 이동
            movement = StartCoroutine(Movement(targetWorldPos, targetCellPos));
        }

        public void Attack(CreatureController creatureController)
        {
            creatureController.Hit(Model.Damage);
        }

        public void Hit(float damage)
        {
            Model.SetHp(Model.Hp - damage);
        }

        private IEnumerator Movement(Vector3 targetWorldPos, Vector3Int targetCellPos)
        {
            Vector3 startWorldPos = transform.position;
            targetWorldPos.z = transform.position.z;

            float elapsedTime = 0f;
            while (elapsedTime < 0.25f)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / 0.25f);
                transform.position = Vector3.Lerp(startWorldPos, targetWorldPos, progress);

                yield return null;
            }

            transform.position = targetWorldPos;
            creatureModel.SetCellPos(targetCellPos);
            movement = null;
        }
    }
}
