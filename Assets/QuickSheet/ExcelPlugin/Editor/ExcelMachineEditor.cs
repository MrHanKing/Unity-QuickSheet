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
        private ExcelMachine machine1;
        protected override void OnEnable()
        {
            base.OnEnable();

            machine = target as ExcelMachine;
            machine1 = target as ExcelMachine;
            if (machine != null && ExcelSettings.Instance != null)
            {
                if (string.IsNullOrEmpty(ExcelSettings.Instance.RuntimePath) == false)
                    machine.RuntimeClassPath = ExcelSettings.Instance.RuntimePath;
                if (string.IsNullOrEmpty(ExcelSettings.Instance.EditorPath) == false)
                    machine.EditorClassPath = ExcelSettings.Instance.EditorPath;
            }
        }

        /// <summary>
        /// 选择单张表的路径
        /// </summary>
        private void DrawOneExcelSelected()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("File:", GUILayout.Width(50));

            string path = string.Empty;
            if (string.IsNullOrEmpty(machine1.excelFilePath))
                path = Application.dataPath;
            else
                path = machine1.excelFilePath;

            machine1.excelFilePath = GUILayout.TextField(path, GUILayout.Width(250));
            var folderSelect = SelectFolderButton(path);
            if (!string.IsNullOrWhiteSpace(folderSelect))
            {
                machine.SpreadSheetName = Path.GetFileName(path);
                var absolutePath = Path.GetFullPath(path);

                // set relative path
                machine1.excelFilePath = PathHelper.RelativePath(Application.dataPath, absolutePath);

                // pass absolute path
                machine1.SheetNames = new ExcelQuery(absolutePath).GetSheetNames();
            }

            GUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ExcelMachine machine = target as ExcelMachine;

            GUILayout.Label("Excel Spreadsheet Settings:", headerStyle);

            DrawOneExcelSelected();

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

            // 选择Sheet
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Worksheet: ", GUILayout.Width(100));
                machine.CurrentSheetIndex = EditorGUILayout.Popup(machine.CurrentSheetIndex, machine.SheetNames);
                if (machine.SheetNames != null)
                    machine.WorkSheetName = machine.SheetNames[machine.CurrentSheetIndex];

                if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                {
                    // reopen the excel file e.g) new worksheet is added so need to reopen.
                    machine.SheetNames = new ExcelQuery(Path.GetFullPath(machine.excelFilePath)).GetSheetNames();

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

            FilePathSelected();

            if (GUILayout.Button("GenerateAllClass"))
            {
                FindAllExcelData();
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
        /// <returns>Excel的相对路径</returns>
        private string FilePathSelected()
        {
            //选择所有表格所在路径
            GUILayout.BeginHorizontal();
            GUILayout.Label("全表格所在目录:", GUILayout.Width(50));
            ExcelMachine machine = target as ExcelMachine;
            string path = string.Empty;
            if (string.IsNullOrEmpty(machine.allExcelFilePath))
            {
                var absolutePath = GetProjectAbsolutePath();
                path = Application.dataPath;
                path = PathHelper.RelativePath(absolutePath, path);
            }
            else
            {
                path = machine.allExcelFilePath;
            }

            var with = Mathf.Max(250, path.Length * 10);
            machine.allExcelFilePath = GUILayout.TextField(path, GUILayout.Width(with));
            var openPath = Path.GetFullPath(path);
            var resultStr = SelectFolderButton(openPath);
            if (!string.IsNullOrWhiteSpace(resultStr))
            {
                // 获得相对路径储存
                var absolutePath = GetProjectAbsolutePath();
                machine.allExcelFilePath = PathHelper.RelativePath(absolutePath, resultStr);
                Debug.Log("选择Excel文件夹:" + machine.allExcelFilePath);
            }

            GUILayout.EndHorizontal();

            return machine.allExcelFilePath;
        }

        /// <summary>
        /// 选择路径按钮
        /// </summary>
        /// <returns></returns>
        private string SelectFolderButton(string openPath)
        {
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                string folder = Path.GetDirectoryName(openPath);
#if UNITY_EDITOR_WIN
                folder = EditorUtility.OpenFolderPanel("Open Excel file", folder, "excel files;*.xls;*.xlsx");
#else // for UNITY_EDITOR_OSX
                folder = EditorUtility.OpenFolderPanel("Open Excel file", folder, "xls");
#endif
                return folder;
            }
            return default;
        }

        private DirectoryInfo GetAllExcelPathDirectory()
        {
            // 拿到的是相对路径
            var path = (target as ExcelMachine).allExcelFilePath;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("请选择好Excel的文件路径");
                return null;
            }
            var excelsPath = Path.GetFullPath(path);
            var targetDirectory = new DirectoryInfo(excelsPath);

            return targetDirectory;
        }

        /// <summary>
        /// 获得项目路径
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 查找数据
        /// </summary>
        /// <param name="OSClassName"></param>
        /// <returns></returns>
        private GenerateExcelData FindGenerateExcelByOSClassName(string OSClassName)
        {
            // check
            if (this.generateExcelDatas.Count <= 0)
            {
                this.FindAllExcelData();
            }

            foreach (var generateData in this.generateExcelDatas)
            {
                if (generateData.className == OSClassName)
                {
                    return generateData;
                }
            }
            return new GenerateExcelData();
        }
        /// <summary>
        /// 查找所有excel数据
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private List<GenerateExcelData> FindAllExcelData()
        {
            var directory = new DirectoryInfo(Path.GetFullPath((target as ExcelMachine).allExcelFilePath));

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
                    var excelQ = new ExcelQuery(excelData.excelPath);
                    target.SpreadSheetName = Path.GetFileName(excelData.excelPath);
                    // pass absolute path
                    target.SheetNames = excelQ.GetSheetNames();
                    // 默认第一个sheet
                    target.WorkSheetName = target.SheetNames[0];
                    // 刷新列信息
                    var titleList = GetExcelTitles(excelQ.ChangeSheet(target.WorkSheetName));
                    target.ColumnHeaderList = ReCountColumnHeader(titleList, target.WorkSheetName);

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

            var titleList = GetExcelTitles(new ExcelQuery(path, sheet));

            if (machine.HasColumnHeader() && reimport == false)
            {
                var headerDic = machine.ColumnHeaderList.ToDictionary(header => header.name);
                var reCountHeader = ReCountColumnHeader(headerDic, titleList);

                machine.ColumnHeaderList.Clear();
                machine.ColumnHeaderList = reCountHeader;
            }
            else
            {
                machine.ColumnHeaderList.Clear();
                machine.ColumnHeaderList = ReCountColumnHeader(titleList, sheet);
            }

            EditorUtility.SetDirty(machine);
            AssetDatabase.SaveAssets();
        }

        public List<ColumnHeader> ReCountColumnHeader(List<string> newTitleList, string sheetName)
        {
            if (newTitleList.Count > 0)
            {
                int order = 0;
                return newTitleList.Select(e => ParseColumnHeader(e, order++)).ToList();
            }
            else
            {
                string msg = string.Format("An empty workhheet: [{0}] ", sheetName);
                Debug.LogWarning(msg);
            }
            return default;
        }

        /// <summary>
        /// 重新计算表格列头数据
        /// </summary>
        /// <param name="oldHeaderDic">老的头数据</param>
        /// <param name="newTitleList">新的头列表string</param>
        /// <returns></returns>
        public List<ColumnHeader> ReCountColumnHeader(Dictionary<string, ColumnHeader> oldHeaderDic, List<string> newTitleList)
        {
            // 老的没变的列
            var exist = newTitleList.Select(t => GetColumnHeaderString(t))
                .Where(e => oldHeaderDic.ContainsKey(e) == true)
                .Select(t => new ColumnHeader { name = t, type = oldHeaderDic[t].type, isArray = oldHeaderDic[t].isArray, OrderNO = oldHeaderDic[t].OrderNO });


            // 新增加的列
            var changed = newTitleList.Select(t => GetColumnHeaderString(t))
                .Where(e => oldHeaderDic.ContainsKey(e) == false)
                .Select(t => ParseColumnHeader(t, newTitleList.IndexOf(t)));

            // merge two list via LINQ
            var merged = exist.Union(changed).OrderBy(x => x.OrderNO);
            return merged.ToList();
        }

        /// <summary>
        /// 获取excel的表头
        /// </summary>
        /// <param name="excelQuery">excel表数据</param>
        /// <param name="titleRowIndex">header所在行</param>
        /// <returns></returns>
        private List<string> GetExcelTitles(ExcelQuery excelQuery, int titleRowIndex = 0)
        {
            string error = string.Empty;
            var titles = excelQuery.GetTitle(titleRowIndex, ref error);
            if (titles == null || !string.IsNullOrEmpty(error))
            {
                EditorUtility.DisplayDialog("Error", error, "OK");
                return default;
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
                        return default;
                    }
                }
            }

            List<string> titleList = titles.ToList();
            return titleList;
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
                        inst = data = (ExcelTableBase)ScriptableObject.CreateInstance(oneType);
                        AssetDatabase.CreateAsset(inst, path);
                    }

                    var config = FindGenerateExcelByOSClassName(resName);

                    ExcelQuery query = new ExcelQuery(config.excelPath, config.sheetName);
                    if (query != null && query.IsValid())
                    {
                        // 设置数据
                        data.SheetName = config.excelPath;
                        data.WorksheetName = config.sheetName;

                        FieldInfo pc = oneType.GetField("dataArray");
                        var args = oneType.BaseType.GetGenericArguments();
                        if (args.Length == 1 && pc.FieldType.IsArray)
                        {
                            var dataType = args[0];
                            // 显式转换类型
                            var targetListData = query.Deserialize(dataType);
                            var targetData = targetListData.GetType().GetMethod("ToArray").Invoke(targetListData, new object[] { });

                            pc.SetValue(data, targetData);
                            // data.dataArray = query.Deserialize(dataType).ToArray();
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