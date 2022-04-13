using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor;

namespace UnityQuickSheet
{
    public class EditorPrefsSaveData
    {
        private const string IS_GENERATE_SO_Key = "IsGenerateSO";
        private const string All_Excel_FilePath = "allExcelFilePath";

        /// <summary>
        /// 脚本加载完成后是否需要刷新SO数据
        /// </summary>
        /// <param name="status"></param>
        public static void SetIsGenerateSOKey(bool status)
        {
            EditorPrefs.SetBool(IS_GENERATE_SO_Key, status);
        }
        /// <summary>
        /// 是否需要生成SO对象
        /// </summary>
        /// <param name="de"></param>
        public static bool GetIsGenerateSOKey(bool de = false)
        {
            return EditorPrefs.GetBool(IS_GENERATE_SO_Key, de);
        }

        /// <summary>
        /// Excel文件夹
        /// </summary>
        /// <value></value>
        public static string AllExcelFilePath
        {
            get
            {
                return EditorPrefs.GetString(All_Excel_FilePath, "");
            }
            set
            {
                EditorPrefs.SetString(All_Excel_FilePath, value);
            }
        }
    }
}