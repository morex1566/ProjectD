using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    public class WorldTestConsole : MonoBehaviour
    {
        [SerializeField] private bool isCastle = false;

        [SerializeField] private bool isForest = false;

        [SerializeField] private bool isFarm = false;


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
            if (WorldManager.TryGetMouseCellPosition(out Vector3Int cellPosition) == false)
            {
                return;
            }

            if (isCastle == true)
            {
                SetCastleTIle(cellPosition);
                return;
            }

            if (isForest == true)
            {
                SetForestTile(cellPosition);
                return;
            }

            if (isFarm == true)
            {
                SetFarmTile(cellPosition);
                return;
            }
        }

        /// <summary>
        /// WorldManager에 저장된 월드 타일을 배치합니다.
        /// </summary>
        public void SetCastleTIle(Vector3Int cellPosition)
        {
            WorldTile tile = new WorldTile()
            {
                CellPosition = cellPosition,
                Flag = WorldTileFlag.Building
            };

            WorldManager.SetTile(tile);
        }

        public void SetForestTile(Vector3Int cellPosition)
        {
            WorldTile tile = new WorldTile()
            {
                CellPosition = cellPosition,
                Flag = WorldTileFlag.Enviroment
            };

            WorldManager.SetTile(tile);
        }

        public void SetFarmTile(Vector3Int cellPosition)
        {
            WorldTile tile = new WorldTile()
            {
                CellPosition = cellPosition,
                Flag = WorldTileFlag.Building
            };

            WorldManager.SetTile(tile);
        }
    }
}

//#if UNITY_EDITOR

//namespace TRPG.Editor
//{
//    using NUnit.Framework.Internal;
//    using TRPG.Runtime;
//    using UnityEditor;
//    using UnityEngine;

//    [CustomEditor(typeof(WorldTestConsole))]
//    public class WorldTestConsoleEditor : UnityEditor.Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            DrawDefaultInspector();
//            EditorGUILayout.Space();

//            if (GUILayout.Button("castle"))
//            {
//                WorldTestConsole test = (WorldTestConsole)target;
//                test.SetCastleTIle();
//            }

//            if (GUILayout.Button("forest"))
//            {
//                WorldTestConsole test = (WorldTestConsole)target;
//                test.SetCastleTIle();
//            }

//            if (GUILayout.Button("farm"))
//            {
//                WorldTestConsole test = (WorldTestConsole)target;
//                test.SetCastleTIle();
//            }
//        }
//    }
//}

//#endif