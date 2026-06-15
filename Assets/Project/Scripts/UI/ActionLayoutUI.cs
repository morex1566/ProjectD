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
            PlayerManager.SetSelectorSelectionMode(SelectionMode.Object);
        }

        public void OnConstructionButtonClicked()
        {
            PlayerManager.SetSelectorSelectionMode(SelectionMode.Construction);
        }
    }
}
