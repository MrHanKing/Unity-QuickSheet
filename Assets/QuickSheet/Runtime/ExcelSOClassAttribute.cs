using System;

namespace UnityQuickSheet
{
    /// <summary>
    /// 导出为ScriptObject的表格类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExcelSOClassAttribute : Attribute
    {
        // public Type classDataType;
        // public ExcelSOClassAttribute(Type classDataType)
        // {
        //     this.classDataType = classDataType;
        // }
    }
}