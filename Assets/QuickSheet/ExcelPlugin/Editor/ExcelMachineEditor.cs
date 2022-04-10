///////////////////////////////////////////////////////////////////////////////
///
/// ExcelMachineEditor.cs
///
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.Compilation;

namespace UnityQuickSheet
{
    /// <summary>
    /// Custom editor script class for excel file setting.
    /// </summary>
    [CustomEditor(typeof(ExcelMachine))]
    public class ExcelMachineEditor : BaseMachineEditor
    {
        /// <summary>
        /// 生成相关的ExcelData
        /// </summary>
        struct GenerateExcelData
        {
            public string excelPath;
            public string sheetName;
            /// <summary>
            /// sheet对应的脚本名字
            /// </summary>
            public string className;
        }

        protected readonly static string ExcelDataFilename = "Default ExcelData.asset";
        /// <summary>
        /// 脚本临时数据
        /// </summary>
        /// <typeparam name="GenerateExcelData"></typeparam>
        /// <returns></returns>
        private List<GenerateExcelData> generateExcelDatas = new List<GenerateExcelData>();
        protected override void OnEnable()
        {
            base.OnEnable();

            machine = target as ExcelMachine;
            if (machine != null && ExcelSettings.Instance != null)
            {
                if (string.IsNullOrEmpty(ExcelSettings.Instance.RuntimePath) == false)
                    machine.RuntimeClassPath = ExcelSettings.Instance.RuntimePath;
                if (string.IsNullOrEmpty(ExcelSettings.Instance.EditorPath) == false)
                    machine.EditorClassPath = ExcelSettings.Instance.EditorPath;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ExcelMachine machine = target as ExcelMachine;

            GUILayout.Label("Excel Spreadsheet Settings:", headerStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("File:", GUILayout.Width(50));

            string path = string.Empty;
            if (string.IsNullOrEmpty(machine.excelFilePath))
                path = Application.dataPath;
            else
                path = machine.excelFilePath;

            machine.excelFilePath = GUILayout.TextField(path, GUILayout.Width(250));
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                string folder = Path.GetDirectoryName(path);
#if UNITY_EDITOR_WIN
                path = EditorUtility.OpenFilePanel("Open Excel file", folder, "excel files;*.xls;*.xlsx");
#else // for UNITY_EDITOR_OSX
                path = EditorUtility.OpenFilePanel("Open Excel file", folder, "xls");
#endif
                if (path.Length != 0)
                {
                    machine.SpreadSheetName = Path.GetFileName(path);

                    // the path should be relative not absolute one to make it work on any platform.
                    int index = path.IndexOf("Assets");
                    if (index >= 0)
                    {
                        // set relative path
                        machine.excelFilePath = path.Substring(index);

                        // pass absolute path
                        machine.SheetNames = new ExcelQuery(path).GetSheetNames();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error",
                            @"Wrong folder is selected.
                        Set a folder under the 'Assets' folder! \n
                        The excel file should be anywhere under  the 'Assets' folder", "OK");
                        return;
                    }
                }
            }
            GUILayout.EndHorizontal();

            // Failed to get sheet name so we just return not to make editor on going.
            if (machine.SheetNames != null && machine.SheetNames.Length == 0)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Error: Failed to retrieve the specified excel file.");
                EditorGUILayout.LabelField("If the excel file is opened, close it then reopen it again.");
                return;
            }

            // spreadsheet name should be read-only
            EditorGUILayout.TextField("Spreadsheet File: ", machine.SpreadSheetName);

            EditorGUILayout.Space();

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Worksheet: ", GUILayout.Width(100));
                machine.CurrentSheetIndex = EditorGUILayout.Popup(machine.CurrentSheetIndex, machine.SheetNames);
                if (machine.SheetNames != null)
                    machine.WorkSheetName = machine.SheetNames[machine.CurrentSheetIndex];

                if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                {
                    // reopen the excel file e.g) new worksheet is added so need to reopen.
                    machine.SheetNames = new ExcelQuery(machine.excelFilePath).GetSheetNames();

                    // one of worksheet was removed, so reset the selected worksheet index
                    // to prevent the index out of range error.
                    if (machine.SheetNames.Length <= machine.CurrentSheetIndex)
                    {
                        machine.CurrentSheetIndex = 0;

                        string message = "Worksheet was changed. Check the 'Worksheet' and 'Update' it again if it is necessary.";
                        EditorUtility.DisplayDialog("Info", message, "OK");
                    }
                }
            }

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();

