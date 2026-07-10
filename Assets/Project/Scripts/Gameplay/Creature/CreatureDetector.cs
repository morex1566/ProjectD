using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    ///<summary>
    /// Creature의 감지 범위 안에 들어온 다른 Creature를 찾고 거리순으로 정렬합니다.
    ///</summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class CreatureDetector : MonoBehaviour
    {
        ///<summary>
        /// 이 감지기를 소유한 CreatureController입니다.
        ///</summary>
        [SerializeField, ReadOnly] private CreatureController controller = null;

        [SerializeField, ReadOnly] private CircleCollider2D detectionCollider = null;

        [SerializeField] private float radius = 1.0f;


        ///<summary>
        /// 감지 범위 안에 들어온 Creature 목록입니다.
        ///</summary>
        private readonly List<CreatureController> detecteds = new();

        ///<summary>
        /// 감지된 Creature 목록입니다.
        ///</summary>
        public IReadOnlyList<CreatureController> Detecteds => detecteds;

        private void OnValidate()
        {
            detectionCollider = GetComponent<CircleCollider2D>();
        }

        ///<summary>
        /// 컴포넌트가 처음 생성될 때 소유자를 찾습니다.
        ///</summary>
        private void Awake()
        {
            controller = gameObject.GetComponentInHierarchy<CreatureController>();
        }

        ///<summary>
        /// 비활성화될 때 감지 목록을 초기화합니다.
        ///</summary>
        private void OnDisable()
        {
            detecteds.Clear();
        }

        ///<summary>
        /// 감지 범위 안으로 Collider2D가 들어왔을 때 호출됩니다.
        ///</summary>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 감지 대상이 creature임?
            if (TryGetCreatureController(collision, out CreatureController target) == false)
            {
                return;
            }

            // 이미 등록된 녀석?
            if (detecteds.Contains(target) == true)
            {
                return;
            }

            detecteds.Add(target);

            Refresh();
        }

        ///<summary>
        /// 감지 범위 밖으로 Collider2D가 나갔을 때 호출됩니다.
        ///</summary>
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (TryGetCreatureController(collision, out CreatureController target) == false)
            {
                return;
            }

            detecteds.Remove(target);

            Refresh();
        }

        public void SetRadius(float radius)
        {
            this.radius = radius;
            detectionCollider.radius = radius;
        }

        public bool IsEnemyDetected(out CreatureController enemy)
        {
            enemy = default;

            foreach (var detected in detecteds)
            {
                if (Faction.GetRelationType(controller.Context.Faction, detected.Context.Faction) == RelationType.Hostile)
                {
                    enemy = detected;
                    return true;
                }
            }

            return false;
        }

        ///<summary>
        /// 감지 목록에서 유효하지 않은 대상을 제거하고 거리순으로 정렬합니다.
        ///</summary>
        private void Refresh()
        {
            for (int i = detecteds.Count - 1; i >= 0; i--)
            {
                CreatureController detected = detecteds[i];

                if (detected == null)
                {
                    detecteds.RemoveAt(i);
                    continue;
                }

                if (detected == controller)
                {
                    detecteds.RemoveAt(i);
                    continue;
                }

                if (CheckTargetIsDead(detected) == true)
                {
                    detecteds.RemoveAt(i);
                    continue;
                }

                if (detected.gameObject.activeInHierarchy == false)
                {
                    detecteds.RemoveAt(i);
                    continue;
                }
            }

            // 가까운 Creature가 앞에 오도록 거리순 정렬합니다.
            detecteds.Sort(CompareByDistance);
        }

        ///<summary>
        /// 두 Creature를 감지기 기준 거리순으로 비교합니다.
        ///</summary>
        private int CompareByDistance(CreatureController a, CreatureController b)
        {
            if (a == null && b == null)
            {
                return 0;
            }

            if (a == null)
            {
                return 1;
            }

            if (b == null)
            {
                return -1;
            }

            // sqrt 계산을 피하기 위해 sqrMagnitude를 사용합니다.
            float aDistance = (a.transform.position - transform.position).sqrMagnitude;
            float bDistance = (b.transform.position - transform.position).sqrMagnitude;

            return aDistance.CompareTo(bDistance);
        }

        ///<summary>
        /// 대상 Creature가 죽었는지 확인합니다.
        ///</summary>
        private bool CheckTargetIsDead(CreatureController creature)
        {
            if (creature == null)
            {
                return true;
            }

            return creature.IsDead();
        }

        ///<summary>
        /// Collider2D에서 CreatureController를 찾습니다.
        ///</summary>
        private bool TryGetCreatureController(Collider2D collision, out CreatureController target)
        {
            target = null;

            if (collision == null)
            {
                return false;
            }

            target = collision.GetComponentInParent<CreatureController>();

            if (target == null)
            {
                return false;
            }

            if (target == controller)
            {
                return false;
            }

            return true;
        }
    }
}
