using UnityEngine;
using TMPro;

namespace TRPG.Runtime
{
    public class TurnStateUI : UIBase
    {
        private const string PlayerTurn = "나의 턴";

        private const string EnemyTurn = "상대 턴";

        [SerializeField] private TMP_Text messageText = null;

        protected override void Awake()
        {
            base.Awake();
            Bind();
        }

        private void Reset()
        {
            Bind();
        }

        private void OnValidate()
        {
            Bind();
        }

        /// <summary>
        /// 플레이어 턴 문구로 갱신합니다.
        /// </summary>
        public void SetPlayerTurn()
        {
            SetText(PlayerTurn);
        }

        /// <summary>
        /// 상대 턴 문구로 갱신합니다.
        /// </summary>
        public void SetEnemyTurn()
        {
            SetText(EnemyTurn);
        }

        private void SetText(string text)
        {
            if (messageText == null) return;

            messageText.text = text;
        }

        private void Bind()
        {
            messageText = GetComponentInChildren<TMP_Text>(true);
        }
    }
}
