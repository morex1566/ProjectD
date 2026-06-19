using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    /// <summary>
    /// 플레이어 입력에 따른 명령을 중개
    /// </summary>
    public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
    {
        private static PlayerManagerSettingsData settings;

        /// <summary>
        /// 현재 플레이어 명령 모드입니다.
        /// </summary>
        private PlayerCommandSystemMode currCommandSystemMode = PlayerCommandSystemMode.Idle;

        /// <summary>
        /// 명령 모드별 처리 객체입니다.
        /// </summary>
        private readonly Dictionary<PlayerCommandSystemMode, PlayerCommandSystem> commandSystems = new()
        {
            { PlayerCommandSystemMode.Idle, new IdleSystem() },
            { PlayerCommandSystemMode.Construction, new ConstructionSystem() },
            { PlayerCommandSystemMode.Mining, new MiningSystem() }
        };

        /// <summary>
        /// 현재 활성화된 명령 모드 객체입니다.
        /// </summary>
        private PlayerCommandSystem currCommandSystem = null;


        /// <summary>
        /// 플레이어 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
            settings = Resources.Load<PlayerManagerSettingsData>("SO_PlayerManagerSettings");
        }

        /// <summary>
        /// 우클릭 입력 이벤트를 명령 처리에 연결합니다.
        /// </summary>
        private void OnEnable()
        {
            InputManager.InputMappingContext.Player.RightClick.performed += OnRightClickPerformed;
        }

        /// <summary>
        /// 비활성화 시 입력 이벤트 연결을 해제합니다.
        /// </summary>
        private void OnDisable()
        {
            InputManager.InputMappingContext.Player.RightClick.performed -= OnRightClickPerformed;
        }

        /// <summary>
        /// 선택기와 MiningSystem 프리팹을 생성한 뒤 현재 명령 모드를 적용합니다.
        /// </summary>
        private void Start()
        {
            currCommandSystem = commandSystems[currCommandSystemMode];
        }

        /// <summary>
        /// 현재 명령 모드에 맞는 우클릭 명령을 실행합니다.
        /// </summary>
        private void OnRightClickPerformed(InputAction.CallbackContext context)
        {
            currCommandSystem.HandleRightClickPerformed();
        }

        /// <summary>
        /// 현재 플레이어 명령 모드를 변경하고 대응되는 Selector와 표시 타일을 전환합니다.
        /// </summary>
        public void SetCommandSystemMode(PlayerCommandSystemMode mode)
        {
            currCommandSystemMode = mode;
            currCommandSystem = commandSystems[mode];
        }
    }
}
