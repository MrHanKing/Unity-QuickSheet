///////////////////////////////////////////////////////////////////////////////
///
/// ExcelMachine.cs
///
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace UnityQuickSheet
{
    /// <summary>
    /// A class for various setting to read excel file and generated related script files.
    /// 表格编辑器存放的数据体
    /// </summary>
    public class ExcelMachine : BaseMachine
    {
        /// <summary>
        /// where the .xls or .xlsx file is. The path should start with "Assets/".
        /// </summary>
        public string excelFilePath;

        // both are needed for popup editor control.
        public string[] SheetNames = { "" };
        public int CurrentSheetIndex
        {
            get { return currentSelectedSheet; }
            set { currentSelectedSheet = value; }
        }

        [SerializeField]
        protected int currentSelectedSheet = 0;
        /// <summary>
        /// 实际存储给运行时用的数据
        /// </summary>
        [SerializeField]
        public ExcelTableBase targetTable;

        [SerializeField]
        List<LazyLoadReference<ColumnDatas>> m_Tables = new List<LazyLoadReference<ColumnDatas>>();

        /// <summary>
        /// 所有行header数据
        /// </summary>
        public AllRowEntryHeader allRowEntryHeader;

        /// <summary>
        /// 所有列元素
        /// </summary>
        ReadOnlyCollection<LazyLoadReference<ColumnDatas>> m_ReadOnlyTables;
        public ReadOnlyCollection<LazyLoadReference<ColumnDatas>> Tables
        {
            get
            {
                if (m_ReadOnlyTables == null)
                {
                    RemoveBrokenTables();
                    m_ReadOnlyTables = m_Tables.AsReadOnly();
                }
                return m_ReadOnlyTables;
            }
        }

        /// <summary>
        /// 清空编辑器记录的列数据
        /// </summary>
        void RemoveBrokenTables()
        {
            // We cant do this in OnBeforeSerialize or OnAfterDeserialize as it uses ForceLoadFromInstanceID and this is not allowed to be called during serialization.
            int brokenCount = 0;
            for (int i = 0; i < m_Tables.Count; ++i)
            {
                if (m_Tables[i].isBroken)
                {
                    m_Tables.RemoveAt(i);
                    --i;
                    ++brokenCount;
                }
            }

            if (brokenCount > 0)
            {
                Debug.LogWarning($"删除了{targetTable.SheetName}记录的所有数据", this);
                EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// 给表格增加一列空数据
        /// </summary>
        /// <param name="columnHeader"></param>
        public void AddNewTable(ColumnHeader columnHeader)
        {

        }

        public void RemoveEntry(long key)
        {
            // allRowEntryHeader.RemoveKey(entryReference.Key);
            // foreach (var table in Tables)
            //     table.SharedData.RemoveKey(entryReference.Key);

        }
        /// <summary>
        /// Note: Called when the asset file is created.
        /// </summary>
        private void Awake()
        {
            if (ExcelSettings.Instance != null)
            {
                // excel and google plugin have its own template files,
                // so we need to set the different path when the asset file is created.
                TemplatePath = ExcelSettings.Instance.TemplatePath;
            }
        }

        /// <summary>
        /// A menu item which create a 'ExcelMachine' asset file.
        /// </summary>
        [MenuItem("Assets/Create/QuickSheet/Tools/Excel")]
        public static void CreateScriptMachineAsset()
        {
            ExcelMachine inst = ScriptableObject.CreateInstance<ExcelMachine>();
            string path = CustomAssetUtility.GetUniqueAssetPathNameOrFallback(ImportSettingFilename);
            AssetDatabase.CreateAsset(inst, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = inst;
        }
    }
}