using System;
using UnityEngine;

namespace TRPG.Runtime
{
    public enum PlayerCommandSystemType
    {
        Idle,
        Creature,
        Construction,
    }

    [Serializable]
    public class PlayerCommandSystem
    {
        protected PlayerCommandSystemType type;


        public PlayerCommandSystemType Type => type;


        /// <summary>
        /// 해당 명령 상태에 진입할 때 실행합니다.
        /// </summary>
        public virtual void Enter() { }

        /// <summary>
        /// 해당 명령 상태에서 빠져나갈 때 실행합니다.
        /// </summary>
        public virtual void Exit() { }
    }

    [Serializable]
    public class IdleCommandSystem : PlayerCommandSystem
    {
        public IdleCommandSystem()
        {
            type = PlayerCommandSystemType.Idle;
        }

        public override void Enter()
        {
            Debug.Log("IdleCommandSystem Enter");
        }

        public override void Exit()
        {
            Debug.Log("IdleCommandSystem Exit");
        }
    }

    [Serializable]
    public class CreatureCommandSystem : PlayerCommandSystem
    {
        [SerializeField, ReadOnly] private CreatureSelector creatureSelector;


        public CreatureCommandSystem(CreatureSelector creatureSelector)
        {
            type = PlayerCommandSystemType.Creature;
            this.creatureSelector = creatureSelector;
        }

        public override void Enter()
        {
            Debug.Log("CreatureCommandSystem Enter");

            creatureSelector.enabled = true;
        }

        public override void Exit()
        {
            Debug.Log("CreatureCommandSystem Exit");

            creatureSelector.enabled = false;
        }
    }

    public class ConstructionCommandSystem : PlayerCommandSystem
    {
        [SerializeField, ReadOnly] private WorldTileSelector worldTileSelector;


        public ConstructionCommandSystem(WorldTileSelector worldTileSelector)
        {
            type = PlayerCommandSystemType.Construction;
            this.worldTileSelector = worldTileSelector;
        }

        public override void Enter()
        {
            Debug.Log("ConstructionCommandSystem Enter");

            worldTileSelector.enabled = true;
        }

        public override void Exit()
        {
            Debug.Log("ConstructionCommandSystem Exit");

            worldTileSelector.enabled = false;
        }
    }
}
