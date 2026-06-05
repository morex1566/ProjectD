using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TRPG.Runtime
{
    public class Dialouge
    {
        public List<string> texts;

        public string speakerName;

        public string eventId;

        public string nextDialogueId;

        public int index;

        public bool IsLastIndex => texts.Count <= index;
    }


    /// <summary>
    /// 대화 시스템의 전역 진입점을 관리하는 매니저입니다.
    /// </summary>
    public class DialogueManager : MonoBehaviourSingleton<DialogueManager>
    {
        private const string DialoguePageSeparator = "\\n";

        private const string DialogueLineBreakMarker = "\\b";

        public static DialougeManagerSettingsData settings;


        /// <summary>
        /// DialogueManager 싱글톤 인스턴스를 보장합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
            settings = Resources.Load<DialougeManagerSettingsData>("SO_DialogueManagerSettings");
        }

        public static Dialouge Load(AssetReferenceT<DialogueData> dialogueData)
        {
            DialogueData data = ResourceManager.GetResource<DialogueData>(dialogueData);

            // "\n"은 다음 대화 페이지로 넘기고, "\b"는 같은 페이지 안의 실제 줄바꿈으로 변환합니다.
            string description = (data.Description ?? string.Empty).Replace(DialogueLineBreakMarker, Environment.NewLine);

            string[] splitTexts = description.Split(new[] { DialoguePageSeparator }, StringSplitOptions.RemoveEmptyEntries);

            return new Dialouge
            {
                texts = new List<string>(splitTexts),
                speakerName = data.Speaker,
                eventId = NormalizeNullableText(data.EventId),
                nextDialogueId = NormalizeNullableText(data.NextDialogueId),
                index = 0
            };
        }

        private static string NormalizeNullableText(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (value.Equals("null", StringComparison.OrdinalIgnoreCase)) return null;

            return value;
        }
    }
}
