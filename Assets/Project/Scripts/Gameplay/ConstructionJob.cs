using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 현재 공사되고 있는 곳의 모든 정보들
    /// </summary>
    public class ConstructionJob
    {
        /// <summary>
        /// 공사 완료에 필요한 점수
        /// </summary>
        public float constructionPoint;

        /// <summary>
        /// 공사에 참여할 수 있는 최대 인원수
        /// </summary>
        public int maxWorkerCount;

        /// <summary>
        /// 공사에 참여하고 있는 인원수
        /// </summary>
        public int currWorkerCount;
    }
}
