using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityQuickSheet;

namespace ExcelEditor.Tool
{
    /// <summary>
    /// 表格选择下拉条
    /// T 选择的数据内容
    /// </summary>
    public class ProjectTablesPopup : PopupField<ExcelMachine>
    {
        public new class UxmlFactory : UxmlFactory<ProjectTablesPopup> { }

        class NoTables : ExcelMachine
        {

        }

        const string k_EditorPrefValueKey = "Localization-SelectedAssetTable";
        const string k_NoTablesMessage = "No Asset Tables Found. Please Create One";
        static readonly NoTables k_NoTables = NoTables.CreateInstance<NoTables>();
        static List<ExcelMachine> s_Tables;
        public ProjectTablesPopup()
        : base(GetChoices(), GetDefaultIndex(), FormatSelectedLabel, FormatListLabel)
        {
            label = "Selected Table Collection";
            // LocalizationEditorSettings.EditorEvents.CollectionAdded += OnCollectionAdded;
            // LocalizationEditorSettings.EditorEvents.CollectionRemoved += OnCollectionRemoved;
            // LocalizationEditorSettings.EditorEvents.CollectionModified += OnCollectionModified;
        }

        ~ProjectTablesPopup()
        {
            // LocalizationEditorSettings.EditorEvents.CollectionAdded -= OnCollectionAdded;
            // LocalizationEditorSettings.EditorEvents.CollectionRemoved -= OnCollectionRemoved;
            // LocalizationEditorSettings.EditorEvents.CollectionModified -= OnCollectionModified;
        }

        void OnCollectionModified(object called, ExcelMachine col)
        {
            OnCollectionAdded(col);
        }

        void OnCollectionAdded(ExcelMachine col)
        {
            bool isEmpty = value is NoTables;
            GetChoices();

            // If we currently have no tables then select the new collection.
            if (isEmpty)
                value = col;
        }

        void OnCollectionRemoved(ExcelMachine col)
        {
            var choices = GetChoices();

            if (value == col)
                value = choices[0];
        }

        static int GetDefaultIndex()
        {
            var selection = EditorPrefs.GetString(k_EditorPrefValueKey, null);
            if (!string.IsNullOrEmpty(selection))
            {
                for (int i = 0; i < s_Tables.Count; ++i)
                {
                    if (s_Tables[i]?.ToString() == selection)
                        return i;
                }
            }

            return 0;
        }

        public override ExcelMachine value
        {
            get => base.value;
            set
            {
                if (value == null)
                    EditorPrefs.DeleteKey(k_EditorPrefValueKey);
                else
                    EditorPrefs.SetString(k_EditorPrefValueKey, value.ToString());
                base.value = value;
            }
        }

        public void RefreshLabels()
        {
            GetChoices();
            var newValue = Mathf.Clamp(s_Tables.FindIndex(o => value.Equals(o)), 0, s_Tables.Count);
            SetValueWithoutNotify(s_Tables[newValue]);
        }

        static string FormatListLabel(ExcelMachine atc)
        {
            return atc is NoTables ? atc.ToString() : $"{atc.GetType().Name}/{atc.WorkSheetName}";
        }

        static string FormatSelectedLabel(ExcelMachine atc)
        {
            if (atc == null)
            {
                return nameof(NoTables);
            }
            return atc.ToString();
        }

        static List<ExcelMachine> GetChoices()
        {
            if (s_Tables == null)
                s_Tables = new List<ExcelMachine>();
            s_Tables.Clear();

            s_Tables.AddRange(ExcelSettings.Instance.TablesOSCache);

            if (s_Tables.Count == 0)
                s_Tables.Add(k_NoTables);
            return s_Tables;
        }
    }
}

