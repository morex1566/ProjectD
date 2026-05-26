using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 네트워크 연결에 사용할 TCP/HTTP 기본 접속 정보를 보관합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_NetManagerSettings", menuName = "Scriptable Objects/Settings/Net Manager")]
    public class NetManagerSettingsData : ScriptableObject
    {
        [Header("TCP")]
        [SerializeField] private string tcpHost = "192.168.0.3";
        [SerializeField] private int tcpPort = 60000;


        public string TcpHost => tcpHost;
        public int TcpPort => tcpPort;


        [Header("HTTP")]
        [SerializeField] private string httpHost = "192.168.0.3";
        [SerializeField] private int httpPort = 60000;


        public string HttpHost => httpHost;
        public int httphost => httpPort;
    }
}
