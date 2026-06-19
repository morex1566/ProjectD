using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 데이터 ID와 표시/설명 로컬라이징 키를 함께 보관하는 식별 에셋입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_IdKey", menuName = "Scriptable Objects/IdKey")]
    public class IdKeyData : ScriptableObject
    {
        public string Id;
        public string NameKey;
        public string DescKey;
    }
}
