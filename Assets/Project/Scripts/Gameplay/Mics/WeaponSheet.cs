using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    /// <summary>
    /// 엑셀 WeaponSheet에서 생성되는 WeaponData 목록과 조회 캐시를 보관합니다.
    /// </summary>
    [ExcelAsset(AssetPath = "Project/Datas/Gen", ExcelName = "WeaponSheet")]
    public class WeaponSheet : ScriptableObject
    {
        private const string WeaponBasePrefabName = "PF_weapon";

        private const string WeaponIdAssetFolder = "Assets/Project/Datas/Gen";

        private const string WeaponIdAssetNamePrefix = "SO_WeaponId_";

        private const string WeaponPrefabFolder = "Assets/Project/Prefabs/Gameplay";


        /// <summary>
        /// 엑셀에서 생성된 원본 WeaponData 목록입니다.
        /// </summary>
        public List<WeaponData> Entities;

        /// <summary>
        /// Id를 기준으로 WeaponData를 빠르게 조회하기 위한 캐시입니다.
        /// </summary>
        private readonly Dictionary<string, WeaponData> entityMap = new();

        /// <summary>
        /// Id 기준 WeaponData 조회 캐시입니다.
        /// </summary>
        public IReadOnlyDictionary<string, WeaponData> EntityMap => entityMap;

        /// <summary>
        /// ScriptableObject가 로드될 때 호출됩니다.
        /// </summary>
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
        /// <summary>
        /// delayCall에 예약된 생성 처리가 에셋 언로드 이후 실행되지 않도록 해제합니다.
        /// </summary>
        private void OnDisable()
        {
            EditorApplication.delayCall -= OnCreate;
        }
#endif

        /// <summary>
        /// ScriptableObject 로드/리임포트 이후 엑셀 외부 참조 데이터를 다시 보정합니다.
        /// </summary>
        private void OnCreate()
        {
#if UNITY_EDITOR
            Debug.Log($"{nameof(WeaponSheet)} : OnCreate");

            CreateIdAssets();
            MapEntityAssets();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

        /// <summary>
        /// Entities 목록을 기반으로 Id 조회 캐시를 다시 생성합니다.
        /// </summary>
        private void BuildEntityMap()
        {
            entityMap.Clear();

            if (Entities == null)
            {
                return;
            }

            foreach (WeaponData entity in Entities)
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

            EnsureWeaponIdAssetFolder();

            foreach (WeaponData entity in Entities)
            {
                if (entity == null || string.IsNullOrEmpty(entity.Id) == true)
                {
                    continue;
                }

                string assetPath = BuildWeaponIdAssetPath(entity.Id);
                WeaponIdData weaponIdData = AssetDatabase.LoadAssetAtPath<WeaponIdData>(assetPath);

                if (weaponIdData == null)
                {
                    weaponIdData = CreateInstance<WeaponIdData>();
                    AssetDatabase.CreateAsset(weaponIdData, assetPath);
                }

                weaponIdData.Id = entity.Id;
                EditorUtility.SetDirty(weaponIdData);
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

            foreach (WeaponData entity in Entities)
            {
                if (entity == null || string.IsNullOrEmpty(entity.Id) == true)
                {
                    continue;
                }

                // Id와 같은 이름 규칙의 스프라이트를 찾아 WeaponData에 매핑합니다.
                entity.Sprite = FindWeaponSprite(entity.Id);
                entity.Prefab = CreateOrUpdateWeaponPrefab(entity);

                if (entity.Sprite == null)
                {
                    Debug.LogWarning($"Weapon sprite not found. Id: {entity.Id}", this);
                }

                if (entity.Prefab == null)
                {
                    Debug.LogWarning($"Weapon prefab not found or generated. Id: {entity.Id}", this);
                }
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// WeaponIdData 에셋을 저장할 폴더를 보장합니다.
        /// </summary>
        private static void EnsureWeaponIdAssetFolder()
        {
            if (AssetDatabase.IsValidFolder(WeaponIdAssetFolder) == true)
            {
                return;
            }

            AssetDatabase.CreateFolder("Assets/Project/Datas", "Gen");
        }

        /// <summary>
        /// Id를 에셋 파일명으로 사용할 수 있는 경로로 변환합니다.
        /// </summary>
        private static string BuildWeaponIdAssetPath(string id)
        {
            string safeId = SanitizeFileName(id);
            return $"{WeaponIdAssetFolder}/{WeaponIdAssetNamePrefix}{safeId}.asset";
        }

        /// <summary>
        /// 파일명에 사용할 수 없는 문자를 밑줄로 치환합니다.
        /// </summary>
        private static string SanitizeFileName(string value)
        {
            string safeValue = value;

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                safeValue = safeValue.Replace(invalidChar, '_');
            }

            return safeValue;
        }

        /// <summary>
        /// Id와 같은 이름을 가진 무기 스프라이트를 프로젝트에서 찾습니다.
        /// </summary>
        private static Sprite FindWeaponSprite(string id)
        {
            string legacySpriteName = id.Replace("weapon_", "item_equipment_");
            string[] spriteGuids = AssetDatabase.FindAssets($"{id} t:Sprite");

            if (spriteGuids == null || spriteGuids.Length <= 0)
            {
                spriteGuids = AssetDatabase.FindAssets($"{legacySpriteName} t:Sprite");
            }

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

                    if (IsMatchingWeaponSpriteName(sprite.name, id, legacySpriteName) == true)
                    {
                        return sprite;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 신규 weapon_* 이름과 이전 item_equipment_* 이름을 모두 허용합니다.
        /// </summary>
        private static bool IsMatchingWeaponSpriteName(string spriteName, string id, string legacySpriteName)
        {
            return spriteName == id ||
                   spriteName.StartsWith($"{id}_") ||
                   spriteName == legacySpriteName ||
                   spriteName.StartsWith($"{legacySpriteName}_");
        }

        /// <summary>
        /// WeaponData를 반영한 완성 Weapon 프리팹을 생성하거나 갱신합니다.
        /// </summary>
        private static GameObject CreateOrUpdateWeaponPrefab(WeaponData entity)
        {
            string prefabPath = ResolvePrefabPath(entity.PrefabPath, entity.Id, WeaponPrefabFolder);
            entity.PrefabPath = prefabPath;

            string basePrefabPath = FindExactPrefabPath(WeaponBasePrefabName, WeaponPrefabFolder);
            if (string.IsNullOrEmpty(basePrefabPath) == true)
            {
                Debug.LogWarning($"Weapon base prefab not found. Name: {WeaponBasePrefabName}");
                return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            EnsureAssetFolderForPath(prefabPath);

            bool isExistingPrefab = File.Exists(prefabPath);
            GameObject prefabRoot = isExistingPrefab == true
                ? PrefabUtility.LoadPrefabContents(prefabPath)
                : InstantiateBasePrefabForVariant(basePrefabPath);

            if (prefabRoot == null)
            {
                Debug.LogWarning($"Weapon prefab generation failed. Prefab root is null. Path: {prefabPath}");
                return null;
            }

            prefabRoot.name = Path.GetFileNameWithoutExtension(prefabPath);

            WeaponController controller = prefabRoot.GetComponent<WeaponController>();
            if (controller == null)
            {
                Debug.LogWarning($"Weapon prefab generation failed. WeaponController is missing. Path: {prefabPath}");
                ReleasePrefabRoot(prefabRoot, isExistingPrefab);
                return null;
            }

            ApplyGeneratedWeaponPrefabData(prefabRoot, controller, entity);

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            ReleasePrefabRoot(prefabRoot, isExistingPrefab);
            AssetDatabase.ImportAsset(prefabPath);

            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        /// <summary>
        /// WeaponSheet가 생성하는 prefab asset의 직렬화 값만 기록합니다.
        /// </summary>
        private static void ApplyGeneratedWeaponPrefabData(GameObject prefabRoot, WeaponController controller, WeaponData entity)
        {
            SpriteRenderer spriteRenderer = EnsureWeaponSpriteRenderer(prefabRoot);

            SerializedObject serializedController = new(controller);
            SetObjectReference(serializedController, "spriter", spriteRenderer);
            WriteGeneratedWeaponContext(serializedController, entity);
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = entity.Sprite;
                EditorUtility.SetDirty(spriteRenderer);
            }

            EditorUtility.SetDirty(controller);
        }

        private static SpriteRenderer EnsureWeaponSpriteRenderer(GameObject prefabRoot)
        {
            SpriteRenderer spriteRenderer = prefabRoot.GetComponentInChildren<SpriteRenderer>(true);
            if (spriteRenderer != null)
            {
                return spriteRenderer;
            }

            GameObject spriteObject = new("Sprite");
            spriteObject.transform.SetParent(prefabRoot.transform);
            spriteObject.transform.localPosition = Vector3.zero;
            spriteObject.transform.localRotation = Quaternion.identity;
            spriteObject.transform.localScale = Vector3.one;
            return spriteObject.AddComponent<SpriteRenderer>();
        }

        private static void WriteGeneratedWeaponContext(SerializedObject serializedController, WeaponData entity)
        {
            SetString(serializedController, "context.Id", entity.Id);
            SetString(serializedController, "context.Name", entity.Name);
            SetString(serializedController, "context.Description", entity.Description);
            SetFloat(serializedController, "context.Damage", entity.Damage);
            SetFloat(serializedController, "context.AttackRange", entity.AttackRange);
            SetFloat(serializedController, "context.AttackSpeed", entity.AttackSpeed);
            SetFloat(serializedController, "context.Weight", entity.Weight);
            SetObjectReference(serializedController, "context.Sprite", entity.Sprite);
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

        private static void SetObjectReference(SerializedObject serializedObject, string propertyPath, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        /// <summary>
        /// Base prefab 인스턴스를 만들어 신규 저장 시 prefab variant가 되도록 합니다.
        /// </summary>
        private static GameObject InstantiateBasePrefabForVariant(string basePrefabPath)
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
            return PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
        }

        /// <summary>
        /// 기존 프리팹 contents와 신규 variant 인스턴스를 생성 방식에 맞게 정리합니다.
        /// </summary>
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

        /// <summary>
        /// 시트의 PrefabPath가 비어 있으면 기본 생성 경로를 사용합니다.
        /// </summary>
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

        /// <summary>
        /// 지정 이름과 정확히 일치하는 프리팹 경로를 찾습니다.
        /// </summary>
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

        /// <summary>
        /// 프리팹 저장 경로의 폴더를 보장합니다.
        /// </summary>
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

        /// <summary>
        /// Id로 WeaponData를 조회합니다.
        /// </summary>
        public bool TryGetEntity(string id, out WeaponData entity)
        {
            return entityMap.TryGetValue(id, out entity);
        }
    }
}