            if (machine.HasColumnHeader())
            {
                if (GUILayout.Button("Update"))
                    Import();
                if (GUILayout.Button("Reimport"))
                    Import(true);
            }
            else
            {
                if (GUILayout.Button("Import"))
                    Import();
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            DrawHeaderSetting(machine);

            EditorGUILayout.Separator();

            GUILayout.Label("Path Settings:", headerStyle);

            machine.TemplatePath = EditorGUILayout.TextField("Template: ", machine.TemplatePath);
            machine.RuntimeClassPath = EditorGUILayout.TextField("Runtime: ", machine.RuntimeClassPath);
            machine.EditorClassPath = EditorGUILayout.TextField("Editor:", machine.EditorClassPath);
            machine.SaveScriptAssetFilePath = EditorGUILayout.TextField("SOData:", machine.SaveScriptAssetFilePath);
            //machine.DataFilePath = EditorGUILayout.TextField("Data:", machine.DataFilePath);

            machine.onlyCreateDataClass = EditorGUILayout.Toggle("Only DataClass", machine.onlyCreateDataClass);

            EditorGUILayout.Separator();

            if (GUILayout.Button("Generate"))
            {
                if (string.IsNullOrEmpty(machine.SpreadSheetName) || string.IsNullOrEmpty(machine.WorkSheetName))
                {
                    Debug.LogWarning("No spreadsheet or worksheet is specified.");
                    return;
                }

                Directory.CreateDirectory(Application.dataPath + Path.DirectorySeparatorChar + machine.RuntimeClassPath);
                Directory.CreateDirectory(Application.dataPath + Path.DirectorySeparatorChar + machine.EditorClassPath);

                ScriptPrescription sp = Generate(machine);
                if (sp != null)
                {
                    Debug.Log("Successfully generated!");
                }
                else
                    Debug.LogError("Failed to create a script from excel.");
            }

            var targetDirectoryInfo = FilePathSelected();

            if (GUILayout.Button("GenerateAllClass"))
            {
                FindAllExcelData(targetDirectoryInfo);
                GenerateAllClass();
            }
            if (GUILayout.Button("GenerateAllObjectScript"))
            {
                CreateAllExcelSOByFind();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(machine);
            }
        }

        /// <summary>
        /// 文件路径选择器
        /// </summary>
        /// <returns></returns>
        private DirectoryInfo FilePathSelected()
        {
            DirectoryInfo targetDirectory = null;
            //选择所有表格所在路径
            GUILayout.BeginHorizontal();
            GUILayout.Label("File:", GUILayout.Width(50));
            ExcelMachine machine = target as ExcelMachine;
            string path = string.Empty;
            if (string.IsNullOrEmpty(machine.allExcelFilePath))
            {
                path = Application.dataPath;
                targetDirectory = new DirectoryInfo(path);
            }
            else
            {
                path = machine.allExcelFilePath;
                var excelsPath = Path.GetFullPath(path);
                targetDirectory = new DirectoryInfo(excelsPath);
            }

            var with = Mathf.Max(250, path.Length * 10);
            machine.allExcelFilePath = GUILayout.TextField(path, GUILayout.Width(with));

            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                string folder = Path.GetDirectoryName(path);
#if UNITY_EDITOR_WIN
                path = EditorUtility.OpenFolderPanel("Open Excel file", folder, "excel files;*.xls;*.xlsx");
#else // for UNITY_EDITOR_OSX
                path = EditorUtility.OpenFolderPanel("Open Excel file", folder, "xls");
#endif
                Debug.Log(path);

                // 获得相对路径储存
                var absolutePath = GetProjectAbsolutePath();
                machine.allExcelFilePath = PathHelper.RelativePath(absolutePath, path);
                Debug.Log("aaa:" + machine.allExcelFilePath);

                // 收集所有表格
                targetDirectory = new DirectoryInfo(path);
            }
            GUILayout.EndHorizontal();

            return targetDirectory;
        }

        private string GetProjectAbsolutePath()
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
        private List<FileInfo> FindAllExcels(DirectoryInfo directory)
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

        private GenerateExcelData FindGenerateExcelByOSClassName(string OSClassName)
        {
            foreach (var generateData in this.generateExcelDatas)
            {
                if (generateData.className == OSClassName)
                {
                    return generateData;
                }
            }
            return new GenerateExcelData();
        }

