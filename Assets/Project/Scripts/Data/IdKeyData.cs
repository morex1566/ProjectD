using UnityEngine;

namespace TRPG.Runtime
{
    [CreateAssetMenu(fileName = "SO_IdKey", menuName = "Scriptable Objects/IdKey")]
    public class IdKeyData : ScriptableObject
    {
        public string Id;
        public string NameKey;
        public string DescKey;
    }
}
