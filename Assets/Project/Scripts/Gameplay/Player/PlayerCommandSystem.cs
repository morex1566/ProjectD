using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public enum PlayerCommandSystemType
    {
        Idle,
        Creature,
        Mining,
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

            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == true)
            {
                inputMappingContext.Player.RightClick.performed += OnLeftClickPerformed;
            }

            creatureSelector.enabled = true;
        }

        public override void Exit()
        {
            Debug.Log("CreatureCommandSystem Exit");

            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == true)
            {
                inputMappingContext.Player.RightClick.performed -= OnLeftClickPerformed;
            }

            creatureSelector.enabled = false;
        }

        public void OnLeftClickPerformed(InputAction.CallbackContext context)
        {
            CommandMove();
        }

        public void CommandMove()
        {
            foreach (var selected in creatureSelector.Selecteds)
            {
                // 크리쳐가 아닌 대상?
                CreatureController controller = selected.SelectedInst.GetComponent<CreatureController>();
                if (controller == null)
                {
                    return;
                }

                // 클릭 지점이 길찾기 가능한 좌표?
                Camera cam = WorldManager.GetWorldCameraController()?.Cam;
                Vector3 mouseWorldPosition = MouseEx.GetMouseWorldPosition(cam);
                Vector3Int targetCellPos = WorldManager.WorldToCell(mouseWorldPosition);
                if (AStarPathfinder.AStarGrid.IsInBound(targetCellPos) == false)
                {
                    return;
                }

                // 이미 이전에 있던 명령?
                if (controller.JobQueue.TryFind<CreatureMoveJob>(out CreatureMoveJob prevJob) == true)
                {
                    controller.JobQueue.Remove(prevJob);
                }

                // 새 이동 명령 추가
                controller.JobQueue.Enqueue(new CreatureMoveJob(controller, targetCellPos));
            }
        }
    }

    public class MiningCommandSystem : PlayerCommandSystem
    {
        [SerializeField, ReadOnly] private WorldTileSelector worldTileSelector;


        public MiningCommandSystem(WorldTileSelector worldTileSelector)
        {
            type = PlayerCommandSystemType.Mining;
            this.worldTileSelector = worldTileSelector;
        }

        public override void Enter()
        {
            Debug.Log("MiningCommandSystem Enter");

            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == true)
            {
                inputMappingContext.Player.LeftClick.performed += OnLeftClickPerformed;
            }


            worldTileSelector.enabled = true;
        }

        public override void Exit()
        {
            Debug.Log("MiningCommandSystem Exit");

            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == true)
            {
                inputMappingContext.Player.LeftClick.performed -= OnLeftClickPerformed;
            }


            worldTileSelector.enabled = false;
        }

        public void OnLeftClickPerformed(InputAction.CallbackContext context)
        {
            CommandMining();
        }

        public void CommandMining()
        {

        }
    }
}
