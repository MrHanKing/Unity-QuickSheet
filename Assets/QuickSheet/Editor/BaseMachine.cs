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
    /// A class which stores various settings for a worksheet which is imported.
    /// </summary>
    public class BaseMachine : ScriptableObject
    {
        protected readonly static string ImportSettingFilename = "New Import Setting.asset";

        [SerializeField]
        private string sheetName;
        /// <summary>
        /// 电子表格名字
        /// </summary>
        /// <value></value>
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

        protected void OnEnable()
        {
            if (columnHeaderList == null)
                columnHeaderList = new List<ColumnHeader>();
        }

    }
}