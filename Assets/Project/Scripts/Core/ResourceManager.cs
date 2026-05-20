using UnityEngine;

namespace TRPG.Runtime
{
    public class ResourceManager : MonoBehaviourSingleton<ResourceManager>
    {
        private static ResourceManagerSettingsData data;

        /// <summary>
        /// 리소스 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
            {
                data = Resources.Load<ResourceManagerSettingsData>("SO_ResourceManagerSettings");
            }
        }
    }
}
