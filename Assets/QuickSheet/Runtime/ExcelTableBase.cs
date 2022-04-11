using UnityEngine;

namespace UnityQuickSheet
{
    /// <summary>
    /// 表格运行时数据结构
    /// </summary>
    [System.Serializable]
    public class ExcelTableBase : ScriptableObject
    {
        /// <summary>
        /// 相对Asset路径的文件位置
        /// </summary>
        [HideInInspector]
        [SerializeField]
        public string SheetName = "";
        /// <summary>
        /// 使用的sheetName
        /// </summary>
        [HideInInspector]
        [SerializeField]
        public string WorksheetName = "";

        // public object[] dataArray;
    }

    [System.Serializable]
    public class ExcelTableBase<T> : ExcelTableBase
    {
        public T[] dataArray;
    }
}