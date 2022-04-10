using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using UnityQuickSheet;

namespace ExcelEditor.Tool
{
    /// <summary>
    /// 关于多列的Header渲染数据
    /// </summary>
    /// <typeparam name="T1">一列的数据类型</typeparam>
    /// <typeparam name="T2">一行的数据类型</typeparam>
    class GenericAssetTableListViewMultiColumnHeader<T1, T2> : MultiColumnHeader
            where T1 : ColumnDatas
            where T2 : GenericAssetTableTreeViewItem<T1>, new()
    {
        const float k_MetadataLabelHeight = 20;

        static readonly GUIContent k_MissingTableMenuItem = new GUIContent("Missing Tables");

        static readonly GUIContent k_MetadataIcon = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>("Packages/com.unity.localization/Editor/Icons/Localization_AssetTable.png"), "Edit Table Metadata");

        ExcelMachine TableCollection { get; set; }

        GenericAssetTableListView<T1, T2> m_Parent;

        public GenericAssetTableListViewMultiColumnHeader(MultiColumnHeaderState state, GenericAssetTableListView<T1, T2> parent, ExcelMachine collection)
            : base(state)
        {
            height += k_MetadataLabelHeight;
            m_Parent = parent;
            TableCollection = collection;

            var visibleColumns = new List<int>();
            for (int i = 0; i < state.columns.Length; ++i)
            {
                if (state.columns[i] is VisibleColumn col && col.Visible)
                    visibleColumns.Add(i);
                else if (state.columns[i] is MissingTableColumn && MissingTableColumn.Visible)
                    visibleColumns.Add(i);
            }
            state.visibleColumns = visibleColumns.ToArray();
        }

        protected override void AddColumnHeaderContextMenuItems(GenericMenu menu)
        {
            menu.AddItem(EditorGUIUtility.TrTextContent("Resize to Fit"), false, ResizeToFit);

            menu.AddSeparator("");

            for (int i = 0; i < state.columns.Length; ++i)
            {
                var column = state.columns[i];

                var menuText = column.headerContent.text;
                if (column.allowToggleVisibility)
                    menu.AddItem(new GUIContent(menuText), state.visibleColumns.Contains(i), ToggleVisibilityCallback, i);
                else if (column is MissingTableColumn)
                {
                    menu.AddSeparator("");
                    menu.AddItem(k_MissingTableMenuItem, IsColumnVisible(i), ToggleMissingTableColumnsVisibility);
                    break;
                }
            }
        }

        void ToggleVisibilityCallback(object userData)
        {
            int colIdx = (int)userData;
            var visibleCol = GetColumn(colIdx) as VisibleColumn;
            if (visibleCol != null)
                visibleCol.Visible = !visibleCol.Visible;
            ToggleVisibility(colIdx);
        }

