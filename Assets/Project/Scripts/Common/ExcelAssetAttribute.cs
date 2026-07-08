using System;

namespace TRPG.Runtime
{
    /// <summary>
    /// ScriptableObject 타입을 엑셀 임포터와 연결하기 위한 설정 속성입니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExcelAssetAttribute : Attribute
    {
        public string AssetPath { get; set; }
        public string ExcelName { get; set; }
        public bool LogOnImport { get; set; }
    }
}
