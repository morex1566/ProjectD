using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Creature가 순차 실행하는 작업 단위의 공통 베이스입니다.
    /// </summary>
    public abstract class CreatureJob
    {
        public bool IsDone;

        protected CreatureController owner;

        /// <summary>
        /// Job 실행에 필요한 소유 CreatureContext, Job 큐, 우선순위를 저장합니다.
        /// </summary>
        protected CreatureJob(CreatureController owner, CreatureJobQueue queue)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Job의 완료 조건을 평가해 IsDone을 갱신합니다.
        /// </summary>
        public virtual void Execute()
        {
            
        }

        /// <summary>
        /// IsDone 되었는지?
        /// </summary>
        public virtual bool Evaluate()
        {
            return true;
        }

        /// <summary>
        /// Job 디버깅용 기즈모를 그립니다.
        /// </summary>
        public virtual void DrawGizmos()
        {

        }
    }
}
