using UnityEngine;

namespace TRPG.Runtime
{
    [CreateAssetMenu(fileName = "SO_UIManagerSettings", menuName = "Scriptable Objects/Settings/UIManager")]
    public class UIManagerSettingsData : ScriptableObject
    {
        [Header("Gameplay")]

        [SerializeField] private GameObject damageUIPb;

        [SerializeField] private GameObject turnNotifyUIPb;




        public GameObject DamageUIPb => damageUIPb;
    }
}
