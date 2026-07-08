using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    ///<summary>
    /// 엑셀 CreatureSheet에서 생성되는 CreatureData 목록과 조회 캐시를 보관합니다.
    ///</summary>
    [ExcelAsset(AssetPath = "Project/Datas/Gen", ExcelName = "CreatureSheet")]
    public class CreatureSheet : ScriptableObject
    {
        private const string CreatureIdAssetFolder = "Assets/Project/Datas/Gen";

        private const string CreatureIdAssetNamePrefix = "SO_CreatureId_";

        private const string CreatureBehaviourTreePrefabFolder = "Assets/Project/Prefabs/Gameplay";

        private const string CreatureBehaviourTreePrefabNamePrefix = "PF_MBT_";

        ///<summary>
        /// 엑셀에서 생성된 원본 CreatureData 목록입니다.
        ///</summary>
        public List<CreatureData> Entities;

        ///<summary>
        /// DataId를 기준으로 CreatureData를 빠르게 조회하기 위한 캐시입니다.
        ///</summary>
        private readonly Dictionary<string, CreatureData> entityMap = new();

        ///<summary>
        /// DataId 기준 CreatureData 조회 캐시입니다.
        ///</summary>
        public IReadOnlyDictionary<string, CreatureData> EntityMap => entityMap;

        ///<summary>
        /// ScriptableObject가 로드될 때 호출됩니다.
        ///</summary>
        private void OnEnable()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                EditorApplication.delayCall -= OnCreate;
                EditorApplication.delayCall += OnCreate;
            }
#endif

            BuildEntityMap();
        }

#if UNITY_EDITOR
        ///<summary>
        /// delayCall에 예약된 생성 처리가 에셋 언로드 이후 실행되지 않도록 해제합니다.
        ///</summary>
        private void OnDisable()
        {
            EditorApplication.delayCall -= OnCreate;
        }
#endif

        ///<summary>
        /// ScriptableObject 로드/리임포트 이후 엑셀 외부 참조 데이터를 다시 보정합니다.
        ///</summary>
        private void OnCreate()
        {
#if UNITY_EDITOR
            Debug.Log($"{nameof(CreatureSheet)} : OnCreate");

            CreateIdAssets();
            MapEntityAssets();

            EditorUtility.SetDirty(this);
#endif
        }

        ///<summary>
        /// Entities 목록을 기반으로 DataId 조회 캐시를 다시 생성합니다.
        ///</summary>
        private void BuildEntityMap()
        {
            entityMap.Clear();

            if (Entities == null)
            {
                return;
            }

            foreach (CreatureData entity in Entities)
            {
                if (entity == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(entity.DataId) == true)
                {
                    continue;
                }

                // DataId가 중복되면 엑셀에서 뒤에 있는 데이터를 최종 값으로 사용합니다.
                entityMap[entity.DataId] = entity;
            }
        }

        private void CreateIdAssets()
        {
#if UNITY_EDITOR
            if (Entities == null)
            {
                return;
            }

            EnsureCreatureIdAssetFolder();

            foreach (CreatureData entity in Entities)
            {
                if (entity == null || string.IsNullOrEmpty(entity.DataId) == true)
                {
                    continue;
                }

                string assetPath = BuildCreatureIdAssetPath(entity.DataId);
                CreatureIdData creatureIdData = AssetDatabase.LoadAssetAtPath<CreatureIdData>(assetPath);

                if (creatureIdData == null)
                {
                    creatureIdData = CreateInstance<CreatureIdData>();
                    AssetDatabase.CreateAsset(creatureIdData, assetPath);
                }

                creatureIdData.Id = entity.DataId;
                EditorUtility.SetDirty(creatureIdData);
            }

#endif
        }

        private void MapEntityAssets()
        {
#if UNITY_EDITOR
            if (Entities == null)
            {
                return;
            }

            foreach (CreatureData entity in Entities)
            {
                if (entity == null || string.IsNullOrEmpty(entity.DataId) == true)
                {
                    continue;
                }

                // DataId와 같은 이름 규칙의 스프라이트/BT 프리팹을 찾아 엔티티에 매핑합니다.
                entity.Sprite = FindCreatureSprite(entity.DataId);
                entity.BehaviourTreePrefab = FindCreatureBehaviourTreePrefab(entity.DataId);

                if (entity.Sprite == null)
                {
                    Debug.LogWarning($"Creature sprite not found. DataId: {entity.DataId}", this);
                }

                if (entity.BehaviourTreePrefab == null)
                {
                    Debug.LogWarning($"Creature behaviour tree prefab not found. DataId: {entity.DataId}", this);
                }
            }

            EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        ///<summary>
        /// CreatureIdData 에셋을 저장할 폴더를 보장합니다.
        ///</summary>
        private static void EnsureCreatureIdAssetFolder()
        {
            if (AssetDatabase.IsValidFolder(CreatureIdAssetFolder) == true)
            {
                return;
            }

            AssetDatabase.CreateFolder("Assets/Project/Datas", "Gen");
        }

        ///<summary>
        /// DataId를 에셋 파일명으로 사용할 수 있는 경로로 변환합니다.
        ///</summary>
        private static string BuildCreatureIdAssetPath(string dataId)
        {
            string safeDataId = SanitizeFileName(dataId);
            return $"{CreatureIdAssetFolder}/{CreatureIdAssetNamePrefix}{safeDataId}.asset";
        }

        ///<summary>
        /// 파일명에 사용할 수 없는 문자를 밑줄로 치환합니다.
        ///</summary>
        private static string SanitizeFileName(string value)
        {
            string safeValue = value;

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                safeValue = safeValue.Replace(invalidChar, '_');
            }

            return safeValue;
        }

        ///<summary>
        /// DataId와 같은 이름을 가진 스프라이트를 프로젝트에서 찾습니다.
        ///</summary>
        private static Sprite FindCreatureSprite(string dataId)
        {
            string[] spriteGuids = AssetDatabase.FindAssets($"{dataId} t:Sprite");

            foreach (string spriteGuid in spriteGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(spriteGuid);
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                foreach (Object asset in assets)
                {
                    Sprite sprite = asset as Sprite;

                    if (sprite == null)
                    {
                        continue;
                    }

                    if (sprite.name == dataId || sprite.name.StartsWith($"{dataId}_"))
                    {
                        return sprite;
                    }
                }
            }

            return null;
        }

        ///<summary>
        /// DataId와 같은 이름 규칙의 Creature BT 프리팹을 찾습니다.
        ///</summary>
        private static GameObject FindCreatureBehaviourTreePrefab(string dataId)
        {
            string expectedName = $"{CreatureBehaviourTreePrefabNamePrefix}{dataId}";
            string[] prefabGuids = AssetDatabase.FindAssets($"{dataId} t:Prefab", new[] { CreatureBehaviourTreePrefabFolder });

            foreach (string prefabGuid in prefabGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (prefab != null && prefab.name == expectedName)
                {
                    return prefab;
                }
            }

            return null;
        }
#endif

        ///<summary>
        /// DataId로 CreatureData를 조회합니다.
        ///</summary>
        public bool TryGetEntity(string dataId, out CreatureData entity)
        {
            return entityMap.TryGetValue(dataId, out entity);
        }
    }
}
