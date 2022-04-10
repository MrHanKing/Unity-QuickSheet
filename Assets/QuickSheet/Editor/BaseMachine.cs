///////////////////////////////////////////////////////////////////////////////
///
/// BaseMachine.cs
/// 
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

namespace UnityQuickSheet
{
    /// <summary>
    /// 数据容器 编辑器时用
    /// </summary>
    public class CellEntry
    {
        public string rawValue;
    }

    /// <summary>
    /// 一列数据
    /// </summary>
    public class ColumnDatas : ScriptableObject
    {
        public ColumnHeader columnHeader;
        public AllRowEntryHeader allRowEntryHeader;

        public List<CellEntry> datasEntry = new List<CellEntry>();
    }


    /// <summary>
    /// 列头的数据
    /// </summary>
    [System.Serializable]
    public class ColumnHeader
    {
        public CellType type;
        public string name;
        public bool isEnable;
        public bool isArray;
        public ColumnHeader nextArrayItem;

        // used to order columns by ascending. (only need on excel-plugin)
        public int OrderNO { get; set; }

        public override string ToString()
        {
            return name;
        }
    }

    /// <summary>
    /// 表格所有的行Header数据 编辑器用
    /// </summary>
    public class AllRowEntryHeader : ScriptableObject
    {
        [SerializeField]
        public List<RowEntryHeader> m_Entries = new List<RowEntryHeader>();

        /// <summary>
        /// RowEntryHeader.Key 和 value的存储 方便索引
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="RowEntryHeader"></typeparam>
        /// <returns></returns>
        Dictionary<string, RowEntryHeader> m_KeyDictionary = new Dictionary<string, RowEntryHeader>();
        /// <summary>
        /// 是否有包含key的行存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key) => FindWithKey(key) != null;
        RowEntryHeader FindWithKey(string key)
        {
            if (m_KeyDictionary.Count == 0)
            {
                foreach (var keyAndIdPair in m_Entries)
                {
                    m_KeyDictionary[keyAndIdPair.Key] = keyAndIdPair;
                }
            }

            m_KeyDictionary.TryGetValue(key, out var foundPair);
            return foundPair;
        }

        /// <summary>
        /// 重命名某行
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public void RenameKey(string oldValue, string newValue)
        {
            var foundEntry = FindWithKey(oldValue);
            if (foundEntry != null)
                RenameKeyInternal(foundEntry, newValue);
        }
        void RenameKeyInternal(RowEntryHeader entry, string newValue)
        {
            if (m_KeyDictionary.Count > 0)
            {
                m_KeyDictionary.Remove(entry.Key);
                m_KeyDictionary[newValue] = entry;
            }

            entry.Key = newValue;
        }
    }

    /// <summary>
    /// 行数据
    /// </summary>
    [Serializable]
    public class RowEntryHeader
    {
        [SerializeField]
        long m_Id;

        [SerializeField]
        string m_Key;

        // [SerializeField]
        // MetadataCollection m_Metadata = new MetadataCollection();

        /// <summary>
        /// Unique id
        /// </summary>
        public long Id
        {
            get => m_Id;
            internal set => m_Id = value;
        }

        /// <summary>
        /// The name of the key, must also be unique.
        /// </summary>
        public string Key
        {
            get => m_Key;
            internal set => m_Key = value;
        }

        /// <summary>
        /// Optional Metadata for this key that is also shared between all tables that use this <see cref="SharedTableData"/>.
        /// </summary>
        // public MetadataCollection Metadata
        // {
        //     get => m_Metadata;
        //     set => m_Metadata = value;
        // }

        public override string ToString() => $"{Id} - {Key}";
    }


    /// <summary>
    /// A class which stores various settings for a worksheet which is imported.
    /// </summary>
    public class BaseMachine : ScriptableObject
    {
        protected readonly static string ImportSettingFilename = "New Import Setting.asset";

        [SerializeField]
        private string templatePath = "QuickSheet/Templates";
        /// <summary>
        /// 脚本模版位置
        /// </summary>
        /// <value></value>
        public string TemplatePath
        {
            get { return templatePath; }
            set { templatePath = value; }
        }

        /// <summary>
        /// path the created ScriptableObject class file will be located.
        /// </summary>
        [SerializeField]
        private string scriptFilePath;
        /// <summary>
        /// 运行时类位置
        /// </summary>
        /// <value></value>
        public string RuntimeClassPath
        {
            get { return scriptFilePath; }
            set { scriptFilePath = value; }
        }

        /// <summary>
        /// path the created editor script files will be located.
        /// </summary>
        [SerializeField]
        private string editorScriptFilePath;
        /// <summary>
        /// 编辑器脚本位置
        /// </summary>
        /// <value></value>
        public string EditorClassPath
        {
            get { return editorScriptFilePath; }
            set { editorScriptFilePath = value; }
        }

        [SerializeField]
        private string saveScriptAssetFilePath;
        /// <summary>
        /// 编辑器生成的ScriptObject所在位置
        /// </summary>
        /// <value></value>
        public string SaveScriptAssetFilePath
        {
            get { return saveScriptAssetFilePath; }
            set { saveScriptAssetFilePath = value; }
        }

        [SerializeField]
        private string sheetName;
        public string SpreadSheetName
        {
            get { return sheetName; }
            set { sheetName = value; }
        }

        [SerializeField]
        private string workSheetName;
        /// <summary>
        /// 选中sheet的名字
        /// </summary>
        /// <value></value>
        public string WorkSheetName
        {
            get { return workSheetName; }
            set { workSheetName = value; }
        }

        public string WorkTableName
        {
            get { return workSheetName; }
        }

        [System.NonSerialized]
        public bool onlyCreateDataClass = false;

        public List<ColumnHeader> ColumnHeaderList
        {
            get { return columnHeaderList; }
            set { columnHeaderList = value; }
        }

        [SerializeField]
        protected List<ColumnHeader> columnHeaderList;

        /// <summary>
        /// Return true, if the list is instantiated and has any its item more than one.
        /// </summary>
        /// <returns></returns>
        public bool HasColumnHeader()
        {
            if (columnHeaderList != null && columnHeaderList.Count > 0)
                return true;

            return false;
        }

        protected readonly string DEFAULT_CLASS_PATH = "Scripts/Runtime";
        protected readonly string DEFAULT_EDITOR_PATH = "Scripts/Editor";

        protected void OnEnable()
        {
            if (columnHeaderList == null)
                columnHeaderList = new List<ColumnHeader>();
        }

        /// <summary>
        /// Initialize with default value whenever the asset file is enabled.
        /// </summary>
        public void ReInitialize()
        {
            if (string.IsNullOrEmpty(RuntimeClassPath))
                RuntimeClassPath = DEFAULT_CLASS_PATH;
            if (string.IsNullOrEmpty(EditorClassPath))
                EditorClassPath = DEFAULT_EDITOR_PATH;

            // reinitialize. it does not need to be serialized.
            onlyCreateDataClass = false;
        }
    }
}