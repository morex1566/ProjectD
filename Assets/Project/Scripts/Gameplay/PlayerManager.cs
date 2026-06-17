using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

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

        private CommandMode commandMode = CommandMode.Idle;

        private IdleSelector idleSelector;

        private ConstructionSelector constructionSelector;

        private DigSystem digSystem;

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
            // selector
            GameObject selectorObj = Instantiate(settings.SelectorPf, transform);
            idleSelector = selectorObj.GetComponent<IdleSelector>();
            constructionSelector = selectorObj.GetComponent<ConstructionSelector>();

            // digSystem
            digSystem = Instantiate(settings.DigSystemPf, transform).GetComponent<DigSystem>();

            // command mode
            SetCommandMode(commandMode);
        }

        private void OnRightClickPerformed(InputAction.CallbackContext context)
        {
            CommandMove();
            CommandConstruct();
        }

        /// <summary>
        /// 선택대상들에게 이동을 명령
        /// </summary>
        /// <param name="enqueueType"></param>
        private void CommandMove(CommandEnqueueType enqueueType = CommandEnqueueType.Replace)
        {
            // 일반 명령 상태가 아니면 이동 불가
            if (commandMode != CommandMode.Idle) return;
            if (idleSelector == null) return;

            // 선택 대상들에게 마우스의 cellpos로 이동하라고 명령
            IReadOnlyList<ISelectable> selectedInsts = idleSelector.Selecteds;
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(WorldManager.CamController.Cam);
            for (int i = 0; i < selectedInsts.Count; i++)
            {
                // 선택대상들이 크리쳐임?
                if (selectedInsts[i] is not CreatureController creature) continue;

                creature.EnqueueMove(mouseWorldPos, enqueueType);
            }
        }

        private void CommandConstruct(CommandEnqueueType enqueueType = CommandEnqueueType.Replace)
        {
            // 공사 명령 상태가 아니면 공사 불가
            if (commandMode != CommandMode.Construction) return;

            // 공사 대상이 아무것도 없으면 공사 불가
            if (constructionSelector.Selecteds.Count <= 0 ) return;

            // 가장 가까운 대상이 Construct 시작?
            {
                CreatureController target = FindNearestCreature(constructionSelector.Selecteds);


                // 건설 지점을 추가
                digSystem.AddDigActions(constructionSelector.Selecteds);

                // 건설 명령
                target.EnqueueConstruct(digSystem, enqueueType);
            }
        }

        public void SetCommandMode(CommandMode mode)
        {
            commandMode = mode;

            switch (commandMode)
            {
                case CommandMode.Idle:
                    constructionSelector.enabled = false;
                    idleSelector.enabled = true;
                    RenderDigPoint(null);
                    break;

                case CommandMode.Construction:
                    constructionSelector.enabled = true;
                    idleSelector.enabled = false;
                    RenderDigPoint(digSystem.DigTile);
                    break;

                default:
                    break;
            }
        }

        private void RenderDigPoint(TileBase indicator)
        {
            foreach (var digAction in digSystem.Actions)
            {
                WorldManager.Map.Selection.SetTile(digAction.CellPos, null);
            }
        }

        private CreatureController FindNearestCreature(IReadOnlyList<Vector3Int> targetCells)
        {
            if (targetCells == null || targetCells.Count == 0) return null;

            Vector3 pivot = Vector3.zero;
            for (int i = 0; i < targetCells.Count; i++)
            {
                pivot += WorldManager.Map.Ground.GetCellCenterWorld(targetCells[i]);
            }

            pivot /= targetCells.Count;

            Tilemap ground = WorldManager.Map.Ground;
            Vector3Int targetPos = ground.WorldToCell(pivot);

            int nearest = int.MaxValue;
            CreatureController target = null;

            foreach (var creature in WorldManager.Creatures)
            {
                Vector3Int startPos = ground.WorldToCell(creature.Value.transform.position);
                List<AStarNode> path = AStarPathfinder.FindPath(startPos, targetPos);

                if (path == null || path.Count == 0) continue;

                if (path.Count < nearest)
                {
                    nearest = path.Count;
                    target = creature.Value;
                }
            }

            return target;
        }
    }
}
