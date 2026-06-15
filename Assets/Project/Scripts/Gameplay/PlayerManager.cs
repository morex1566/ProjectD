using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public enum CommandQueueMode
    {
        Replace,
        Append
    }

    /// <summary>
    /// 플레이어 입력에 따른 명령을 중개
    /// </summary>
    public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
    {
        private static PlayerManagerSettingsData settings;

        private Selector selector;

        /// <summary>
        /// 플레이어 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
            settings = Resources.Load<PlayerManagerSettingsData>("SO_PlayerManagerSettings");
        }

        private void OnEnable()
        {
            InputManager.InputMappingContext.Player.RightClick.performed += OnRightClickPerformed;
        }

        private void OnDisable()
        {
            InputManager.InputMappingContext.Player.RightClick.performed -= OnRightClickPerformed;
        }

        private void Start()
        {
            selector = Instantiate(settings.Selector);
        }

        private void OnRightClickPerformed(InputAction.CallbackContext context)
        {
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(WorldManager.CamController.Cam);
            CommandMove(mouseWorldPos, CommandQueueMode.Replace);
        }

        private void CommandMove(Vector3 targetPos, CommandQueueMode mode)
        {
            IReadOnlyList<ISelectable> selectedInsts = selector.SelectedInsts;

            for (int i = 0; i < selectedInsts.Count; i++)
            {
                if (selectedInsts[i] is not CreatureController creature) continue;

                if (creature.Owner != gameObject) continue;

                creature.EnqueueMove(targetPos, mode);
            }
        }

        public static void SetSelectorSelectionMode(SelectionMode mode)
        {
            Selector selector = GetInstance().selector;
            selector.Mode = mode;
        }
    }
}
