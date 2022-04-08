using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityQuickSheet;

namespace ExcelEditor.Tool
{
    /// <summary>
    /// 一行的视图构建
    /// </summary>
    /// <typeparam name="T1">一整列的数据类型</typeparam>
    class GenericAssetTableTreeViewItem<T1> : TreeViewItem
    {
        // 对应行定义
        public virtual RowEntryHeader SharedEntry { get; set; }

        public string Key
        {
            get => SharedEntry.Key;
            set => SharedEntry.Key = value;
        }

        public long KeyId => SharedEntry.Id;

        public bool Selected { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        /// <summary>
        /// Called during the setup of the tree view.
        /// </summary>
        public virtual void Initialize(ExcelMachine collection, int startIdx, List<ColumnDatas> sortedTables) { }

        /// <summary>
        /// Called before the key entry is deleted.
        /// </summary>
        public virtual void OnDeleteKey() { }
    }
}