using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TRPG.Runtime
{
    /// <summary>
    /// 플레이어 입력에 따른 명령을 중개
    /// </summary>
    public class PlayerManager : MonoBehaviourSingleton<PlayerManager>, IDisposable
    {
        public static PlayerManagerSettingsData Settings;

        private bool isDisposed = false;

        [SerializeField, ReadOnly] private CreatureSelector creatureSelector;

        [SerializeField, ReadOnly] private WorldTileSelector worldTileSelector;

        [SerializeField, ReadOnly] private Dictionary<PlayerCommandSystemType, PlayerCommandSystem> commandSystems = new();

        [SerializeField, ReadOnly] private PlayerCommandSystem currentCommandSystem;


        public UnityEvent<PlayerCommandSystemType> CommandSystemModeChanged;


        /// <summary>
        /// 플레이어 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            PlayerManager manager = GetInstance();
            manager.isDisposed = false;

            Settings = ResourceManager.GetResource<PlayerManagerSettingsData>(UnityConstant.Addressable.Label.Core);

            manager.currentCommandSystem?.Exit();
            manager.DisposeSelector();
            GameObject selectorInst = Instantiate(Settings.SelectorPrefab, manager.transform);
            manager.creatureSelector = selectorInst.GetComponent<CreatureSelector>();
            manager.worldTileSelector = selectorInst.GetComponent<WorldTileSelector>();

            // 명령 시스템 초기화/등록
            manager.currentCommandSystem = new IdleCommandSystem();
            manager.commandSystems.Clear();
            manager.commandSystems.Add(PlayerCommandSystemType.Idle, manager.currentCommandSystem);
            manager.commandSystems.Add(PlayerCommandSystemType.Creature, new CreatureCommandSystem(manager.creatureSelector));
            manager.commandSystems.Add(PlayerCommandSystemType.Mining, new MiningCommandSystem(manager.worldTileSelector));
            manager.currentCommandSystem.Enter();
        }

        public static void SetCommandSystemType(PlayerCommandSystemType type)
        {
            PlayerManager manager = GetInstance();

            if (manager.currentCommandSystem.Type == type)
            {
                return;
            }

            if (manager.commandSystems.TryGetValue(type, out PlayerCommandSystem nextSystem) == false)
            {
                return;
            }

            // 교체 시작
            manager.currentCommandSystem?.Exit();
            manager.currentCommandSystem = nextSystem;
            manager.currentCommandSystem?.Enter();
            manager.CommandSystemModeChanged?.Invoke(type);
        }

        /// <summary>
        /// 플레이어 입력 명령과 선택기 인스턴스를 정리합니다.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed == true)
            {
                return;
            }

            isDisposed = true;

            currentCommandSystem?.Exit();
            currentCommandSystem = null;
            commandSystems.Clear();
            CommandSystemModeChanged?.RemoveAllListeners();
            DisposeSelector();
            Settings = null;
        }

        protected override void OnDestroy()
        {
            Dispose();
            base.OnDestroy();
        }

        /// <summary>
        /// 현재 생성된 선택기 GameObject를 제거합니다.
        /// </summary>
        private void DisposeSelector()
        {
            GameObject selectorObj = creatureSelector != null ? creatureSelector.gameObject : worldTileSelector?.gameObject;

            if (selectorObj != null)
            {
                UnityEngine.Object.Destroy(selectorObj);
            }

            creatureSelector = null;
            worldTileSelector = null;
        }
    }
}