        private List<GenerateExcelData> FindAllExcelData(DirectoryInfo directory)
        {
            var files = FindAllExcels(directory);
            var assetPath = GetProjectAbsolutePath();
            foreach (var file in files)
            {
                var sheetNames = new ExcelQuery(file.FullName).GetSheetNames();
                this.generateExcelDatas.Add(new GenerateExcelData()
                {
                    excelPath = file.FullName,
                    sheetName = sheetNames[0],
                    className = GetExcelClassName(sheetNames[0]),
                });
            }
            return this.generateExcelDatas;
        }
        private void GenerateAllClass()
        {
            if (machine is ExcelMachine target)
            {
                // check 文件夹
                string pathRuntime = Application.dataPath + Path.DirectorySeparatorChar + machine.RuntimeClassPath;
                string pathEditor = Application.dataPath + Path.DirectorySeparatorChar + machine.EditorClassPath;
                if (!Directory.Exists(pathRuntime))
                {
                    Directory.CreateDirectory(pathRuntime);
                }
                if (!Directory.Exists(pathEditor))
                {
                    Directory.CreateDirectory(pathEditor);
                }

                foreach (var excelData in this.generateExcelDatas)
                {
                    // Todo 优化
                    target.SpreadSheetName = Path.GetFileName(excelData.excelPath);
                    // pass absolute path
                    target.SheetNames = new ExcelQuery(excelData.excelPath).GetSheetNames();
                    // 默认第一个sheet
                    target.WorkSheetName = target.SheetNames[0];

                    ScriptPrescription sp = Generate(machine);
                    if (sp != null)
                    {
                        Debug.Log("Successfully generated!");
                    }
                    else
                        Debug.LogError($"Failed to create a script from excel :{excelData.excelPath}");
                }
            }
        }



