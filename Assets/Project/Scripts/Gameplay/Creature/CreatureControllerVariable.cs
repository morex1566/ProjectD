using System;
using UnityEngine;
using MBT;

namespace TRPG.Runtime
{
    ///<summary>
    /// Blackboard에 저장할 CreatureController 변수입니다.
    ///</summary>
    [AddComponentMenu("")]
    public class CreatureControllerVariable : Variable<CreatureController>
    {
        ///<summary>
        /// 값이 변경되었는지 비교합니다.
        ///</summary>
        protected override bool ValueEquals(CreatureController val1, CreatureController val2)
        {
            // UnityEngine.Object 계열은 == 비교로 null 처리까지 비교합니다.
            return val1 == val2;
        }
    }

    ///<summary>
    /// CreatureControllerVariable을 노드에서 참조하기 위한 Reference 클래스입니다.
    ///</summary>
    [Serializable]
    public class CreatureControllerReference : VariableReference<CreatureControllerVariable, CreatureController>
    {
        ///<summary>
        /// 기본 생성자입니다.
        ///</summary>
        public CreatureControllerReference(VarRefMode mode = VarRefMode.EnableConstant)
        {
            SetMode(mode);
        }

        protected override bool isConstantValid
        {
            get { return constantValue != null; }
        }

        public CreatureController Value
        {
            get
            {
                return (useConstant) ? constantValue : this.GetVariable().Value;
            }
            set
            {
                if (useConstant)
                {
                    constantValue = value;
                }
                else
                {
                    this.GetVariable().Value = value;
                }
            }
        }
    }
}