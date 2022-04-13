
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
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

        #region 路径相关
        /// <summary>
        /// e.g. "Assets/Script/Data/Runtime/Item.cs"
        /// </summary>
        public static string TargetPathForClassScript(string worksheetName)
        {
            return Path.Combine("Assets/" + ExcelSettings.Instance.RuntimePath, worksheetName + "." + "cs");
        }

        /// <summary>
        /// e.g. "Assets/Script/Data/Editor/ItemEditor.cs"
        /// </summary>
        public static string TargetPathForEditorScript(string worksheetName)
        {
            return Path.Combine("Assets/" + ExcelSettings.Instance.EditorPath, worksheetName + "Editor" + "." + "cs");
        }

        /// <summary>
        /// data class script file has 'WorkSheetNameData' for its filename.
        /// e.g. "Assets/Script/Data/Runtime/ItemData.cs"
        /// </summary>
        public static string TargetPathForData(string worksheetName)
        {
            return Path.Combine("Assets/" + ExcelSettings.Instance.RuntimePath, worksheetName + "Data" + "." + "cs");
        }

        /// <summary>
        /// e.g. "Assets/Script/Data/Editor/ItemAssetCreator.cs"
        /// </summary>
        public static string TargetPathForAssetFileCreateFunc(string worksheetName)
        {
            return Path.Combine("Assets/" + ExcelSettings.Instance.EditorPath, worksheetName + "AssetCreator" + "." + "cs");
        }

        /// <summary>
        /// AssetPostprocessor class should be under "Editor" folder.
        /// </summary>
        public static string TargetPathForAssetPostProcessorFile(string worksheetName)
        {
            return Path.Combine("Assets/" + ExcelSettings.Instance.EditorPath, worksheetName + "AssetPostProcessor" + "." + "cs");
        }
        /// <summary>
        /// 获得目标ScriptObject存储位置
        /// </summary>
        /// <param name="worksheetName"></param>
        /// <returns></returns>
        public static string TargetPathForSOAsset(string worksheetName)
        {
            return Path.Combine("Assets/" + ExcelSettings.Instance.SaveScriptAssetFilePath, worksheetName + ".asset");
        }

        /// <summary>
        /// 获取Assets内自定义模版
        /// e.g. "Assets/QuickSheet/Templates"
        /// </summary>
        public static string GetAbsoluteCustomTemplatePath()
        {
            return Path.Combine(Application.dataPath, ExcelSettings.Instance.TemplatePath);
        }

        /// <summary>
        /// 获取Unity内建模版
        /// e.g. "C:/Program File(x86)/Unity/Editor/Data"
        /// </summary>
        public static string GetAbsoluteBuiltinTemplatePath()
        {
            return Path.Combine(EditorApplication.applicationContentsPath, ExcelSettings.Instance.TemplatePath);
        }
        #endregion
    }
}