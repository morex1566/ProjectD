using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 대화 엑셀 시트의 한 행을 ScriptableObject로 저장하는 데이터입니다.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "SO_Dialogue", menuName = "Scriptable Objects/Data/Dialogue")]
    public class DialogueData : ScriptableObject
    {
        [ReadOnly] public string Id;

        [ReadOnly] public string Speaker;

        [ReadOnly] public string Description;

        [ReadOnly] public string NextDialogueId;

        [ReadOnly] public string EventId;
    }
}
