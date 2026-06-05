using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TRPG.Runtime
{
    [CreateAssetMenu(fileName = "SO_DialogueManagerSettings", menuName = "Scriptable Objects/Settings/DialougeManager")]
    public class DialougeManagerSettingsData : ScriptableObject
    {
        [Header(nameof(DialougeManagerSettingsData) + ".tutorial")]

        public AssetReferenceT<DialogueData> TutorialRef;
    }
}
