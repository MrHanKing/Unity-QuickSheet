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

            Debug.Log("Try Listen SO");
            var isGenerate = EditorPrefsSaveData.GetIsGenerateSOKey();
            Debug.Log("Try Listen SO" + isGenerate);
            if (isGenerate)
            {
                Debug.Log("Try Generate SO");



                EditorPrefsSaveData.SetIsGenerateSOKey(false);
            }
        }
    }
}