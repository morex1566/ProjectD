using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public class WorldTestConsole : MonoBehaviour
    {
        [SerializeField] private WorldTileType selectedTileType = WorldTileType.None;

        private void OnEnable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext context) == true)
            {
                context.Player.LeftClick.performed += OnLeftClickPerformed;
            }
        }

        private void OnDisable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext context) == true)
            {
                context.Player.LeftClick.performed -= OnLeftClickPerformed;
            }
        }

        private void OnLeftClickPerformed(InputAction.CallbackContext context)
        {
            if (selectedTileType == WorldTileType.None)
            {
                return;
            }

            if (WorldManager.TryGetMouseCellPosition(out Vector3Int cellPosition) == false)
            {
                return;
            }

            WorldTile worldTile = new WorldTile()
            {
                CellPosition = cellPosition,
                Type = selectedTileType,
                Flag = GetTileFlag(selectedTileType)
            };

            WorldManager.SetTile(worldTile);
        }

        /// <summary>
        /// 타일 타입에 필요한 게임 로직 플래그를 반환합니다.
        /// </summary>
        private WorldTileFlag GetTileFlag(WorldTileType type)
        {
            switch (type)
            {
                case WorldTileType.Gate:
                    return WorldTileFlag.Gate | WorldTileFlag.Road;

                case WorldTileType.Road:
                    return WorldTileFlag.Road;

                case WorldTileType.Castle:
                    return WorldTileFlag.Building;

                case WorldTileType.Forest:
                    return WorldTileFlag.Environment;

                case WorldTileType.Farm:
                    return WorldTileFlag.Building | WorldTileFlag.Spawnable;

                default:
                    return WorldTileFlag.None;
            }
        }
    }
}