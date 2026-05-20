using UnityEngine;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    public enum NetProtocolType
    {
        TCP,
        UDP,
        HTTP
    }

    public class NetManager : MonoBehaviourSingleton<NetManager>
    {
        private static NetManagerSettingsData data;

//        private static TCP tcp;

//        public static TCP TCP => tcp;

//#if UNITY_EDITOR
//        [MenuItem("Network/Init")]
//#endif
//        public static bool Init()
//        {
//            settings = Resources.Load<NetManagerSettingsData>("SO_NetManagerSettings");
//            if (settings == null)
//            {
//                Debug.LogError("SO_NetManagerSettings resource was not found.");
//                return false;
//            }

//            GetInstance();
//            {
//                tcp = TCP.GetInstance();
//                {
//                    bool succeeded = tcp.Init(settings.TcpHost, settings.TcpPort);
//                    if (!succeeded)
//                    {
//                        Debug.LogWarning("TCP init failed.");
//                    }
//                }
//            }

//            return true;
//        }

//#if UNITY_EDITOR
//        [MenuItem("Network/ConnectAsync")]
//#endif
//        public static async Task<bool> ConnectAsync()
//        {
//            bool succeeded = await tcp.ConnectAsync();
//            if (!succeeded)
//            {
//                Debug.LogWarning("TCP connect failed.");
//            }

//            return succeeded;
//        }

//#if UNITY_EDITOR
//        [MenuItem("Network/Disconnect")]
//#endif
//        public static bool Disconnect()
//        {
//            bool succeeded = tcp.Disconnect();
//            if (!succeeded)
//            {
//                Debug.LogWarning("TCP disconnect failed.");
//            }

//            return succeeded;
//        }
    }
}
