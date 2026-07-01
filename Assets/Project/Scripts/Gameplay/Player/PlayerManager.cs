using System;
using UnityEngine;
using UnityEngine.Events;

namespace TRPG.Runtime
{
    /// <summary>
    /// 플레이어 입력에 따른 명령을 중개
    /// </summary>
    public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
    {
        public static PlayerManagerSettingsData Settings;

        [SerializeField, ReadOnly] private PlayerCommandSystemType commandSystemType = PlayerCommandSystemType.None;

        [SerializeField, ReadOnly] private CreatureSelector creatureSelector;

        [SerializeField, ReadOnly] private WorldTileSelector worldTileSelector;



        [SerializeField, ReadOnly] public UnityEvent<PlayerCommandSystemType> CommandSystemModeChanged;



        /// <summary>
        /// 플레이어 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            PlayerManager manager = GetInstance();

            Settings = ResourceManager.GetResource<PlayerManagerSettingsData>(UnityConstant.Addressable.Label.Core);

            GameObject selectorInst = Instantiate(Settings.SelectorPf, manager.transform);
            manager.creatureSelector = selectorInst.GetComponent<CreatureSelector>();
            manager.worldTileSelector = selectorInst.GetComponent<WorldTileSelector>();
        }

        public static void SetCommandSystemType(PlayerCommandSystemType type)
        {
            PlayerManager manager = GetInstance();

            manager.commandSystemType = type;

            if (manager.creatureSelector != null) manager.creatureSelector.enabled = type == PlayerCommandSystemType.Idle;

            if (manager.worldTileSelector != null) manager.worldTileSelector.enabled = type == PlayerCommandSystemType.Construction;

            manager.CommandSystemModeChanged?.Invoke(type);
        }
    }
}
