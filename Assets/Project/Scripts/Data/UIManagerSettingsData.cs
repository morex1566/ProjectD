using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// UIManager가 런타임에 인스턴스화할 UI 프리팹 참조를 보관합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_UIManagerSettings", menuName = "Scriptable Objects/Settings/UIManager")]
    public class UIManagerSettingsData : ScriptableObject
    {
        [Header(nameof(UIManagerSettingsData) + ".Setup")]

        [SerializeField] protected Sprite cursorShape;

        [SerializeField] protected LoadingUI loadingUI;

        [SerializeField] private TitleUI titleMessagePf;

        [SerializeField] private DialougeUI dialougeUIPf;

        [SerializeField] protected PanelUI panelUIPf;



        private readonly Dictionary<Type, UIBase> uiPbs = new();


        public Sprite CursorShape => cursorShape;


        private void OnEnable()
        {
            CacheUIPrefabs();
        }

        /// <summary>
        /// 요청한 UI 컴포넌트 타입에 맞는 프리팹을 반환합니다.
        /// </summary>
        public T Get<T>() where T : UIBase
        {
            Type type = typeof(T);

            if (!uiPbs.TryGetValue(type, out UIBase prefab))
            {
                Debug.LogError($"[{nameof(UIManagerSettingsData)}] {type.Name} 타입에 맞는 UI 프리팹을 찾지 못했습니다.");
            }

            return prefab as T;
        }

        /// <summary>
        /// Settings에 등록된 UI 프리팹 필드를 타입별 매핑으로 변환합니다.
        /// </summary>
        private void CacheUIPrefabs()
        {
            uiPbs.Clear();

            FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                if (!typeof(UIBase).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                UIBase uiBase = field.GetValue(this) as UIBase;
                if (uiBase == null)
                {
                    continue;
                }

                Type uiType = uiBase.GetType();

                if (uiPbs.ContainsKey(uiType))
                {
                    Debug.LogWarning($"[{nameof(UIManagerSettingsData)}] 이미 등록된 UI 타입입니다. Type: {uiType.Name}", this);
                    continue;
                }

                uiPbs.Add(uiType, uiBase);
            }
        }
    }
}