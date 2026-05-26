using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// UIManager가 런타임에 인스턴스화할 UI 프리팹 참조를 보관합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_UIManagerSettings", menuName = "Scriptable Objects/Settings/UIManager")]
    public class UIManagerSettingsData : ScriptableObject
    {
        [Header("Gameplay")]

        [SerializeField] private GameObject damageUIPb;

        [SerializeField] private GameObject turnNotifyUIPb;




        public GameObject DamageUIPb => damageUIPb;
    }
}
