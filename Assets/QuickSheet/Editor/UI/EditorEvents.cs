using System;
using UnityQuickSheet;

namespace ExcelEditor.Tool
{
    /// <summary>
    /// 表格工具编辑器事件
    /// </summary>
    public class EditorEvents
    {
        public event Action<ColumnHeader> LocaleAdded;
        /// <summary>
        /// 增加一列数据
        /// </summary>
        /// <param name="locale"></param>
        internal virtual void RaiseColumnAdded(ColumnHeader locale) => LocaleAdded?.Invoke(locale);
        /// <summary>
        /// 删除一列
        /// </summary>
        public event Action<ColumnHeader> LocaleRemoved;
        internal virtual void RaiseLocaleRemoved(ColumnHeader locale) => LocaleRemoved?.Invoke(locale);

        // public event EventHandler<Locale> LocaleSortOrderChanged;
        // internal virtual void RaiseLocaleSortOrderChanged(object sender, Locale locale) => LocaleSortOrderChanged?.Invoke(sender, locale);

        // public event Action<SharedTableData.SharedTableEntry> TableEntryModified;
        // internal virtual void RaiseTableEntryModified(SharedTableData.SharedTableEntry tableEntry) => TableEntryModified?.Invoke(tableEntry);

        // public event Action<LocalizationTableCollection, SharedTableData.SharedTableEntry> TableEntryAdded;
        // internal virtual void RaiseTableEntryAdded(LocalizationTableCollection collection, SharedTableData.SharedTableEntry entry) => TableEntryAdded?.Invoke(collection, entry);

        // public event Action<LocalizationTableCollection, SharedTableData.SharedTableEntry> TableEntryRemoved;
        // internal virtual void RaiseTableEntryRemoved(LocalizationTableCollection collection, SharedTableData.SharedTableEntry entry) => TableEntryRemoved?.Invoke(collection, entry);

        // public event Action<AssetTableCollection, AssetTable, AssetTableEntry> AssetTableEntryAdded;
        // internal virtual void RaiseAssetTableEntryAdded(AssetTableCollection collection, AssetTable table, AssetTableEntry entry) => AssetTableEntryAdded?.Invoke(collection, table, entry);

        // public event Action<AssetTableCollection, AssetTable, AssetTableEntry, string> AssetTableEntryRemoved;
        // internal virtual void RaiseAssetTableEntryRemoved(AssetTableCollection collection, AssetTable table, AssetTableEntry entry, string assetGuid) => AssetTableEntryRemoved?.Invoke(collection, table, entry, assetGuid);

        // public event EventHandler<LocalizationTableCollection> CollectionModified;
        // internal virtual void RaiseCollectionModified(object sender, LocalizationTableCollection collection) => CollectionModified?.Invoke(sender, collection);

        // public event Action<LocalizationTableCollection> CollectionAdded;
        // internal virtual void RaiseCollectionAdded(LocalizationTableCollection collection) => CollectionAdded?.Invoke(collection);

        // public event Action<LocalizationTableCollection> CollectionRemoved;
        // internal virtual void RaiseCollectionRemoved(LocalizationTableCollection collection) => CollectionRemoved?.Invoke(collection);

        // public event Action<LocalizationTableCollection, LocalizationTable> TableAddedToCollection;
        // internal virtual void RaiseTableAddedToCollection(LocalizationTableCollection collection, LocalizationTable table) => TableAddedToCollection?.Invoke(collection, table);

        // public event Action<LocalizationTableCollection, LocalizationTable> TableRemovedFromCollection;
        // internal virtual void RaiseTableRemovedFromCollection(LocalizationTableCollection collection, LocalizationTable table) => TableRemovedFromCollection?.Invoke(collection, table);
    }
}