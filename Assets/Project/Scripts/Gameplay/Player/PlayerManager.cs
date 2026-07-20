using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 플레이어 시스템의 생명주기 진입점입니다.
    /// </summary>
    public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
    {
        // public static readonly
        public static PlayerManagerSettingsData Settings { get; private set; }

        [SerializeField, ReadOnly] private CreatureSelector creatureSelector = null;

        [SerializeField, ReadOnly] private WorldTileSelector worldTileSelector = null;

        private Dictionary<PlayerCommandSystemType, PlayerCommandSystem> systems = new();

        private PlayerCommandSystem currentCommandSystem = null;

        /// <summary>
        /// 플레이어 매니저와 기본 Idle 명령 시스템을 준비합니다.
        /// </summary>
        public static void Init()
        {
            PlayerManager manager = GetInstance();

            Settings = ResourceManager.GetResource<PlayerManagerSettingsData>(UnityConstant.Addressable.Label.Core);
        }

        public void Start()
        {
            GameObject selectorInstance = Instantiate(Settings.SelectorPrefab);
            {
                creatureSelector = selectorInstance.GetComponent<CreatureSelector>();
                worldTileSelector = selectorInstance.GetComponent<WorldTileSelector>();
            }

            // system selection
            systems.Add(PlayerCommandSystemType.Idle, new IdleCommandSystem(creatureSelector));
            SetCommandSystem(PlayerCommandSystemType.Idle);
        }

        public static void SetCommandSystem(PlayerCommandSystemType systemType)
        {
            PlayerManager manager = GetInstance();

            manager.currentCommandSystem?.Exit();
            manager.currentCommandSystem = manager.systems[systemType];
            manager.currentCommandSystem.Enter();
        }
    }
}
