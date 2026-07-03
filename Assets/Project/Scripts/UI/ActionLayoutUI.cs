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

        [SerializeField] private Button creatureButton;

        [SerializeField] private Button miningButton;



        private void OnEnable()
        {
            idleButton.onClick.AddListener(OnIdleButtonClicked);
            creatureButton.onClick.AddListener (OnCreatureButtonClicked);
            miningButton.onClick.AddListener(OnMiningButtonClicked);
        }

        private void OnDisable()
        {
            idleButton.onClick.RemoveListener(OnIdleButtonClicked);
            creatureButton.onClick.RemoveListener(OnCreatureButtonClicked);
            miningButton.onClick.RemoveListener(OnMiningButtonClicked);
        }


        private void OnIdleButtonClicked()
        {
            PlayerManager.SetCommandSystemType(PlayerCommandSystemType.Idle);
        }

        private void OnCreatureButtonClicked()
        {
            PlayerManager.SetCommandSystemType(PlayerCommandSystemType.Creature);
        }

        private void OnMiningButtonClicked()
        {
            PlayerManager.SetCommandSystemType(PlayerCommandSystemType.Mining);
        }
    }
}
