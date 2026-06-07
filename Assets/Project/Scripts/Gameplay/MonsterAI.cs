using UnityEngine;
using System.Collections.Generic;

namespace TRPG.Runtime
{
    public readonly struct AIMove
    {
        public readonly CreatureController Actor;
        public readonly Vector3Int From;
        public readonly Vector3Int To;
        public readonly CreatureController Target;

        public bool IsAttack => Target != null;

        public AIMove(CreatureController actor, Vector3Int from, Vector3Int to, CreatureController target)
        {
            Actor = actor;
            From = from;
            To = to;
            Target = target;
        }
    }


    public class MonsterAI
    {
        /// <summary>
        /// 현재 월드 상태에서 몬스터가 실행할 최선의 한 수를 결정합니다.
        /// </summary>
        public bool TryGetBestMove(IReadOnlyDictionary<int, CreatureController> creatures, out AIMove move)
        {
            move = default;

            if (creatures == null || !TryGetPlayer(creatures, out PlayerController player)) return false;

            bool hasMove = false;
            int bestScore = int.MinValue;
            foreach (KeyValuePair<int, CreatureController> pair in creatures)
            {
                if (pair.Value is not MonsterController monster) continue;

                if (!monster.CanExecuteAIMove) continue;

                if (TryGetBestMove(monster, player, creatures, out AIMove candidateMove, out int candidateScore) &&
                    (!hasMove || candidateScore > bestScore || (candidateScore == bestScore && IsEarlierMove(candidateMove, move))))
                {
                    move = candidateMove;
                    bestScore = candidateScore;
                    hasMove = true;
                }
            }

            return hasMove;
        }

        /// <summary>
        /// 지정 몬스터가 현재 월드 상태에서 실행할 최선의 한 수를 결정합니다.
        /// </summary>
        public bool TryGetBestMove(MonsterController monster, IReadOnlyDictionary<int, CreatureController> creatures, out AIMove move)
        {
            move = default;

            if (monster == null || creatures == null) return false;

            if (!monster.CanExecuteAIMove) return false;

            if (!TryGetPlayer(creatures, out PlayerController player)) return false;

            return TryGetBestMove(monster, player, creatures, out move, out _);
        }

        private bool TryGetBestMove(
            MonsterController monster,
            PlayerController player,
            IReadOnlyDictionary<int, CreatureController> creatures,
            out AIMove move,
            out int score)
        {
            move = default;
            score = int.MinValue;

            bool hasMove = false;
            foreach (AIMove candidateMove in GetLegalMoves(monster, player, creatures))
            {
                int candidateScore = GetMoveScore(candidateMove, player);
                if (!hasMove || candidateScore > score || (candidateScore == score && IsEarlierMove(candidateMove, move)))
                {
                    move = candidateMove;
                    score = candidateScore;
                    hasMove = true;
                }
            }

            return hasMove;
        }

        private List<AIMove> GetLegalMoves(
            MonsterController monster,
            PlayerController player,
            IReadOnlyDictionary<int, CreatureController> creatures)
        {
            List<AIMove> moves = new();

            if (monster.Model.Directions == null) return moves;

            if (IsPawn(monster))
            {
                AddPawnMoves(monster, player, creatures, moves);
                return moves;
            }

            foreach (Vector3Int direction in GetMoveDirections(monster, player))
            {
                if (direction == Vector3Int.zero) continue;

                Vector3Int cellPos = monster.Model.CellPos + direction;
                while (WorldManager.TryGetMapWorldPos(cellPos, out _))
                {
                    CreatureController occupant = GetCreatureInCellPos(creatures, cellPos);
                    if (occupant == null)
                    {
                        moves.Add(new AIMove(monster, monster.Model.CellPos, cellPos, null));
                    }
                    else
                    {
                        if (occupant == player)
                        {
                            moves.Add(new AIMove(monster, monster.Model.CellPos, cellPos, occupant));
                        }

                        break;
                    }

                    if (!monster.Model.IsMoveRepeatable) break;

                    cellPos += direction;
                }
            }

            return moves;
        }

