using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityQuickSheet;

namespace ExcelEditor.Tool
{
    /// <summary>
    /// 跟UI匹配 windows上创建表格编辑
    /// </summary>
    public class EditorAssetTables : VisualElement
    {
        internal new class UxmlFactory : UxmlFactory<EditorAssetTables> { }
        /// <summary>
        /// 表格内容
        /// </summary>
        VisualElement m_TableContents;
        ProjectTablesPopup m_AssetTablesField;
        // TableEditor m_CurrentEditor;

        // public LocalizationTableCollection SelectedCollection => m_AssetTablesField.value;

        public EditorAssetTables()
        {
            var asset = EditorUIResources.GetTemplateAsset(nameof(EditorAssetTables));
            asset.CloneTree(this);

            m_TableContents = this.Q("table-contents");
            m_AssetTablesField = this.Q<ProjectTablesPopup>();
            m_AssetTablesField.RegisterValueChangedCallback((evt) => TableCollectionSelected(evt.newValue));
            // TableCollectionSelected(m_AssetTablesField.value);
        }

        void TableCollectionSelected(ExcelMachine ltc)
        {
            m_TableContents.Clear();

            // if (m_CurrentEditor != null)
            // {
            //     m_CurrentEditor.OnDisable();
            // }

            // if (ltc == null || ltc.TableType == null)
            //     return;

            // var editorType = GetEditorTypeForCollection(ltc.GetType());
            // if (editorType == null)
            //     return;

            // m_CurrentEditor = (TableEditor)Activator.CreateInstance(editorType);
            // m_CurrentEditor.TableCollection = ltc;
            // m_TableContents.Add(m_CurrentEditor);
            // m_CurrentEditor.StretchToParentSize();
            // m_CurrentEditor.OnEnable();
        }

        // static Type GetEditorTypeForCollection(Type tableType)
        // {
        //     var editors = TypeCache.GetTypesWithAttribute<TableCollectionEditorAttribute>();
        //     Type editorType = null;
        //     foreach (var e in editors)
        //     {
        //         var attribute = e.GetCustomAttribute<TableCollectionEditorAttribute>();
        //         if (attribute.EditorTargetType == tableType)
        //         {
        //             editorType = e;
        //             break;
        //         }
        //     }

        //     if (editorType == null)
        //         Debug.LogError($"Table Type {tableType.Name} does not have a Table Editor. Please use the TableEditorAttribute to assign one.");

        //     return editorType;
        // }

        // public void Select(LocalizationTableCollection collection) => m_AssetTablesField.value = collection;

    }
}

