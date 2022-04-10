using UnityEngine;

namespace UnityQuickSheet
{
    /// <summary>
    /// 表格运行时数据结构
    /// </summary>
    [System.Serializable]
    public class ExcelTableBase : ScriptableObject
    {
        [HideInInspector]
        [SerializeField]
        public string SheetName = "";

        [HideInInspector]
        [SerializeField]
        public string WorksheetName = "";

        public object[] dataArray;
    }

    [System.Serializable]
    public class ExcelTableBase<T> : ExcelTableBase
    {
        public new T[] dataArray;
    }
}