        private void AddPawnMoves(
            MonsterController monster,
            PlayerController player,
            IReadOnlyDictionary<int, CreatureController> creatures,
            List<AIMove> moves)
        {
            int yDirection = GetPawnYDirection(monster, player);

            // 폰은 전방 1칸이 비어 있을 때만 전진합니다.
            Vector3Int forwardCellPos = monster.Model.CellPos + new Vector3Int(0, yDirection, 0);
            if (WorldManager.TryGetMapWorldPos(forwardCellPos, out _) &&
                GetCreatureInCellPos(creatures, forwardCellPos) == null)
            {
                moves.Add(new AIMove(monster, monster.Model.CellPos, forwardCellPos, null));
            }

            // 폰은 전방 대각선 1칸에 플레이어가 있을 때만 공격합니다.
            AddPawnAttackMove(monster, player, creatures, moves, new Vector3Int(-1, yDirection, 0));
            AddPawnAttackMove(monster, player, creatures, moves, new Vector3Int(1, yDirection, 0));
        }

        private void AddPawnAttackMove(
            MonsterController monster,
            PlayerController player,
            IReadOnlyDictionary<int, CreatureController> creatures,
            List<AIMove> moves,
            Vector3Int direction)
        {
            Vector3Int attackCellPos = monster.Model.CellPos + direction;
            if (!WorldManager.TryGetMapWorldPos(attackCellPos, out _)) return;

            CreatureController occupant = GetCreatureInCellPos(creatures, attackCellPos);
            if (occupant != player) return;

            moves.Add(new AIMove(monster, monster.Model.CellPos, attackCellPos, occupant));
        }

        private int GetMoveScore(AIMove move, PlayerController player)
        {
            if (move.IsAttack) return 100000;

            // 플레이어와의 맨해튼 거리가 가까워지는 수를 우선합니다.
            return -GetDistance(move.To, player.Model.CellPos);
        }

        private List<Vector3Int> GetMoveDirections(MonsterController monster, PlayerController player)
        {
            if (IsPawn(monster))
            {
                return new List<Vector3Int> { new Vector3Int(0, GetPawnYDirection(monster, player), 0) };
            }

            return monster.Model.Directions;
        }

        private bool IsPawn(MonsterController monster)
        {
            return monster.Model.Data != null && monster.Model.Data.CreatureType == CreatureType.Pawn;
        }

        private int GetPawnYDirection(MonsterController monster, PlayerController player)
        {
            return player.Model.CellPos.y >= monster.Model.CellPos.y ? 1 : -1;
        }

        private bool TryGetPlayer(IReadOnlyDictionary<int, CreatureController> creatures, out PlayerController player)
        {
            foreach (KeyValuePair<int, CreatureController> pair in creatures)
            {
                if (pair.Value is not PlayerController candidate) continue;

                player = candidate;
                return true;
            }

            player = null;
            return false;
        }

        private CreatureController GetCreatureInCellPos(IReadOnlyDictionary<int, CreatureController> creatures, Vector3Int cellPos)
        {
            foreach (KeyValuePair<int, CreatureController> pair in creatures)
            {
                if (pair.Value == null) continue;

                if (pair.Value.Model.CellPos != cellPos) continue;

                return pair.Value;
            }

            return null;
        }

        private bool IsEarlierMove(AIMove candidate, AIMove current)
        {
            if (current.Actor == null) return true;

            int actorCompare = candidate.Actor.GetInstanceID().CompareTo(current.Actor.GetInstanceID());
            if (actorCompare != 0) return actorCompare < 0;

            if (candidate.To.x != current.To.x) return candidate.To.x < current.To.x;
            if (candidate.To.y != current.To.y) return candidate.To.y < current.To.y;

            return candidate.To.z < current.To.z;
        }

        private int GetDistance(Vector3Int lhs, Vector3Int rhs)
        {
            return Mathf.Abs(lhs.x - rhs.x) + Mathf.Abs(lhs.y - rhs.y);
        }
    }
}