        void ToggleMissingTableColumnsVisibility()
        {
            var show = !MissingTableColumn.Visible;
            MissingTableColumn.Visible = show;
            for (int i = 0; i < state.columns.Length; ++i)
            {
                var column = state.columns[i];
                if (column is MissingTableColumn)
                {
                    if (show != IsColumnVisible(i))
                        ToggleVisibility(i);
                }
            }
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            var btnRect = new Rect(headerRect.x, headerRect.y, headerRect.width, k_MetadataLabelHeight);
            headerRect.yMin += k_MetadataLabelHeight;
            switch (column)
            {
                case ISelectable selectable:
                    {
                        EditorGUI.BeginChangeCheck();
                        GUI.Toggle(btnRect, selectable.Selected, k_MetadataIcon, GUI.skin.button);
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_Parent.Selected = selectable;
                        }
                        base.ColumnHeaderGUI(column, headerRect, columnIndex);
                        break;
                    }

                case VisibleColumn _:
                    base.ColumnHeaderGUI(column, headerRect, columnIndex);
                    break;

                case MissingTableColumn mtc:
                    {
                        var labelRect = new Rect(headerRect.x, headerRect.yMax - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing, headerRect.width, EditorGUIUtility.singleLineHeight);
                        if (GUI.Button(labelRect, mtc.headerContent))
                        {
                            CreateMissingTable(mtc);
                        }
                        break;
                    }
            }
        }

        void CreateMissingTable(MissingTableColumn mtc)
        {
            TableCollection.AddNewTable(mtc.TableLocale);
        }
    }

    /// <summary>
    /// 可见列
    /// </summary>
    class VisibleColumn : MultiColumnHeaderState.Column
    {
        const string k_ColumnVisiblePref = "Localization-Table-Window-{0}";

        public bool defaultVisibility { get; set; } = true;

        public bool Visible
        {
            get => EditorPrefs.GetBool(string.Format(k_ColumnVisiblePref, headerContent.text), defaultVisibility);
            set => EditorPrefs.SetBool(string.Format(k_ColumnVisiblePref, headerContent.text), value);
        }
    }

    /// <summary>
    /// 记录一列数据的列样式
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    class TableColumn<T1> : VisibleColumn, ISelectable where T1 : ColumnDatas
    {
        public T1 Table { get; set; }
        public SerializedObject SerializedObjectTable { get; private set; }
        public SerializedObject SerializedObjectSharedTableData { get; private set; }

        public ColumnHeader TableLocale { get; set; }
        public bool Selected { get; set; }

        ExcelMachine m_TableCollection;
        bool m_PreloadAllMixedValues;
        bool m_PreloadAllToggle;
        bool m_PreloadSelectedToggle;

        public TableColumn(ExcelMachine collection, ColumnDatas table, ColumnHeader locale)
        {
            m_TableCollection = collection;
            minWidth = 100;
            Table = table as T1;
            SerializedObjectTable = new SerializedObject(Table);
            SerializedObjectSharedTableData = new SerializedObject(Table.allRowEntryHeader);
            TableLocale = locale;
            headerContent = new GUIContent(TableLocale != null ? TableLocale.ToString() : table.columnHeader.ToString());
            headerTextAlignment = TextAlignment.Center;
            canSort = false;
            allowToggleVisibility = true;
            Undo.undoRedoPerformed += RefreshPreloadToggle;
        }

        ~TableColumn()
        {
            Undo.undoRedoPerformed -= RefreshPreloadToggle;
        }

        void RefreshPreloadToggle()
        {
            // TODO: We could use the instanceId instead of the asset so that we do not need to load the asset. (e.g for hidden columns)
            // int preloadCount = m_TableCollection.Tables.Count(tbl => tbl.asset != null && LocalizationEditorSettings.GetPreloadTableFlag(tbl.asset));
            // if (preloadCount == 0)
            // {
            //     m_PreloadAllToggle = false;
            //     m_PreloadAllMixedValues = false;
            //     m_PreloadSelectedToggle = false;
            // }
            // else if (preloadCount == m_TableCollection.Tables.Count)
            // {
            //     m_PreloadAllToggle = true;
            //     m_PreloadAllMixedValues = false;
            //     m_PreloadSelectedToggle = true;
            // }
            // else
            // {
            //     m_PreloadAllToggle = true;
            //     m_PreloadAllMixedValues = true;
            //     m_PreloadSelectedToggle = LocalizationEditorSettings.GetPreloadTableFlag(Table);
            // }
        }

        public VisualElement CreateEditor()
        {
            var root = new VisualElement() { style = { marginLeft = 5, marginRight = 5, marginTop = 5, marginBottom = 5 } };

            var preloadEditor = new IMGUIContainer(() =>
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = m_PreloadAllMixedValues;
                m_PreloadAllToggle = EditorGUILayout.Toggle("Preload All Tables", m_PreloadAllToggle);
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                {
                    var group = Undo.GetCurrentGroup();
                    Undo.SetCurrentGroupName("Set Preload flag");
                    foreach (var table in m_TableCollection.Tables)
                    {
                        ExcelSettings.SetPreloadTableFlag(table.asset, m_PreloadAllToggle, true);
                    }
                    Undo.CollapseUndoOperations(group);
                    RefreshPreloadToggle();
                }

                EditorGUI.BeginChangeCheck();
                m_PreloadSelectedToggle = EditorGUILayout.Toggle("Preload Table", m_PreloadSelectedToggle);
                if (EditorGUI.EndChangeCheck())
                {
                    ExcelSettings.SetPreloadTableFlag(Table, m_PreloadSelectedToggle, true);
                    RefreshPreloadToggle();
                }
            });
            preloadEditor.AddToClassList("unity-box");
            root.Add(preloadEditor);

            var sharedTableDataEditor = new IMGUIContainer(() =>
            {
                SerializedObjectSharedTableData.Update();
                EditorGUILayout.LabelField("Shared", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(SerializedObjectSharedTableData.FindProperty("m_Metadata"));
                SerializedObjectSharedTableData.ApplyModifiedProperties();
            });
            sharedTableDataEditor.style.borderBottomWidth = 10;
            root.Add(sharedTableDataEditor);

            RefreshPreloadToggle();

            // TODO: Clean this up so we have editors for each type.
            // var tablePropDrawer = new MetadataCollectionField();
            // tablePropDrawer.Type = new MetadataTypeAttribute(Table is AssetTable ? MetadataType.AssetTable : MetadataType.StringTable);
            // var label = new GUIContent("Metadata");
            // var tableEditor = new IMGUIContainer(() =>
            // {
            //     SerializedObjectTable.Update();
            //     EditorGUILayout.LabelField(headerContent, EditorStyles.boldLabel);

            //     var p = SerializedObjectTable.FindProperty("m_Metadata");
            //     var rect = EditorGUILayout.GetControlRect(true, tablePropDrawer.GetPropertyHeight(p, label));
            //     tablePropDrawer.OnGUI(rect, p, label);
            //     SerializedObjectTable.ApplyModifiedProperties();
            // });
            // root.Add(tableEditor);

            return root;
        }

        public override string ToString() => "Table: " + Table;
    }

    /// <summary>
    /// key列样式
    /// </summary>
    class KeyColumn : VisibleColumn
    {
        public AllRowEntryHeader SharedData { get; }

        public KeyColumn(AllRowEntryHeader sharedData)
        {
            minWidth = 100;
            SharedData = sharedData;
            headerContent = new GUIContent("Key");
            headerTextAlignment = TextAlignment.Center;
            canSort = true;
            allowToggleVisibility = true;
        }

        public override string ToString() => "Key";
    }

    /// <summary>
    /// ID 列样式
    /// </summary>
    class KeyIdColumn : VisibleColumn
    {
        public AllRowEntryHeader Keys { get; }

        public KeyIdColumn(AllRowEntryHeader sharedData)
        {
            minWidth = 50;
            maxWidth = 150;
            Keys = sharedData;
            headerContent = new GUIContent("Key Id");
            headerTextAlignment = TextAlignment.Center;
            canSort = true;
            defaultVisibility = false;
            allowToggleVisibility = true;
        }

        public override string ToString() => "Key Id";
    }

    /// <summary>
    /// 丢失列样式
    /// </summary>
    class MissingTableColumn : MultiColumnHeaderState.Column
    {
        const string k_ColumnVisiblePref = "Localization-Table-Window-{0}";

        public ColumnHeader TableLocale { get; }

        public static bool Visible
        {
            get => EditorPrefs.GetBool(string.Format(k_ColumnVisiblePref, "Missing Tables"), false);
            set => EditorPrefs.SetBool(string.Format(k_ColumnVisiblePref, "Missing Tables"), value);
        }

        public MissingTableColumn(ColumnHeader locale)
        {
            minWidth = 50;
            maxWidth = 150;

            TableLocale = locale;
            headerContent = new GUIContent("Add " + TableLocale);
            headerTextAlignment = TextAlignment.Center;
            canSort = false;
            allowToggleVisibility = false;
        }

        public override string ToString() => "Missing Table: " + TableLocale;
    }
}