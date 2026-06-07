using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TRPG.Runtime
{
    public enum CombatTurnState
    {
        None,
        Player,
        Enemy,
        Ended
    }

    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        [Header(nameof(WorldManager) + ".Turn")]

        [SerializeField, ReadOnly] private CombatTurnState combatTurnState = CombatTurnState.None;

        [SerializeField, ReadOnly] private TurnStateUI turnStateUI = null;

        [SerializeField, ReadOnly] private TitleUI gameOverRestartUI = null;

        private Coroutine turnCoroutine = null;

        private bool isRestartingTutorial = false;


        public static CombatTurnState CurrentTurnState => GetInstance().combatTurnState;

        public static bool IsCombatActive => CurrentTurnState == CombatTurnState.Player ||
                                            CurrentTurnState == CombatTurnState.Enemy;

        public static bool IsPlayerTurn => CurrentTurnState == CombatTurnState.Player;

        public static bool CanPlayerAct => !IsCombatActive || IsPlayerTurn;


        /// <summary>
        /// 플레이어 선턴으로 전투 턴제를 시작합니다.
        /// </summary>
        public static void StartCombatTurns()
        {
            GetInstance().StartCombatTurnsInternal();
        }

        /// <summary>
        /// 플레이어 행동이 끝난 뒤 몬스터 턴으로 넘깁니다.
        /// </summary>
        public static void EndPlayerTurn()
        {
            GetInstance().EndPlayerTurnInternal();
        }

        /// <summary>
        /// 전투 턴제를 종료하고 턴 UI를 닫습니다.
        /// </summary>
        public static void EndCombatTurns()
        {
            GetInstance().EndCombatTurnsInternal();
        }

        /// <summary>
        /// 플레이어 사망 후 재시작 안내 UI를 표시합니다.
        /// </summary>
        public static void ShowGameOverRestartUI()
        {
            GetInstance().ShowGameOverRestartUIInternal();
        }

        private void StartCombatTurnsInternal()
        {
            if (turnCoroutine != null)
            {
                StopCoroutine(turnCoroutine);
                turnCoroutine = null;
            }

            combatTurnState = CombatTurnState.Player;
            ShowTurnStateUI(combatTurnState);
        }

        private void EndPlayerTurnInternal()
        {
            if (combatTurnState != CombatTurnState.Player) return;

            if (turnCoroutine != null) return;

            turnCoroutine = StartCoroutine(EnemyTurnCoroutine());
        }

        private IEnumerator EnemyTurnCoroutine()
        {
            combatTurnState = CombatTurnState.Enemy;
            ShowTurnStateUI(combatTurnState);

            yield return new WaitForSeconds(0.5f);

            List<MonsterController> monsters = GetMonsterTurnOrder();
            foreach (MonsterController monster in monsters)
            {
                if (combatTurnState != CombatTurnState.Enemy)
                {
                    turnCoroutine = null;
                    yield break;
                }

                if (monster == null) continue;

                if (!creatures.ContainsKey(monster.GetInstanceID())) continue;

                if (TryExecuteMonsterAITurn(monster, out AIMove move))
                {
                    yield return WaitForActionCompleted(move.Actor);
                    yield return new WaitForSeconds(0.2f);

                    if (combatTurnState != CombatTurnState.Enemy)
                    {
                        turnCoroutine = null;
                        yield break;
                    }
                }
            }

            combatTurnState = CombatTurnState.Player;
            ShowTurnStateUI(combatTurnState);
            turnCoroutine = null;
        }

        private List<MonsterController> GetMonsterTurnOrder()
        {
            List<MonsterController> monsters = new();

            foreach (KeyValuePair<int, CreatureController> pair in creatures)
            {
                if (pair.Value is not MonsterController monster) continue;

                monsters.Add(monster);
            }

            monsters.Sort((lhs, rhs) => lhs.GetInstanceID().CompareTo(rhs.GetInstanceID()));
            return monsters;
        }

        private IEnumerator WaitForActionCompleted(CreatureController actor)
        {
            if (actor == null) yield break;

            yield return null;

            while (actor != null && actor.IsActing)
            {
                yield return null;
            }
        }

        private void EndCombatTurnsInternal()
        {
            if (turnCoroutine != null)
            {
                StopCoroutine(turnCoroutine);
                turnCoroutine = null;
            }

            combatTurnState = CombatTurnState.Ended;

            if (turnStateUI != null)
            {
                turnStateUI.Close();
                turnStateUI = null;
            }
        }

        private void ShowGameOverRestartUIInternal()
        {
            EndCombatTurnsInternal();

            if (gameOverRestartUI != null) return;

            gameOverRestartUI = UIManager.Open<TitleUI>(UIManager.RenderSpace.Camera, new Vector3(0f, -360f, 0f));
            if (gameOverRestartUI == null) return;

            gameOverRestartUI.SetMessage("처음부터 다시 시작하려면 아무 키나 누르세요");
            gameOverRestartUI.OnClick += RestartTutorialFromGameOver;
        }

        private void RestartTutorialFromGameOver()
        {
            RestartTutorialFromGameOverAsync().Forget();
        }

        private async UniTask RestartTutorialFromGameOverAsync()
        {
            if (isRestartingTutorial) return;
            isRestartingTutorial = true;

            try
            {
                if (gameOverRestartUI != null)
                {
                    gameOverRestartUI.OnClick -= RestartTutorialFromGameOver;
                    await gameOverRestartUI.PlayExitAsync(gameOverRestartUI.Close);
                    gameOverRestartUI = null;
                }

                RemoveAllTileIndicators();
                DespawnAllCreatures();
                UnloadMapTiles();

                UIManager.SetBackgroundColorBlack();
                await EventManager.Trigger<TutorialEvent>();
            }
            finally
            {
                isRestartingTutorial = false;
            }
        }

        private void ShowTurnStateUI(CombatTurnState state)
        {
            if (state != CombatTurnState.Player && state != CombatTurnState.Enemy) return;

            if (turnStateUI == null)
            {
                turnStateUI = UIManager.Open<TurnStateUI>(UIManager.RenderSpace.Camera);
            }

            if (turnStateUI == null) return;

            if (state == CombatTurnState.Player)
            {
                turnStateUI.SetPlayerTurn();
            }
            else
            {
                turnStateUI.SetEnemyTurn();
            }
        }
    }
}
