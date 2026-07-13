using System;

namespace TRPG.Runtime
{
    /// <summary>
    /// 플레이어 시스템의 생명주기 진입점입니다.
    /// </summary>
    public class PlayerManager : MonoBehaviourSingleton<PlayerManager>, IDisposable
    {
        private bool isDisposed = false;


        /// <summary>
        /// 플레이어 매니저 인스턴스를 준비합니다.
        /// </summary>
        public static void Init()
        {
            PlayerManager manager = GetInstance();
            manager.isDisposed = false;
        }

        /// <summary>
        /// 플레이어 시스템의 런타임 상태를 정리합니다.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed == true)
            {
                return;
            }

            isDisposed = true;
        }

        protected override void OnDestroy()
        {
            Dispose();
            base.OnDestroy();
        }
    }
}
