using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace ExcelEditor.Tool
{
    public class ExcelEditorWindow : EditorWindow
    {
        [MenuItem("Window/Tools/ExcelEditorWindow")]
        public static void ShowExample()
        {
            ExcelEditorWindow wnd = GetWindow<ExcelEditorWindow>();
            wnd.titleContent = new GUIContent("ExcelEditorWindow");
            wnd.Show();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = EditorUIResources.GetTemplateAsset(nameof(ExcelEditorWindow));
            visualTree.CloneTree(root);
            InitPanel();

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = EditorUIResources.GetStyleSheetAsset(nameof(ExcelEditorWindow));
            VisualElement labelWithStyle = new Label("Hello World! With Style");
            labelWithStyle.styleSheets.Add(styleSheet);
            root.Add(labelWithStyle);
        }

        /// <summary>
        /// 初始化window上的Panel
        /// </summary>
        private void InitPanel()
        {
            var panel = rootVisualElement.Q("editor-table-panel");
            var tablePanel = EditorUIResources.GetTemplateAsset(nameof(EditorAssetTables));
            tablePanel.CloneTree(panel);
        }
    }
}