using UnityEngine;
using UnityEngine.UI;

namespace TRPG.Runtime
{
    /// <summary>
    /// 플레이어 명령 모드를 변경하는 액션 버튼 UI입니다.
    /// </summary>
    public class ActionLayoutUI : UIBase
    {
        [SerializeField] private Button idleButton;

        [SerializeField] private Button constructionButton;

        /// <summary>
        /// 버튼 클릭 이벤트를 명령 모드 변경 함수에 연결합니다.
        /// </summary>
        private void OnEnable()
        {
            idleButton.onClick.AddListener(OnIdleButtonClicked);
            constructionButton.onClick.AddListener(OnConstructionButtonClicked);
        }

        /// <summary>
        /// 비활성화 시 버튼 클릭 이벤트 연결을 해제합니다.
        /// </summary>
        private void OnDisable()
        {
            idleButton.onClick.RemoveListener(OnIdleButtonClicked);
            constructionButton.onClick.RemoveListener(OnConstructionButtonClicked);
        }

        /// <summary>
        /// 플레이어 명령 모드를 일반 선택/이동 모드로 전환합니다.
        /// </summary>
        public void OnIdleButtonClicked()
        {
            PlayerManager.GetInstance().SetCommandSystemMode(PlayerCommandSystemMode.Idle);
        }

        /// <summary>
        /// 플레이어 명령 모드를 공사 타일 선택 모드로 전환합니다.
        /// </summary>
        public void OnConstructionButtonClicked()
        {
            PlayerManager.GetInstance().SetCommandSystemMode(PlayerCommandSystemMode.Construction);
        }
    }
}
