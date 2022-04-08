using UnityEditor;
using UnityEngine.UIElements;
using UnityQuickSheet;

namespace ExcelEditor.Tool
{
    /// <summary>
    /// 表格编辑界面容器
    /// </summary>
    abstract class TableEditor : VisualElement
    {
        public ExcelMachine TableCollection { get; set; }

        /// <summary>
        /// 表格内容
        /// </summary>
        protected VisualElement m_TableContentsPanel;
        /// <summary>
        /// 选择的属性内容
        /// </summary>
        protected VisualElement m_PropertiesPanel;
        // 表格名字输入框
        protected TextField m_NameField;
        // 帮助条
        // protected VisualElement m_TableNameHelpBoxContainer;
        // protected VisualElement m_TableNameHelpBox;

        public virtual void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;

            var asset = EditorUIResources.GetTemplateAsset(GetType().Name);
            asset.CloneTree(this);

            m_TableContentsPanel = this.Q("table-contents-panel");
            // m_PropertiesPanel = this.Q("properties-panel");

            m_NameField = this.Q<TextField>("table-name-field");
            m_NameField.value = TableCollection.WorkSheetName;
            m_NameField.RegisterCallback<ChangeEvent<string>>(TableCollectionNameChanged);
            m_NameField.isDelayed = true; // Prevent an undo per char change.

            // m_TableNameHelpBoxContainer = this.Q("table-name-help-box-container");
        }

        public virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        /// <summary>
        /// 查找表格下拉条
        /// </summary>
        /// <returns></returns>
        ProjectTablesPopup FindTablesPopup()
        {
            // Find the root
            var root = parent;
            while (root.parent != null)
            {
                root = root.parent;
            }
            return root.Q<ProjectTablesPopup>();
        }

        /// <summary>
        /// 名字修改的时候的回调
        /// </summary>
        /// <param name="evt"></param>
        void TableCollectionNameChanged(ChangeEvent<string> evt)
        {
            // m_TableNameHelpBox?.RemoveFromHierarchy();

            if (TableCollection.WorkSheetName == evt.newValue)
                return;

            // 检查名字有效不
            // var tableNameError = LocalizationEditorSettings.Instance.IsTableNameValid(TableCollection.GetType(), evt.newValue);
            // if (tableNameError != null)
            // {
            //     m_TableNameHelpBox = HelpBoxFactory.CreateDefaultHelpBox(tableNameError);
            //     m_TableNameHelpBoxContainer.Add(m_TableNameHelpBox);
            //     return;
            // }

            // Todo 改名后续支持
            // TableCollection.SetTableCollectionName(evt.newValue, true);

            // Force the label to update itself.
            // var atf = FindTablesPopup();
            // atf?.RefreshLabels();
        }

        /// <summary>
        /// 撤销 Todo
        /// </summary>
        protected virtual void UndoRedoPerformed()
        {
            // TableCollection.RefreshAddressables();

            // var name = TableCollection.WorkSheetName;
            // m_NameField?.SetValueWithoutNotify(name);
            // var atf = FindTablesPopup();
            // atf?.RefreshLabels();
        }

        protected virtual void TableListViewOnSelectedForEditing(ISelectable selected)
        {
            m_PropertiesPanel.Clear();

            if (selected != null)
            {
                m_PropertiesPanel.style.display = DisplayStyle.Flex;
                var editor = selected.CreateEditor();
                m_PropertiesPanel.Add(editor);
                editor.StretchToParentSize();
            }
            else
            {
                m_PropertiesPanel.style.display = DisplayStyle.None;
            }
        }
    }
}