using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityQuickSheet
{
    /// 数据定义

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
        // 列的位置index
        public int OrderNO { get; set; }

        public override string ToString()
        {
            return name;
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

}