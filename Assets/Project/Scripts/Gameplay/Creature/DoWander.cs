using MBT;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - DoWander")]
    public class DoWander : Leaf
    {
        private const float ArriveDistance = 0.01f;

        [SerializeField, ReadOnly] private CreatureController controller = null;
        [SerializeField, ReadOnly] private Vector3Int targetCellPosition = Vector3Int.zero;
        [SerializeField] private int wanderRadius = 4;
        [SerializeField, Min(2)] private float retryIntervalSec = 5f;

        private List<Vector3> currentPathWorldPositions = new();

        private float nextPickTime = 0f;

        private int pathIndex = 1;

        private void OnValidate()
        {
            CacheComponents();
        }

        private void Awake()
        {
            CacheComponents();
            InitRuntimeState();
        }

        public override NodeResult Execute()
        {
            // 이미 이동 Job이 있으면 새 Move 목적지를 만들지 않습니다.
            if (currentPathWorldPositions != null && currentPathWorldPositions.Count > 1 && pathIndex < currentPathWorldPositions.Count)
            {
                Move();
                return NodeResult.running;
            }

            // 아직 뽑을 때 안됨
            if (Time.time < nextPickTime)
            {
                return NodeResult.running;
            }

            // 새로 뽑기
            if (TryPickReachableCell() == false)
            {
                return NodeResult.failure;
            }

            return NodeResult.running;
        }

        private bool TryPickReachableCell()
        {
            // 대상의 좌우 노드만 선택합니다.
            List<Vector3Int> candidateCellPositions = new();
            Vector3Int currentCellPositions = WorldManager.WorldToCell(controller.transform.position);
            for (int x = -wanderRadius; x <= wanderRadius; x++)
            {
                if (x == 0)
                {
                    continue;
                }

                Vector3Int candidateCellPos = currentCellPositions + new Vector3Int(x, 0, 0);

                // AI 타입별 선택 조건
                switch (controller.Context.AIType)
                {
                    case CreatureAIType.Ground:
                        if (IsGroundReachableCell(candidateCellPos) == false)
                        {
                            continue;
                        }

                        break;
                    case CreatureAIType.Air:
                        break;
                    default:
                        break;
                }

                candidateCellPositions.Add(candidateCellPos);
            }

            // 후보 셀 순서를 섞어서 매번 같은 방향으로만 가지 않게 합니다.
            for (int i = 0; i < candidateCellPositions.Count; i++)
            {
                int randomIndex = Random.Range(i, candidateCellPositions.Count);

                Vector3Int temp = candidateCellPositions[i];
                candidateCellPositions[i] = candidateCellPositions[randomIndex];
                candidateCellPositions[randomIndex] = temp;
            }

            // 후보가 있으면?
            if (candidateCellPositions.Count > 0)
            {
                // 이제 후보군에서 길찾기ㄱㄱ
                // + 선택 확정
                targetCellPosition = candidateCellPositions[0];
                List<AStarNode> path = AStarPathfinder.FindPath(currentCellPositions, targetCellPosition);
                SetCurrentPath(path);
                nextPickTime = Time.time + Random.Range(retryIntervalSec - 2, retryIntervalSec + 2);

                return true;
            }

            // 후보가 없으면
            return false;
        }

        private void SetCurrentPath(IReadOnlyList<AStarNode> path)
        {
            pathIndex = 1;
            currentPathWorldPositions.Clear();

            WorldGridController gridController = WorldManager.GetWorldGridController();
            if (gridController == null || gridController.Grid == null)
            {
                return;
            }

            for (int i = 0; i < path.Count; i++)
            {
                AStarNode node = path[i];
                Vector3Int cellPos = new Vector3Int(node.X, node.Y, 0);
                Vector3 wayPoint = gridController.Grid.GetCellCenterWorld(cellPos) + AStarPathfinder.RandomOffset;

                currentPathWorldPositions.Add(wayPoint);
            }
        }

        private bool IsGroundReachableCell(Vector3Int cellPos)
        {
            // 목적지 칸은 이동 가능한 빈 칸이어야 합니다.
            if (AStarPathfinder.AStarGrid.TryGetNode(cellPos.x, cellPos.y, out _) == false)
            {
                return false;
            }

            // 그라운드가 있어야함
            WorldTilemapController groundTilemap = WorldManager.GetWorldTilemapController(WorldTilemapType.WorldTilemapGround);
            if (groundTilemap == null)
            {
                return false;
            }

            // 바로 아래에 Ground 타일이 있어야 "땅 위"로 인정합니다.
            Vector3Int belowCellPos = cellPos + Vector3Int.down;
            return groundTilemap.TryGetTile(belowCellPos.x, belowCellPos.y, out _);
        }

        /// <summary>
        /// 경로를 따라 이동
        /// </summary>
        private void Move()
        {
            // 이미 도착한거임?
            if (pathIndex >= currentPathWorldPositions.Count)
            {
                return;
            }

            // 현재 목적지는 경로를 선택할 때 미리 계산해 둔 월드 좌표를 사용합니다.
            Vector3 targetWorldPosition = currentPathWorldPositions[pathIndex];

            // X축만 이동합니다.
            Vector3 currentWorldPosition = controller.transform.position;
            float nextX = Mathf.MoveTowards(currentWorldPosition.x, targetWorldPosition.x, controller.Context.MoveSpeed * Time.deltaTime);
            controller.transform.position = new Vector3(nextX, currentWorldPosition.y, currentWorldPosition.z);

            // 이동 후 검사, 현재 노드에 도착하면 다음 노드로 넘어갑니다.
            if (Mathf.Abs(controller.transform.position.x - targetWorldPosition.x) <= ArriveDistance)
            {
                controller.transform.position = new Vector3(targetWorldPosition.x, currentWorldPosition.y, currentWorldPosition.z);
                pathIndex++;
            }

            return;
        }

        public override void DrawGizmos()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            WorldGridController gridController = WorldManager.GetWorldGridController();
            if (gridController == null || gridController.Grid == null)
            {
                return;
            }

            Gizmos.DrawWireCube(gridController.Grid.GetCellCenterWorld(targetCellPosition), Vector3.one * 0.25f);
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }

        private void InitRuntimeState()
        {
            nextPickTime = Time.time + Random.Range(retryIntervalSec - 2, retryIntervalSec + 2);
        }
    }
}
