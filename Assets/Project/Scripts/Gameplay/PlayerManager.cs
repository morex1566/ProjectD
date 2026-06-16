using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public enum CommandEnqueueType
    {
        Replace,
        Append
    }

    public enum CommandMode
    {
        Idle,
        Construction
    }

    /// <summary>
    /// 플레이어 입력에 따른 명령을 중개
    /// </summary>
    public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
    {
        private static PlayerManagerSettingsData settings;

        private CommandEnqueueType commandEnqueueMode = CommandEnqueueType.Replace;

        private CommandMode commandMode = CommandMode.Idle;

        private IdleSelector idleSelector;

        private ConstructionSelector constructionSelector;

        private Stack<PlayerCommand> commands = new();


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
            idleSelector = Instantiate(settings.SelectorPf, transform).GetComponent<IdleSelector>();
            constructionSelector = Instantiate(settings.SelectorPf, transform).GetComponent<ConstructionSelector>();
        }

        private void OnRightClickPerformed(InputAction.CallbackContext context)
        {
            CommandMove();
            CommandConstruct();
        }

        /// <summary>
        /// 선택대상들에게 이동을 명령
        /// </summary>
        /// <param name="mode"></param>
        private void CommandMove(CommandEnqueueType mode = CommandEnqueueType.Replace)
        {
            // 일반 명령 상태가 아니면 이동 불가
            if (commandMode != CommandMode.Idle) return;

            // 선택 대상들에게 마우스의 cellpos로 이동하라고 명령
            IReadOnlyList<ISelectable> selectedInsts = idleSelector.Selecteds;
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(WorldManager.CamController.Cam);
            for (int i = 0; i < selectedInsts.Count; i++)
            {
                // 선택대상들이 크리쳐임?
                if (selectedInsts[i] is not CreatureController creature) continue;

                creature.EnqueueMove(mouseWorldPos, mode);
            }
        }

        private void CommandConstruct(CommandEnqueueType mode = CommandEnqueueType.Replace)
        {
            // 공사 명령 상태가 아니면 공사 불가
            if (commandMode != CommandMode.Construction) return;
        }

        public void SetCommandMode(CommandMode mode)
        {
            commandMode = mode;

            switch (commandMode)
            {
                case CommandMode.Idle:
                    constructionSelector.enabled = false;
                    idleSelector.enabled = true;
                    break;

                case CommandMode.Construction:
                    constructionSelector.enabled = true;
                    idleSelector.enabled = false;
                    break;

                default:
                    break;
            }
        }
    }
}
