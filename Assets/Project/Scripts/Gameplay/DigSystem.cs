using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 한 타일에 대한 작업단위
    /// </summary>
    public class DigAction
    {
        public const float MaxProgress = 100f;

        /// <summary>
        /// 지금 작업에 대한 진행도, 100이 되면 이 작업이 종료
        /// </summary>
        public float CurrProgress = 0;

        /// <summary>
        /// 이 작업에서 몇 명이 일하고 있음?
        /// </summary>
        public int WorkerCount = 0;

        /// <summary>
        /// 이 작업의 위치
        /// </summary>
        public Vector3Int CellPos = Vector3Int.zero;

        public Action OnCompleted;

        public bool IsCompleted => CurrProgress >= MaxProgress;

        public void AddProgress(float amount)
        {
            CurrProgress += amount;
        }
    }

    /// <summary>
    /// 현재 땅파기 명령이 수행된 타일의 포지션을 저장
    /// </summary>
    [Serializable]
    public class DigSystem : MonoBehaviour
    {
        [SerializeField] public TileBase DigTile = null;

        [SerializeField, ReadOnly] public Queue<DigAction> Actions = new();



        public void AddDigActions(IReadOnlyList<Vector3Int> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                DigAction job = new DigAction
                {
                    CellPos = cells[i]
                };

                Actions.Enqueue(job);
                WorldManager.Map.Selection.SetTile(cells[i], DigTile);
            }
        }

        public bool TryGetNextAction(out DigAction action)
        {
            while (Actions.Count > 0)
            {
                action = Actions.Peek();

                if (!action.IsCompleted)
                {
                    return true;
                }

                Actions.Dequeue();
            }

            action = null;
            return false;
        }

        public void CompleteAction(DigAction action)
        {
            if (action == null) return;

            action.OnCompleted?.Invoke();

            WorldManager.Map.RemoveTile(action.CellPos);
            WorldManager.Map.Selection.SetTile(action.CellPos, null);

            if (Actions.Count > 0 && Actions.Peek() == action)
            {
                Actions.Dequeue();
            }
        }
    }
}
