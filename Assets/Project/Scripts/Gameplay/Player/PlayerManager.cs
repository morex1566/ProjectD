using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 플레이어 입력에 따른 명령을 중개
    /// </summary>
    public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
    {
        public static PlayerManagerSettingsData Settings;

        private static PlayerCommandSystemType commandSystemType = PlayerCommandSystemType.None;

        private static CreatureSelector creatureSelector;

        private static WorldTileSelector worldTileSelector;

        public event Action<PlayerCommandSystemType> CommandSystemModeChanged;

        /// <summary>
        /// 플레이어 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
            Settings = ResourceManager.GetResource<PlayerManagerSettingsData>(UnityConstant.Addressable.Label.Core);

            GameObject selectorInst = Instantiate(Settings.SelectorPf, instance.transform);
            creatureSelector = selectorInst.GetComponent<CreatureSelector>();
            worldTileSelector = selectorInst.GetComponent<WorldTileSelector>();
        }

        public void SetCommandSystemType(PlayerCommandSystemType type)
        {
            commandSystemType = type;

            if (creatureSelector != null) creatureSelector.enabled = type == PlayerCommandSystemType.Idle;

            if (worldTileSelector != null) worldTileSelector.enabled = type == PlayerCommandSystemType.Construction;

            CommandSystemModeChanged?.Invoke(type);
        }
    }
}
