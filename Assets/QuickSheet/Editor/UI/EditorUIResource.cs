using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExcelEditor.Tool
{
    public static class EditorUIResources
    {
        const string k_TemplateRoot = "Assets/QuickSheet/Editor/UI/Templates";
        const string k_StyleRoot = "Assets/QuickSheet/Editor/UI/Templates";

        public static string GetStyleSheetPath(string filename) => $"{k_StyleRoot}/{filename}.uss";

        static string TemplatePath(string filename) => $"{k_TemplateRoot}/{filename}.uxml";
        /// <summary>
        /// 获得UI模版资源
        /// </summary>
        /// <param name="templateFilename"></param>
        /// <returns></returns>
        public static VisualTreeAsset GetTemplateAsset(string templateFilename)
        {
            var path = TemplatePath(templateFilename);

            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);

            if (asset == null)
                throw new FileNotFoundException("没有找到对应UI模版 " + path);
            return asset;
        }
        /// <summary>
        /// 获得UI模版实例
        /// </summary>
        /// <param name="templateFilename"></param>
        /// <returns></returns>
        public static VisualElement GetTemplate(string templateFilename)
        {
            return GetTemplateAsset(templateFilename).CloneTree();
        }
        /// <summary>
        /// 获得样式资源
        /// </summary>
        /// <param name="styleSheetFilename"></param>
        /// <returns></returns>
        public static StyleSheet GetStyleSheetAsset(string styleSheetFilename)
        {
            var path = GetStyleSheetPath(styleSheetFilename);
            var asset = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);

            if (asset == null)
                throw new FileNotFoundException("没有找到对应UI的样式 " + path);
            return asset;
        }
    }

}