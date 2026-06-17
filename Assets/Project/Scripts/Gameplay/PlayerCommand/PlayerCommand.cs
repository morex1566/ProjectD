using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public abstract class PlayerCommand
    {
        public abstract bool Execute();
    }

    public abstract class ConstructCommand : PlayerCommand
    {

    }

    public sealed class DigOrder : ConstructCommand
    {
        /// <summary>
        /// 이 명령에 참여하고 있는 크리쳐
        /// </summary>
        private readonly List<CreatureController> creatures;

        /// <summary>
        /// DigAction 대상들
        /// </summary>
        private readonly List<Vector3Int> cells;

        /// <summary>
        /// 이 명령이 creature에게 어떻게 해석되는지?
        /// </summary>
        private readonly CommandEnqueueType type;

        public DigOrder(List<CreatureController> creatures, IReadOnlyList<Vector3Int> cells, CommandEnqueueType type)
        {
            this.creatures = creatures;
            this.cells = new List<Vector3Int>(cells);
            this.type = type;
        }

        public override bool Execute()
        {
            if (creatures == null) return false;

            if (cells.Count == 0) return false;

            // creatures.EnqueueDig(cells, type);
            return true;
        }
    }
}
