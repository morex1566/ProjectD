using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Creature의 감지 범위 안에 들어온 다른 Creature를 찾고 거리순으로 정렬합니다.
    /// </summary>
    public class CreatureDetector
    {
        private CreatureController owner = null;

        private CreatureContext status = null;

        private readonly List<CreatureController> detectedCreatures = new();

        public IReadOnlyList<CreatureController> DetectedCreatures => detectedCreatures;



        /// <summary>
        /// 감지 주체와 감지에 사용할 런타임 상태값을 저장합니다.
        /// </summary>
        public CreatureDetector(CreatureController owner, CreatureContext status)
        {
            this.owner = owner;
            this.status = status;
        }



        /// <summary>
        /// 현재 씬의 Creature들을 스캔해 감지 범위 안의 대상 목록을 갱신합니다.
        /// </summary>
        public IReadOnlyList<CreatureController> Detect()
        {
            detectedCreatures.Clear();

            // 탐색 시작
            float range = status.DetectRange;
            float sqrRange = range * range;
            CreatureController[] creatures = GameObject.FindObjectsByType<CreatureController>(FindObjectsSortMode.None);
            for (int i = 0; i < creatures.Length; i++)
            {
                if (creatures[i] == owner) continue;

                if (!IsInRange(creatures[i], sqrRange)) continue;

                detectedCreatures.Add(creatures[i]);
            }

            // 가장 가까운 크리쳐가 먼저 오도록 정렬하고, 거리가 같으면 InstanceId로 결과를 고정합니다.
            detectedCreatures.Sort(CompareByDistance);

            return detectedCreatures;
        }

        /// <summary>
        /// 특정 Creature가 감지 범위 안에 있는지 검사합니다.
        /// </summary>
        public bool Detect(CreatureController target)
        {
            if (target == null) return false;
            if (target == owner) return false;

            float range = status.DetectRange;
            float sqrRange = range * range;

            return IsInRange(target, sqrRange);
        }

        /// <summary>
        /// 제곱 거리 기준으로 대상이 감지 범위 안에 있는지 확인합니다.
        /// </summary>
        private bool IsInRange(CreatureController target, float sqrRange)
        {
            float sqrDistance = Vector3.SqrMagnitude(target.transform.position - owner.transform.position);

            return sqrDistance <= sqrRange;
        }

        /// <summary>
        /// 감지 결과를 가까운 순서로 정렬하고 동률이면 InstanceId로 순서를 고정합니다.
        /// </summary>
        private int CompareByDistance(CreatureController a, CreatureController b)
        {
            float aSqrDistance = Vector3.SqrMagnitude(a.transform.position - owner.transform.position);
            float bSqrDistance = Vector3.SqrMagnitude(b.transform.position - owner.transform.position);

            if (!Mathf.Approximately(aSqrDistance, bSqrDistance))
            {
                return aSqrDistance.CompareTo(bSqrDistance);
            }

            return a.InstanceId.CompareTo(b.InstanceId);
        }
    }
}