        /// <summary>
        /// Import the specified excel file and prepare to set type of each cell.
        /// </summary>
        protected override void Import(bool reimport = false)
        {
            ExcelMachine machine = target as ExcelMachine;

            string path = machine.excelFilePath;
            string sheet = machine.WorkSheetName;

            if (string.IsNullOrEmpty(path))
            {
                string msg = "You should specify spreadsheet file first!";
                EditorUtility.DisplayDialog("Error", msg, "OK");
                return;
            }

            if (!File.Exists(path))
            {
                string msg = string.Format("File at {0} does not exist.", path);
                EditorUtility.DisplayDialog("Error", msg, "OK");
                return;
            }

            int startRowIndex = 0;
            string error = string.Empty;
            var titles = new ExcelQuery(path, sheet).GetTitle(startRowIndex, ref error);
            if (titles == null || !string.IsNullOrEmpty(error))
            {
                EditorUtility.DisplayDialog("Error", error, "OK");
                return;
            }
            else
            {
                // check the column header is valid
                foreach (string column in titles)
                {
                    if (!IsValidHeader(column))
                    {
                        error = string.Format(@"Invalid column header name {0}. Any c# keyword should not be used for column header. Note it is not case sensitive.", column);
                        EditorUtility.DisplayDialog("Error", error, "OK");
                        return;
                    }
                }
            }

            List<string> titleList = titles.ToList();

            if (machine.HasColumnHeader() && reimport == false)
            {
                var headerDic = machine.ColumnHeaderList.ToDictionary(header => header.name);

                // collect non-changed column headers
                var exist = titleList.Select(t => GetColumnHeaderString(t))
                    .Where(e => headerDic.ContainsKey(e) == true)
                    .Select(t => new ColumnHeader { name = t, type = headerDic[t].type, isArray = headerDic[t].isArray, OrderNO = headerDic[t].OrderNO });


                // collect newly added or changed column headers
                var changed = titleList.Select(t => GetColumnHeaderString(t))
                    .Where(e => headerDic.ContainsKey(e) == false)
                    .Select(t => ParseColumnHeader(t, titleList.IndexOf(t)));

                // merge two list via LINQ
                var merged = exist.Union(changed).OrderBy(x => x.OrderNO);

                machine.ColumnHeaderList.Clear();
                machine.ColumnHeaderList = merged.ToList();
            }
            else
            {
                machine.ColumnHeaderList.Clear();
                if (titleList.Count > 0)
                {
                    int order = 0;
                    machine.ColumnHeaderList = titleList.Select(e => ParseColumnHeader(e, order++)).ToList();
                }
                else
                {
                    string msg = string.Format("An empty workhheet: [{0}] ", sheet);
                    Debug.LogWarning(msg);
                }
            }

            EditorUtility.SetDirty(machine);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Generate AssetPostprocessor editor script file.
        /// </summary>
        protected override void CreateAssetCreationScript(BaseMachine m, ScriptPrescription sp)
        {
            ExcelMachine machine = target as ExcelMachine;

            sp.className = machine.WorkSheetName;
            sp.dataClassName = machine.WorkSheetName + "Data";
            sp.worksheetClassName = machine.WorkSheetName;

            // where the imported excel file is.
            sp.importedFilePath = machine.excelFilePath;

            // path where the .asset file will be created.
            string path = Path.GetDirectoryName(machine.excelFilePath);
            path += "/" + machine.WorkSheetName + ".asset";
            sp.assetFilepath = path.Replace('\\', '/');
            sp.assetPostprocessorClass = machine.WorkSheetName + "AssetPostprocessor";
            sp.template = GetTemplate("PostProcessor");

            // write a script to the given folder.
            using (var writer = new StreamWriter(TargetPathForAssetPostProcessorFile(machine.WorkSheetName)))
            {
                writer.Write(new ScriptGenerator(sp).ToString());
                writer.Close();
            }
        }

        /// <summary>
        /// 自行搜索脚本创建所有表格ScriptObject对象
        /// </summary>
        public void CreateAllExcelSOByFind()
        {
            var allType = FindAllScriptTypeByAttribute<ExcelSOClassAttribute>();
            Debug.Log($"allTypeLength{allType.Count}");
            ScriptableObject inst = null;
            foreach (var oneType in allType)
            {
                if (oneType.IsGenericTypeDefinition)
                {
                    continue;
                }

                if (AIsExtendsByB(oneType, typeof(ScriptableObject)))
                {
                    var resName = oneType.Name;
                    string path = TargetPathForSOAsset(resName);
                    ExcelTableBase data = (ExcelTableBase)AssetDatabase.LoadAssetAtPath(path, oneType);
                    if (data == null)
                    {
                        inst = data = ScriptableObject.CreateInstance(oneType) as ExcelTableBase;
                        AssetDatabase.CreateAsset(inst, path);
                    }

                    var config = FindGenerateExcelByOSClassName(resName);

                    ExcelQuery query = new ExcelQuery(config.excelPath, config.sheetName);
                    if (query != null && query.IsValid())
                    {
                        var args = oneType.BaseType.GetGenericArguments();
                        if (args.Length == 1)
                        {
                            var dataType = args[0];
                            data.dataArray = query.Deserialize(dataType).ToArray();
                            EditorUtility.SetDirty(data);
                        }
                        else
                        {
                            Debug.LogError($"{resName}没有找到对应的表格数据类型");
                        }
                    }

                }
            }

            if (inst != null)
            {
                AssetDatabase.SaveAssets();
                Selection.activeObject = inst;
            }
        }


        /// <summary>
        /// a 是否继承自 b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool AIsExtendsByB(Type a, Type b)
        {
            var baseType = a.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == b.Name)
                {
                    return true;
                }
                else
                {
                    baseType = baseType.BaseType;
                }
            }

            return false;
        }

        /// <summary>
        /// 拿到所有的表格数据脚本类
        /// </summary>
        public List<Type> FindAllScriptTypeByAttribute<T>() where T : Attribute
        {
            List<Type> result = new List<Type>();

            var types = System.Reflection.Assembly.Load("Assembly-CSharp").GetTypes();
            var targetT = typeof(T);
            foreach (var type in types)
            {
                var attrs = type.GetCustomAttributes(targetT, false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                result.Add(type);
            }

            return result;
        }

        /// <summary>
        /// 查找对应Unity Assembly
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static UnityEditor.Compilation.Assembly FindAssembly(string assemblyName)
        {
            UnityEngine.Debug.Log("== Player Assemblies ==");
            UnityEditor.Compilation.Assembly[] playerAssemblies =
                CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);

            foreach (var assembly in playerAssemblies)
            {
                if (assembly.name == assemblyName)
                {
                    return assembly;
                }
            }
            return null;
        }
    }
}