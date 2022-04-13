///////////////////////////////////////////////////////////////////////////////
///
/// ExcelSettings.cs
/// 
/// (c)2015 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ExcelEditor.Tool;
using System;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace UnityQuickSheet
{
    /// <summary>
    /// A class manages excel setting.
    /// </summary>
    [CreateAssetMenu(menuName = "QuickSheet/Setting/Excel Setting")]
    public class ExcelSettings : SingletonScriptableObject<ExcelSettings>
    {
        /// <summary>
        /// A default path where .txt template files are.
        /// </summary>
        public string TemplatePath = "QuickSheet/ExcelPlugin/Templates";

        /// <summary>
        /// A path where generated ScriptableObject derived class and its data class script files are to be put.
        /// </summary>
        public string RuntimePath = string.Empty;

        /// <summary>
        /// A path where generated editor script files are to be put.
        /// </summary>
        public string EditorPath = string.Empty;
        /// <summary>
        /// 脚本资源所在路径
        /// </summary>
        [SerializeField]
        public string SaveScriptAssetFilePath = string.Empty;

        /// <summary>
        /// Select currently exist account setting asset file.
        /// </summary>
        [MenuItem("Edit/QuickSheet/Select Excel Setting")]
        public static void Edit()
        {
            Selection.activeObject = Instance;
            if (Selection.activeObject == null)
            {
                Debug.LogError(@"No ExcelSetting.asset file is found. Create setting file first. See the menu at 'Create/QuickSheet/Setting/Excel Setting'.");
            }
        }

        public EditorEvents editorEvents = new EditorEvents();

        List<ExcelMachine> m_TablesOSCache;
        /// <summary>
        /// 编辑器表格数据缓存列表
        /// </summary>
        /// <value></value>
        public List<ExcelMachine> TablesOSCache
        {
            get
            {
                if (m_TablesOSCache == null)
                    m_TablesOSCache = LoadTableCollections<ExcelMachine>();
                return m_TablesOSCache;
            }
        }

        /// <summary>
        /// 加载所有编辑器表格数据
        /// </summary>
        /// <typeparam name="TCollection"></typeparam>
        /// <returns></returns>
        protected virtual List<TCollection> LoadTableCollections<TCollection>() where TCollection : BaseMachine
        {
            var foundCollections = new List<TCollection>();
            var foundAssets = FindAssets($"t:{typeof(TCollection).Name}");
            foreach (var guid in foundAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var collection = AssetDatabase.LoadAssetAtPath<TCollection>(path);
                if (collection == null)
                {
                    Debug.LogError($"Could not load collection as type {typeof(TCollection).Name} at path {path}.");
                    continue;
                }

                // var validState = collection.IsValid;
                // if (!validState.valid)
                // {
                //     Debug.LogWarning($"Collection {collection.name} is invalid and will be ignored because {validState.error}.");
                //     continue;
                // }
                foundCollections.Add(collection);
            }
            Debug.Log($"table count {foundCollections.Count}");
            return foundCollections.OrderBy(col => col.WorkSheetName).ToList();
        }
        /// <summary>
        /// 根据过滤条件查找资源
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>guid[]</returns>
        protected virtual string[] FindAssets(string filter) => AssetDatabase.FindAssets(filter);

        /// <summary>
        /// 给资源设置预加载的Label
        /// </summary>
        /// <param name="table"></param>
        /// <param name="preload"></param>
        /// <param name="createUndo"></param>
        public static void SetPreloadTableFlag(ColumnDatas table, bool preload, bool createUndo = false)
        {
            Instance.SetPreloadTableInternal(table, preload, createUndo);
        }
        internal const string PreloadLabel = "Preload";
        protected virtual void SetPreloadTableInternal(ColumnDatas table, bool preload, bool createUndo = false)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table), "Can not set preload flag on a null table");

            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            var tableEntry = GetAssetEntry(table);
            if (tableEntry == null)
                throw new AddressableEntryNotFoundException(table);

            if (createUndo)
                Undo.RecordObjects(new UnityEngine.Object[] { aaSettings, tableEntry.parentGroup }, "Set Preload flag");

            tableEntry.SetLabel(ExcelSettings.PreloadLabel, preload, preload);
        }
        internal virtual AddressableAssetSettings GetAddressableAssetSettings(bool create)
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(create);
            if (settings != null)
                return settings;

            // By default Addressables wont return the settings if updating or compiling. This causes issues for us, especially if we are trying to get the Locales.
            // We will just ignore this state and try to get the settings regardless.
            if (EditorApplication.isUpdating || EditorApplication.isCompiling)
            {
                // Legacy support
                if (EditorBuildSettings.TryGetConfigObject(AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName, out settings))
                {
                    return settings;
                }

                AddressableAssetSettingsDefaultObject so;
                if (EditorBuildSettings.TryGetConfigObject(AddressableAssetSettingsDefaultObject.kDefaultConfigObjectName, out so))
                {
                    // Extract the guid
                    var serializedObject = new SerializedObject(so);
                    var guid = serializedObject.FindProperty("m_AddressableAssetSettingsGuid")?.stringValue;
                    if (!string.IsNullOrEmpty(guid))
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        return AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(path);
                    }
                }
            }
            return null;
        }
        internal virtual AddressableAssetEntry GetAssetEntry(UnityEngine.Object asset) => GetAssetEntry(asset.GetInstanceID());

        internal virtual AddressableAssetEntry GetAssetEntry(int instanceId)
        {
            var settings = GetAddressableAssetSettings(false);
            if (settings == null)
                return null;

            var guid = GetAssetGuid(instanceId);
            return settings.FindAssetEntry(guid);
        }
        internal virtual string GetAssetGuid(int instanceId)
        {
            Debug.Assert(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(instanceId, out string guid, out long _), "Failed to extract the asset Guid");
            return guid;
        }

        #region 
        protected readonly string DEFAULT_CLASS_PATH = "Scripts/Runtime";
        protected readonly string DEFAULT_EDITOR_PATH = "Scripts/Editor";
        /// <summary>
        /// Initialize with default value whenever the asset file is enabled.
        /// </summary>
        public void ReInitialize()
        {
            if (string.IsNullOrEmpty(RuntimePath))
                RuntimePath = DEFAULT_CLASS_PATH;
            if (string.IsNullOrEmpty(EditorPath))
                EditorPath = DEFAULT_EDITOR_PATH;
        }
        #endregion
    }

    public class AddressableEntryNotFoundException : Exception
    {
        public AddressableEntryNotFoundException(UnityEngine.Object target) :
            base($"{target.name} could not find an Addressable asset.")
        {
        }
    }



}
