using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor;

namespace UnityQuickSheet
{
    public class ListenScriptLoad
    {
        /// <summary>
        /// 监听脚本加载完成
        /// </summary>
        [DidReloadScripts]
        public static void Listen()
        {
            var isGenerate = EditorPrefsSaveData.GetIsGenerateSOKey();

            if (isGenerate)
            {
                Debug.Log("Try Generate SO");
                var allExcelPath = EditorPrefsSaveData.AllExcelFilePath;
                if (!string.IsNullOrWhiteSpace(EditorPrefsSaveData.AllExcelFilePath))
                {
                    var generateExcelDatas = ExcelMachineHelper.FindAllExcelData(allExcelPath);
                    ExcelMachineEditor.RefreshAllExcelSOByFind(generateExcelDatas);
                }

                EditorPrefsSaveData.SetIsGenerateSOKey(false);
            }
        }
    }
}