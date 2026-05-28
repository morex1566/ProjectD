using System;

namespace TRPG.Runtime
{
    /// <summary>
    /// 이 속성이 붙은 MonoBehaviour는 루트 GameObject에만 붙을 수 있습니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class RootGameObjectOnlyAttribute : Attribute
    {
    }
}
