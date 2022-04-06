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

        List<ExcelExample> m_TablesOSCache;
        public List<ExcelExample> TablesOSCache
        {
            get
            {
                if (m_TablesOSCache == null)
                    m_TablesOSCache = LoadTableCollections<ExcelExample>();
                return m_TablesOSCache;
            }
        }

        protected virtual List<TCollection> LoadTableCollections<TCollection>() where TCollection : ExcelExample
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
            return foundCollections.OrderBy(col => col.WorksheetName).ToList();
        }
        protected virtual string[] FindAssets(string filter) => AssetDatabase.FindAssets(filter);
    }
}
