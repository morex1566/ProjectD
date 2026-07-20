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
        private const string CreatureBasePrefabName = "PF_creature";

        private const string CreatureIdAssetFolder = "Assets/Project/Datas/Gen";

        private const string CreatureIdAssetNamePrefix = "SO_CreatureId_";

        private const string CreaturePrefabFolder = "Assets/Project/Prefabs/Gameplay";

        private const string CreatureBehaviourTreePrefabNamePrefix = "PF_MBT_";

        ///<summary>
        /// 엑셀에서 생성된 원본 CreatureData 목록입니다.
        ///</summary>   
        public List<CreatureData> Entities;

        ///<summary>
        /// Id를 기준으로 CreatureData를 빠르게 조회하기 위한 캐시입니다.
        ///</summary>
        private readonly Dictionary<string, CreatureData> entityMap = new();

        ///<summary>
        /// Id 기준 CreatureData 조회 캐시입니다.
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
            AssetDatabase.SaveAssets();
#endif
        }

        ///<summary>
        /// Entities 목록을 기반으로 Id 조회 캐시를 다시 생성합니다.
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

                if (string.IsNullOrEmpty(entity.Id) == true)
                {
                    continue;
                }

                // Id가 중복되면 엑셀에서 뒤에 있는 데이터를 최종 값으로 사용합니다.
                entityMap[entity.Id] = entity;
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
                if (entity == null || string.IsNullOrEmpty(entity.Id) == true)
                {
                    continue;
                }

                string assetPath = BuildCreatureIdAssetPath(entity.Id);
                CreatureIdData creatureIdData = AssetDatabase.LoadAssetAtPath<CreatureIdData>(assetPath);

                if (creatureIdData == null)
                {
                    creatureIdData = CreateInstance<CreatureIdData>();
                    AssetDatabase.CreateAsset(creatureIdData, assetPath);
                }

                creatureIdData.Id = entity.Id;
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
                if (entity == null || string.IsNullOrEmpty(entity.Id) == true)
                {
                    continue;
                }

                // Id와 같은 이름 규칙의 스프라이트와 MBT 프리팹을 찾아 엔티티에 매핑합니다.
                entity.Sprite = FindCreatureSprite(entity.Id);
                entity.BehaviourTree = FindCreatureBehaviourTreePrefab(entity.Id);
                entity.Prefab = CreateOrUpdateCreaturePrefab(entity);

                if (entity.Sprite == null)
                {
                    Debug.LogWarning($"Creature sprite not found. Id: {entity.Id}", this);
                }

                if (entity.BehaviourTree == null)
                {
                    Debug.LogWarning($"Creature behaviour tree prefab not found. Id: {entity.Id}", this);
                }

                if (entity.Prefab == null)
                {
                    Debug.LogWarning($"Creature prefab not found or generated. Id: {entity.Id}", this);
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
        /// Id를 에셋 파일명으로 사용할 수 있는 경로로 변환합니다.
        ///</summary>
        private static string BuildCreatureIdAssetPath(string id)
        {
            string safeId = SanitizeFileName(id);
            return $"{CreatureIdAssetFolder}/{CreatureIdAssetNamePrefix}{safeId}.asset";
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
        /// Id와 같은 이름을 가진 스프라이트를 프로젝트에서 찾습니다.
        ///</summary>
        private static Sprite FindCreatureSprite(string id)
        {
            string[] spriteGuids = AssetDatabase.FindAssets($"{id} t:Sprite");

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

                    if (sprite.name == id || sprite.name.StartsWith($"{id}_"))
                    {
                        return sprite;
                    }
                }
            }

            return null;
        }

        ///<summary>
        /// Id와 같은 이름 규칙의 Creature MBT 프리팹을 찾습니다.
        ///</summary>
        private static GameObject FindCreatureBehaviourTreePrefab(string id)
        {
            string prefabName = $"{CreatureBehaviourTreePrefabNamePrefix}{id}";
            string prefabPath = FindExactPrefabPath(prefabName, CreaturePrefabFolder);

            if (string.IsNullOrEmpty(prefabPath) == true)
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        ///<summary>
        /// CreatureData를 반영한 완성 Creature 프리팹을 생성하거나 갱신합니다.
        ///</summary>
        private static GameObject CreateOrUpdateCreaturePrefab(CreatureData entity)
        {
            string prefabPath = ResolvePrefabPath(entity.PrefabPath, entity.Id, CreaturePrefabFolder);
            entity.PrefabPath = prefabPath;

            string basePrefabPath = FindExactPrefabPath(CreatureBasePrefabName, CreaturePrefabFolder);
            if (string.IsNullOrEmpty(basePrefabPath) == true)
            {
                Debug.LogWarning($"Creature base prefab not found. Name: {CreatureBasePrefabName}");
                return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            EnsureAssetFolderForPath(prefabPath);

            bool isExistingPrefab = File.Exists(prefabPath);
            GameObject prefabRoot = isExistingPrefab == true
                ? PrefabUtility.LoadPrefabContents(prefabPath)
                : InstantiateBasePrefabForVariant(basePrefabPath);

            if (prefabRoot == null)
            {
                Debug.LogWarning($"Creature prefab generation failed. Prefab root is null. Path: {prefabPath}");
                return null;
            }

            prefabRoot.name = Path.GetFileNameWithoutExtension(prefabPath);

            CreatureController controller = prefabRoot.GetComponent<CreatureController>();
            if (controller == null)
            {
                Debug.LogWarning($"Creature prefab generation failed. CreatureController is missing. Path: {prefabPath}");
                ReleasePrefabRoot(prefabRoot, isExistingPrefab);
                return null;
            }

            ApplyGeneratedCreaturePrefabData(prefabRoot, controller, entity);

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            ReleasePrefabRoot(prefabRoot, isExistingPrefab);
            AssetDatabase.ImportAsset(prefabPath);

            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        ///<summary>
        /// CreatureSheet가 생성하는 prefab asset의 직렬화 값만 기록합니다.
        ///</summary>
        private static void ApplyGeneratedCreaturePrefabData(GameObject prefabRoot, CreatureController controller, CreatureData entity)
        {
            SpriteRenderer spriteRenderer = prefabRoot.GetComponentInChildren<SpriteRenderer>(true);
            BoxCollider2DSizeFitter collider2DSizeFitter = prefabRoot.GetComponentInChildren<BoxCollider2DSizeFitter>(true);
            CreateGeneratedBehaviourTree(prefabRoot, entity.BehaviourTree);
            SerializedObject serializedController = new(controller);
            SetObjectReference(serializedController, "spriter", spriteRenderer);
            WriteGeneratedCreatureContext(serializedController, entity);
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = entity.Sprite;
                EditorUtility.SetDirty(spriteRenderer);
            }

            if (collider2DSizeFitter != null)
            {
                collider2DSizeFitter.Fit();
                EditorUtility.SetDirty(collider2DSizeFitter);
            }

            EditorUtility.SetDirty(controller);
        }

        ///<summary>
        /// 기존에 생성된 MBT 자식을 교체하고 현재 Creature의 MBT 프리팹을 자식으로 추가합니다.
        ///</summary>
        private static void CreateGeneratedBehaviourTree(GameObject prefabRoot, GameObject behaviourTreePrefab)
        {
            MBT.MonoBehaviourTree[] behaviourTrees = prefabRoot.GetComponentsInChildren<MBT.MonoBehaviourTree>(true);
            foreach (MBT.MonoBehaviourTree behaviourTree in behaviourTrees)
            {
                if (behaviourTree == null || behaviourTree.transform == prefabRoot.transform)
                {
                    continue;
                }

                DestroyImmediate(behaviourTree.gameObject);
            }

            if (behaviourTreePrefab == null)
            {
                return;
            }

            GameObject behaviourTreeObject = PrefabUtility.InstantiatePrefab(behaviourTreePrefab, prefabRoot.transform) as GameObject;
            if (behaviourTreeObject == null)
            {
                behaviourTreeObject = Instantiate(behaviourTreePrefab, prefabRoot.transform);
            }

            behaviourTreeObject.name = behaviourTreePrefab.name;
            behaviourTreeObject.transform.localPosition = Vector3.zero;
            behaviourTreeObject.transform.localRotation = Quaternion.identity;
            behaviourTreeObject.transform.localScale = Vector3.one;
            EditorUtility.SetDirty(behaviourTreeObject);
        }

        private static void WriteGeneratedCreatureContext(SerializedObject serializedController, CreatureData entity)
        {
            SetFloat(serializedController, "context.BaseAtk", entity.Damage);
            SetFloat(serializedController, "context.BaseAttackRange", entity.AttackRange);
            SetFloat(serializedController, "context.BaseAttackSpeed", entity.AttackSpeed);
            SetFloat(serializedController, "context.Hp", entity.Hp);
            SetFloat(serializedController, "context.Atk", entity.Damage);
            SetFloat(serializedController, "context.DetectRange", entity.DetectRange);
            SetFloat(serializedController, "context.AttackRange", entity.AttackRange);
            SetFloat(serializedController, "context.AttackSpeed", entity.AttackSpeed);
            SetFloat(serializedController, "context.MoveSpeed", entity.MoveSpeed);
            SetString(serializedController, "context.Id", entity.Id);
            SetString(serializedController, "context.Name", entity.Name);
            SetString(serializedController, "context.Description", entity.Description);
            SetInt(serializedController, "context.Faction", (int)CreatureContext.ParseFaction(entity.Faction));
            SetObjectReference(serializedController, "context.Sprite", entity.Sprite);
            ClearGeneratedWeaponContext(serializedController);
        }

        private static void ClearGeneratedWeaponContext(SerializedObject serializedController)
        {
            SetString(serializedController, "context.EquippedWeapon.Id", null);
            SetString(serializedController, "context.EquippedWeapon.Name", null);
            SetString(serializedController, "context.EquippedWeapon.Description", null);
            SetFloat(serializedController, "context.EquippedWeapon.Damage", 0f);
            SetFloat(serializedController, "context.EquippedWeapon.AttackRange", 0f);
            SetFloat(serializedController, "context.EquippedWeapon.AttackSpeed", 0f);
            SetFloat(serializedController, "context.EquippedWeapon.Weight", 0f);
            SetObjectReference(serializedController, "context.EquippedWeapon.Sprite", null);
        }

        private static void SetString(SerializedObject serializedObject, string propertyPath, string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property != null)
            {
                property.stringValue = value ?? string.Empty;
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyPath, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetInt(SerializedObject serializedObject, string propertyPath, int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyPath, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        ///<summary>
        /// Base prefab 인스턴스를 만들어 신규 저장 시 prefab variant가 되도록 합니다.
        ///</summary>
        private static GameObject InstantiateBasePrefabForVariant(string basePrefabPath)
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
            return PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
        }

        ///<summary>
        /// 기존 프리팹 contents와 신규 variant 인스턴스를 생성 방식에 맞게 정리합니다.
        ///</summary>
        private static void ReleasePrefabRoot(GameObject prefabRoot, bool isLoadedPrefabContents)
        {
            if (prefabRoot == null)
            {
                return;
            }

            if (isLoadedPrefabContents == true)
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                return;
            }

            DestroyImmediate(prefabRoot);
        }

        ///<summary>
        /// 시트의 PrefabPath가 비어 있으면 기본 생성 경로를 사용합니다.
        ///</summary>
        private static string ResolvePrefabPath(string prefabPath, string id, string defaultFolder)
        {
            string normalizedPath = string.IsNullOrWhiteSpace(prefabPath) == true
                ? $"{defaultFolder}/PF_{id}.prefab"
                : prefabPath.Replace('\\', '/');

            if (normalizedPath.EndsWith(".prefab") == false)
            {
                normalizedPath = $"{normalizedPath.TrimEnd('/')}/PF_{id}.prefab";
            }

            if (normalizedPath.StartsWith("Assets/") == true)
            {
                return normalizedPath;
            }

            Debug.LogWarning($"PrefabPath must start with Assets/. Id: {id}, PrefabPath: {prefabPath}");
            return $"{defaultFolder}/PF_{id}.prefab";
        }

        ///<summary>
        /// 지정 이름과 정확히 일치하는 프리팹 경로를 찾습니다.
        ///</summary>
        private static string FindExactPrefabPath(string prefabName, string folder)
        {
            string[] prefabGuids = AssetDatabase.FindAssets($"{prefabName} t:Prefab", new[] { folder });

            foreach (string prefabGuid in prefabGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (prefab != null && prefab.name == prefabName)
                {
                    return assetPath;
                }
            }

            return null;
        }

        ///<summary>
        /// 프리팹 저장 경로의 폴더를 보장합니다.
        ///</summary>
        private static void EnsureAssetFolderForPath(string assetPath)
        {
            string directoryPath = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(directoryPath) == true)
            {
                return;
            }

            Directory.CreateDirectory(directoryPath);
            AssetDatabase.Refresh();
        }
#endif

        ///<summary>
        /// Id로 CreatureData를 조회합니다.
        ///</summary>
        public bool TryGetEntity(string id, out CreatureData entity)
        {
            return entityMap.TryGetValue(id, out entity);
        }
    }
}
