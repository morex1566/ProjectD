using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// UIManager가 런타임에 사용할 UI 프리팹 설정입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_UIManagerSettings", menuName = "Scriptable Objects/Settings/UI Manager")]
    public class UIManagerSettingsData : ScriptableObject
    {
        /// <summary>
        /// 커서 표시용 프리팹입니다.
        /// </summary>
        public GameObject cursorShape;

        /// <summary>
        /// 로딩 화면 프리팹입니다.
        /// </summary>
        public GameObject loadingUI;

        /// <summary>
        /// 타이틀 메시지 UI 프리팹입니다.
        /// </summary>
        public GameObject titleMessagePf;

        /// <summary>
        /// 대화 UI 프리팹입니다.
        /// </summary>
        public GameObject dialougeUIPf;

        /// <summary>
        /// 패널 UI 프리팹입니다.
        /// </summary>
        public GameObject panelUIPf;

        /// <summary>
        /// 대화 진행 UI 프리팹입니다.
        /// </summary>
        public GameObject conversationUIPf;

        /// <summary>
        /// 턴 UI 프리팹입니다.
        /// </summary>
        public GameObject turnUIPf;

        /// <summary>
        /// 턴 상태 UI 프리팹입니다.
        /// </summary>
        public GameObject turnStateUIPf;
    }
}
