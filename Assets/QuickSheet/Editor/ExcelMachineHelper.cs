
using System.Collections.Generic;
using System.IO;
using static UnityQuickSheet.ExcelMachineEditor;

namespace UnityQuickSheet
{
    /// <summary>
    /// 帮助类
    /// </summary>
    public class ExcelMachineHelper
    {
        /// <summary>
        /// 获得OS数据结构的类名
        /// </summary>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static string GetDataClassName(string sheetName)
        {
            return $"{sheetName}Data";
        }
        public static string GetEditorClassName(string sheetName)
        {
            return $"{sheetName}Editor";
        }
        public static string GetExcelClassName(string sheetName)
        {
            return sheetName;
        }
        /// <summary>
        /// 获得项目路径
        /// </summary>
        /// <returns></returns>
        public static string GetProjectAbsolutePath()
        {
            // 获得相对路径储存
            string comparePath = "./";

            var absolutePath = new DirectoryInfo(comparePath).FullName;
            return absolutePath;
        }

        /// <summary>
        /// 找到所有Excel
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static List<FileInfo> FindAllExcels(DirectoryInfo directory)
        {
#if UNITY_EDITOR_WIN
            string[] _patterns = new string[] { "*.xls", "*.xlsx"};//识别不同的后缀名
#else
            string[] _patterns = new string[] { "*.xls" };//识别不同的后缀名
#endif
            List<FileInfo> _allFilePaths = new List<FileInfo>();
            foreach (var pattern in _patterns)
            {
                FileInfo[] _temp = directory.GetFiles(pattern, SearchOption.AllDirectories);
                _allFilePaths.AddRange(_temp);
            }
            return _allFilePaths;
        }

        /// <summary>
        /// 查找所有excel数据
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static List<GenerateExcelData> FindAllExcelData(string allExcelFilePath)
        {
            var result = new List<GenerateExcelData>();
            var directory = new DirectoryInfo(Path.GetFullPath(allExcelFilePath));

            var files = FindAllExcels(directory);
            var assetPath = GetProjectAbsolutePath();
            foreach (var file in files)
            {
                var sheetNames = new ExcelQuery(file.FullName).GetSheetNames();
                result.Add(new GenerateExcelData()
                {
                    excelPath = file.FullName,
                    sheetName = sheetNames[0],
                    className = GetExcelClassName(sheetNames[0]),
                });
            }
            return result;
        }
    }
}