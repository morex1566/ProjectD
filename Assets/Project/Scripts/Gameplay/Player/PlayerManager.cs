namespace TRPG.Runtime
{
    /// <summary>
    /// 플레이어 시스템의 생명주기 진입점입니다.
    /// </summary>
    public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
    {
        /// <summary>
        /// 플레이어 매니저 인스턴스를 준비합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
        }
    }
}
