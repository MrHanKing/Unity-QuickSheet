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
    /// </summary>
    public class ProjectTablesPopup : PopupField<ExcelExample>
    {
        public new class UxmlFactory : UxmlFactory<ProjectTablesPopup> { }

        class NoTables : ExcelExample
        {

        }

        const string k_EditorPrefValueKey = "Localization-SelectedAssetTable";
        const string k_NoTablesMessage = "No Asset Tables Found. Please Create One";
        static readonly NoTables k_NoTables = NoTables.CreateInstance<NoTables>();
        static List<ExcelExample> s_Tables;
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

        void OnCollectionModified(object called, ExcelExample col)
        {
            OnCollectionAdded(col);
        }

        void OnCollectionAdded(ExcelExample col)
        {
            bool isEmpty = value is NoTables;
            GetChoices();

            // If we currently have no tables then select the new collection.
            if (isEmpty)
                value = col;
        }

        void OnCollectionRemoved(ExcelExample col)
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

        public override ExcelExample value
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

        static string FormatListLabel(ExcelExample atc)
        {
            return atc is NoTables ? atc.ToString() : $"{atc.GetType().Name}/{atc.WorksheetName}";
        }

        static string FormatSelectedLabel(ExcelExample atc) => atc.ToString();

        static List<ExcelExample> GetChoices()
        {
            if (s_Tables == null)
                s_Tables = new List<ExcelExample>();
            s_Tables.Clear();

            s_Tables.AddRange(ExcelSettings.Instance.TablesOSCache);

            if (s_Tables.Count == 0)
                s_Tables.Add(k_NoTables);
            return s_Tables;
        }
    }
}

