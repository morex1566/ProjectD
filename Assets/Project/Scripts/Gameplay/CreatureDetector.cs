using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureDetector
    {
        private CreatureController owner = null;

        private CreatureStatus status = null;

        private readonly List<CreatureController> detectedCreatures = new();

        public IReadOnlyList<CreatureController> DetectedCreatures => detectedCreatures;



        public CreatureDetector(CreatureController owner, CreatureStatus status)
        {
            this.owner = owner;
            this.status = status;
        }



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

        public bool Detect(CreatureController target)
        {
            if (target == null) return false;
            if (target == owner) return false;

            float range = status.DetectRange;
            float sqrRange = range * range;

            return IsInRange(target, sqrRange);
        }

        private bool IsInRange(CreatureController target, float sqrRange)
        {
            float sqrDistance = Vector3.SqrMagnitude(target.transform.position - owner.transform.position);

            return sqrDistance <= sqrRange;
        }

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
