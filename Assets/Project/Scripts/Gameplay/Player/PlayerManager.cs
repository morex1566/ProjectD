using UnityEngine;

namespace TRPG.Runtime
{
    public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
    {
        public static PlayerManagerSettings Settings;

        private static ObjectSelector selector;

        private void Awake()
        {
            Settings = ResourceManager.GetResource<PlayerManagerSettings>(UnityConstant.Addressable.Label.Core);
        }

        private void Start()
        {
            selector = Instantiate(Settings.ObjectSelector, Settings.ObjectSelector.transform.position, Quaternion.identity).GetComponent<ObjectSelector>();
        }
    }
}
