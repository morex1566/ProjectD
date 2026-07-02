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



        private void OnEnable()
        {
            idleButton.onClick.AddListener(OnIdleButtonClicked);
            constructionButton.onClick.AddListener(OnConstructionButtonClicked);
        }

        private void OnDisable()
        {
            idleButton.onClick.RemoveListener(OnIdleButtonClicked);
            constructionButton.onClick.RemoveListener(OnConstructionButtonClicked);
        }


        private void OnIdleButtonClicked()
        {
            PlayerManager.SetCommandSystemType(PlayerCommandSystemType.Creature);
        }

        private void OnConstructionButtonClicked()
        {
            PlayerManager.SetCommandSystemType(PlayerCommandSystemType.Construction);
        }
    }
}
