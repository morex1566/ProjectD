using UnityEngine;
using UnityEngine.UI;

namespace TRPG.Runtime
{
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

        public void OnIdleButtonClicked()
        {
            PlayerManager.GetInstance().SetCommandMode(CommandMode.Idle);
        }

        public void OnConstructionButtonClicked()
        {
            PlayerManager.GetInstance().SetCommandMode(CommandMode.Construction);
        }
    }
}
