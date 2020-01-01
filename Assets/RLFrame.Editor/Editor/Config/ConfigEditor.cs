using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Text;
using OfficeOpenXml;
using Object = UnityEngine.Object;
using static ReflectionExtensions;
using Debug = UnityEngine.Debug;
using static PathDefineEditor;

public class ConfigEditor
{
    #region base

    

    [MenuItem("Assets/.cs/生成xml")]
    public static void AssetsClassGenXml()
    {
        Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("类转xml", $"正在执行{objs[i].name}...", 1.0f / objs.Length);
            ClassGenXmlProcess(objs[i].name);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    private static void ClassGenXmlProcess(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        try
        {
            Type type = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(name);
                if (type != null)
                {
                    break;
                }
            }

            if (type != null)
            {
                var instance = Activator.CreateInstance(type);
                if (instance is ConfigDataBase)
                {
                    (instance as ConfigDataBase).Construction();
                }

                string xmlPath = XmlFullPath(name);
                SerializeOption.XmlSerialize(xmlPath, instance);
                Debug.Log($"{name} to xml done : {xmlPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    [MenuItem("Assets/.xml/生成二进制文件")]
    public static void AssetsXmlGenBinary()
    {
        Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("xml转binary", $"正在执行{objs[i].name}...", 1.0f / objs.Length);
            XmlGenBinaryProcess(objs[i].name);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/配置/全体xml生成二进制")]
    public static void AssetsXmlGenBinaryUnite()
    {
        Object[] objs = Selection.objects;
        string xmlPath = XmlLocalPath();
        string[] filesPath = Directory.GetFiles(xmlPath, "*", SearchOption.AllDirectories);

        for (int i = 0; i < filesPath.Length; i++)
        {
            var filePath = filesPath[i];
            EditorUtility.DisplayProgressBar("查找文件夹下的xml", $"正在扫描{filePath}...", 1.0f / objs.Length);
            if (filePath.EndsWith(".xml"))
            {
                string name = filePath.Substring(filePath.LastIndexOf(@"\") + 1).Replace(".xml", "");
                XmlGenBinaryProcess(name);
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    private static void XmlGenBinaryProcess(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        try
        {
            Type type = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(name);
                if (type != null) break;
            }

            if (type != null)
            {
                string xmlPath = XmlFullPath(name);
                System.Object obj = SerializeOption.XmlDeserializeEditor(xmlPath, type);
                string binaryPath = BinaryFullPath(name);
                SerializeOption.BinarySerialize(binaryPath, obj);
                Debug.Log($"Xml {name} to binary done : {binaryPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    [MenuItem("Assets/.xml/生成Excel")]
    public static void AssetsXmlGenExcel()
    {
        Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("xml生成Excel", $"正在生成{objs[i].name}的excel...", 1.0f / objs.Length);
            XmlGenExcelProcess(objs[i].name);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    #endregion

    #region genexcel

    

    [MenuItem("Tools/配置/全体xml生成Excel")]
    public static void AssetsXmlGenExcelUnite()
    {
        Object[] objs = Selection.objects;
        string xmlPath = XmlLocalPath();
        string[] filesPath = Directory.GetFiles(xmlPath, "*", SearchOption.AllDirectories);

        for (int i = 0; i < filesPath.Length; i++)
        {
            var filePath = filesPath[i];
            EditorUtility.DisplayProgressBar("xml生成Excel", $"正在生成{filePath}的Excel...", 1.0f / objs.Length);
            if (filePath.EndsWith(".xml"))
            {
                string name = filePath.Substring(filePath.LastIndexOf(@"\") + 1).Replace(".xml", "");
                XmlGenExcelProcess(name);
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    private static void XmlGenExcelProcess(string name)
    {
        string className = null;
        string xmlName = null;
        string excelName = null;
        //get  sheet struc
        Dictionary<string, Sheet> sheetDic = ReadSheet(name, ref excelName, ref xmlName, ref className);

        object data = GetInstanceFromXml(className);

        List<Sheet> outermostSheets = new List<Sheet>();
        foreach (var sheet in sheetDic.Values)
        {
            if (sheet.Depth == 1)
            {
                outermostSheets.Add(sheet);
            }
        }

        Dictionary<string, SheetData> sheetDataDic = new Dictionary<string, SheetData>();
        foreach (var t in outermostSheets)
        {
            WriteSheetData(data, t, sheetDic, sheetDataDic);
        }

        string xlslPath = ExcelFullPath(excelName);
        if (IsFileOccupied(xlslPath))
        {
            Debug.LogError($"File {excelName} is already be occupied, please close and try again.");

            return;
        }

        try
        {
            FileInfo xlslInfo = new FileInfo(xlslPath);
            if (xlslInfo.Exists)
            {
                xlslInfo.Delete();
                xlslInfo = new FileInfo(xlslPath);
            }

            using (ExcelPackage excelPackage = new ExcelPackage(xlslInfo))
            {
                foreach (string key in sheetDataDic.Keys)
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add(key);
                    SheetData sheetData = sheetDataDic[key];
                    for (int i = 0; i < sheetData.Names.Count; i++)
                    {
                        var range = worksheet.Cells[1, i + 1];
                        range.Value = sheetData.Names[i];
                    }

                    for (int i = 0; i < sheetData.Datas.Count; i++) //->>>行
                    {
                        RowData rowData = sheetData.Datas[i];
                        for (int j = 0; j < sheetData.Datas[i].Dic.Count; j++) //->>>列
                        {
                            var range = worksheet.Cells[i + 2, j + 1];
                            var value = rowData.Dic[sheetData.Names[j]];
                            range.Value = value;
                            range.AutoFitColumns();
                            if (value.Contains("\n") || value.Contains("\r"))
                            {
                                range.Style.WrapText = true;
                            }
                        }
                    }
                }

                excelPackage.Save();
                Debug.Log($"生成{xlslPath}成功");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    #region genexcel.tools

    private static object GetInstanceFromXml(string name)
    {
        object instance = null;
        Type type = GetTypeByClassName(name);
        if (type != null)
        {
            string xmlPath = XmlFullPath(name);
            instance = SerializeOption.XmlDeserializeEditor(xmlPath, type);
        }

        return instance;
    }


    private static void ReadRegElement(XmlElement element, Dictionary<string, Sheet> sheetDic, int depth)
    {
        depth++;
        foreach (XmlNode xmlNode in element.ChildNodes)
        {
            var parentElement = xmlNode as XmlElement;
            if (parentElement.GetAttribute("type").Equals("list"))
            {
                XmlElement list = parentElement.FirstChild as XmlElement;
                Variable parent = new Variable()
                {
                    Name = parentElement.GetAttribute("name"),
                    Type = parentElement.GetAttribute("type"),

                    Col = parentElement.GetAttribute("col"), //?
                    Default = parentElement.GetAttribute("default"),
                    Foreign = parentElement.GetAttribute("foreign"),
                    Split = parentElement.GetAttribute("split"),

                    ListClassName = list.GetAttribute("name"),
                    ListSheetName = list.GetAttribute("sheetname")
                };
                Sheet sheet = new Sheet()
                {
                    Parent = parent,
                    ClassName = list.GetAttribute("name"),
                    SheetName = list.GetAttribute("sheetname"),
                    MainKey = list.GetAttribute("mainkey"),
                    Split = list.GetAttribute("split"),
                    Variables = new List<Variable>(),
                    Depth = depth
                };
                if (!string.IsNullOrEmpty(sheet.SheetName))
                {
                    if (!sheetDic.ContainsKey(sheet.SheetName))
                    {
                        foreach (var childNode in list.ChildNodes)
                        {
                            var childElement = childNode as XmlElement;
                            Variable childVariable = new Variable()
                            {
                                Name = childElement.GetAttribute("name"),
                                Type = childElement.GetAttribute("type"),
                                Col = childElement.GetAttribute("col"),
                                Default = childElement.GetAttribute("default"),
                                Foreign = childElement.GetAttribute("foreign"),
                                Split = childElement.GetAttribute("split")
                            };
                            if (childVariable.Type == "list")
                            {
                                var childList = childElement.FirstChild as XmlElement;
                                childVariable.ListClassName = childList.GetAttribute("name");
                                childVariable.ListSheetName = childList.GetAttribute("sheetname");
                                ReadRegElement(childList, sheetDic, depth);
                            }

                            sheet.Variables.Add(childVariable);
                        }

                        sheetDic.Add(sheet.SheetName, sheet);
                    }
                }

                ReadRegElement(list, sheetDic, depth);
            }
        }
    }

    //todo ???? xml(数据保持)->reg(sheet)->sheetdata->excel
    //data->>sheetdata
    private static void WriteSheetData(object data, Sheet sheet, Dictionary<string, Sheet> sheetDic,
        Dictionary<string, SheetData> sheetDataDic, string mainKey = null)
    {
        List<Variable> variables = sheet.Variables;
        var parent = sheet.Parent;
        object sheetDataList =
            data.GetMemberValue(parent.Name);
        int listCount = sheetDataList.GetListCount();
        SheetData sheetData = new SheetData();
        if (!string.IsNullOrEmpty(parent.Foreign))
        {
            sheetData.Names.Add(parent.Foreign);
            sheetData.Types.Add(parent.Type);
        }

        for (int i = 0; i < variables.Count; i++)
        {
            var variable = variables[i];
            if (!string.IsNullOrEmpty(variable.Col))
            {
                sheetData.Names.Add(variable.Col);
                sheetData.Types.Add(variable.Type);
            }
        }

        for (int i = 0; i < listCount; i++) //--->一条数据
        {
            object sheetItem = sheetDataList.GetListItemValue(i);
            var rowData = new RowData();
            if (!string.IsNullOrEmpty(parent.Foreign))
            {
                rowData.Dic.Add(parent.Foreign, mainKey);
            }

            if (!string.IsNullOrEmpty(sheet.MainKey))
            {
                mainKey = sheetItem.GetMemberValue(sheet.MainKey).ToString();
            }

            for (int j = 0; j < variables.Count; j++) //每条数据里的每个变量
            {
                var variable = variables[j];
                if (variable.Type == "list" && string.IsNullOrEmpty(variable.Split)) // todo->>>sheetname 外层表
                {
                    Sheet innerSheet = sheetDic[variable.ListSheetName];
                    WriteSheetData(sheetItem, innerSheet, sheetDic, sheetDataDic, mainKey);
                }
                else if (string.Equals(variable.Type, "list"))
                {
                    string block = PackSheetBlock(sheetItem, variable, sheetDic);
                    rowData.Dic.Add(variable.Col, block);
                }
                else if (variable.Type.Equals("stringblock") || variable.Type.Equals("floatblock") ||
                         variable.Type.Equals("intblock") || variable.Type.Equals("boolblock"))
                {
                    string block = PackBlock(sheetItem, variable);
                    rowData.Dic.Add(variable.Col, block);
                }
                else
                {
                    var value = sheetItem.GetMemberValue(variable.Name);
                    if (value != null)
                    {
                        rowData.Dic.Add(variable.Col, value.ToString());
                    }
                    else
                    {
                        Debug.LogError($"Can not get member value : {variable.Name},check your reg.");
                    }
                }
            }

            if (sheetDataDic.ContainsKey(sheet.SheetName))
            {
                sheetDataDic[sheet.SheetName].Datas.Add(rowData);
            }
            else
            {
                sheetData.Datas.Add(rowData);
                sheetDataDic.Add(sheet.SheetName, sheetData);
            }
        }
    }

    /// <summary>
    /// 基本数据类型链表
    /// </summary>
    /// <param name="data"></param>
    /// <param name="variable"></param>
    /// <returns></returns>
    private static string PackBlock(object data, Variable variable)
    {
        string split = variable.Split;
        if (string.IsNullOrEmpty(split))
        {
            Debug.LogError("split is isNullOrEmpty");
        }

        StringBuilder content = new StringBuilder();
        object block = data.GetMemberValue(variable.Name);
        var blockListCount = block.GetListCount();
        for (int i = 0; i < blockListCount; i++)
        {
            content.Append(block.GetListItemValue(i));
            content.Append(split);
        }

        content.Remove(content.Length - 1, 1);
        return content.ToString();
    }

    /// <summary>
    /// 数据较少的类列表
    /// </summary>
    /// <param name="data"></param>
    /// <param name="variable"></param>
    /// <param name="sheetDic"></param>
    /// <returns></returns>
    private static string PackSheetBlock(object data, Variable variable, Dictionary<string, Sheet> sheetDic)
    {
        if (!sheetDic.TryGetValue(variable.ListSheetName, out Sheet sheet))
        {
            Debug.LogError($"Can not Find shhet :{variable.ListSheetName}");

            return null;
        }

        string splitOuter = variable.Split;
        string splitInner = sheet.Split;
        if (string.IsNullOrEmpty(splitOuter) || string.IsNullOrEmpty(splitInner))
        {
            Debug.LogError("split is isNullOrEmpty");
            return null;
        }

        object block = data.GetMemberValue(variable.Name);

        StringBuilder content = new StringBuilder();
        var listCount = block.GetListCount();
        for (int i = 0; i < listCount; i++)
        {
            var item = block.GetListItemValue(i);
            for (int j = 0; j < sheet.Variables.Count; j++)
            {
                content.Append(item.GetMemberValue(sheet.Variables[j].Name));
                content.Append(splitInner);
            }

            content.Remove(content.Length - 1, 1);
            if (i != sheet.Variables.Count - 1)
                content.Append(splitOuter);
        }

        return content.ToString();
    }
    #endregion


    #endregion

    #region genxml

    

    [MenuItem("Tools/配置/全体Excel生成数据")]
    public static void AssetsExcelGenXmlUnite()
    {
        string[] files = Directory.GetFiles(RegLocalPath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            if (!file.EndsWith(".xml"))
                continue;
            EditorUtility.DisplayProgressBar("正在生成xml", $"正在处理{file}...", i * 1.0f / files.Length);
            string name = file.Substring(file.LastIndexOf(@"\") + 1).Replace(".reg.xml","");
            ExcelGenXmlProcess(name);
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    private static void ExcelGenXmlProcess(string name)
    {
        string className = null;
        string xmlName = null;
        string excelName = null;
        Dictionary<string, Sheet> sheetDic = ReadSheet(name, ref excelName, ref xmlName, ref className);
        string excelPath = ExcelFullPath(excelName);
        Dictionary<string, SheetData> sheetDataDic = new Dictionary<string, SheetData>();

        #region read excel

        try
        {
            using (FileStream fileStream =
                new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
                {
                    ExcelWorksheets worksheets = excelPackage.Workbook.Worksheets;
                    foreach (var worksheet in worksheets)
                    {
                        SheetData sheetData = new SheetData();
                        Sheet sheet = sheetDic[worksheet.Name];
                        var sheetColCount = worksheet.Dimension.End.Column;
                        int sheetRowCount = worksheet.Dimension.End.Row;
                        foreach (var variable in sheet.Variables)
                        {
                            sheetData.Names.Add(variable.Name);
                            sheetData.Types.Add(variable.Type);
                        }

                        for (int r = 1; r < sheetRowCount; r++)
                        {
                            RowData rowData = new RowData();

                            int c = 0;
                            if (string.IsNullOrEmpty(sheet.Split) && !string.IsNullOrEmpty(sheet.Parent.Foreign))
                            {
                                rowData.ForeignKey = worksheet.Cells[r + 1, 1].Value.ToString().Trim();
                                c = 1;
                            }

                            for (; c < sheetColCount; c++)
                            {
                                string value = "";
                                var range = worksheet.Cells[r + 1, c + 1];
                                if (range.Value != null)
                                    value = range.Value.ToString().Trim();
                                string key = GetNameFromCol(sheet.Variables, worksheet.Cells[1, c + 1].Value.ToString())
                                    .Trim();
                                rowData.Dic.Add(key, value);
                            }

                            sheetData.Datas.Add(rowData);
                        }

                        sheetDataDic.Add(worksheet.Name, sheetData);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        #endregion

        object instance = CreateInstance(className);
        List<Sheet> outermostSheets = new List<Sheet>();
        foreach (var sheet in sheetDic.Values)
        {
            if (sheet.Depth == 1)
            {
                outermostSheets.Add(sheet);
            }
        }

        foreach (var outermostSheet in outermostSheets)
        {
            WriteInstanceData(instance, outermostSheet, sheetDataDic[outermostSheet.SheetName], sheetDic,
                sheetDataDic, null);
        }

        SerializeOption.XmlSerialize(XmlFullPath(xmlName), instance);
        SerializeOption.BinarySerialize(BinaryFullPath(xmlName), instance);
        Debug.Log($"{className} data  imported");

        
        AssetDatabase.Refresh();
    }

    #region genxml.tools

    private static string GetNameFromCol(List<Variable> variables, string col)
    {
        foreach (var variable in variables)
        {
            if (string.Equals(variable.Col, col))
            {
                return variable.Name;
            }
        }

        return null;
    }

    private static void WriteInstanceData(object instance, Sheet sheet, SheetData sheetData,
        Dictionary<string, Sheet> sheetDic, Dictionary<string, SheetData> sheetDataDic, string mainKey)
    {
        var list = CreateList(GetTypeByClassName(sheet.ClassName));
        for (int i = 0; i < sheetData.Datas.Count; i++)
        {
            if (mainKey != null)
            {
                if (!string.IsNullOrEmpty(sheetData.Datas[i].ForeignKey))
                {
                    if (sheetData.Datas[i].ForeignKey != mainKey)
                    {
                        continue;
                    }
                }
            }
            var listItem = CreateInstance(sheet.ClassName);
            foreach (var variable in sheet.Variables)
            {
                if (variable.Type.Equals("list"))
                {
                    if (string.IsNullOrEmpty(variable.Split))
                        WriteInstanceData(listItem, sheetDic[variable.ListSheetName],
                            sheetDataDic[variable.ListSheetName],
                            sheetDic, sheetDataDic, sheetData.Datas[i].Dic[sheet.MainKey]);
                    else
                    {
                        var blockValue = sheetData.Datas[i].Dic[variable.Name];
                        UnpackSheetBlock(listItem, blockValue, sheetDic[variable.ListSheetName]);
                    }
                }
                else if (variable.Type.Equals("stringblock") || variable.Type.Equals("floatblock") ||
                         variable.Type.Equals("intblock") || variable.Type.Equals("boolblock"))
                {
                    var blockValue = sheetData.Datas[i].Dic[variable.Name];
                    UnpackBlock(listItem, blockValue, variable);
                }
                else
                {
                    var value = sheetData.Datas[i].Dic[variable.Name];
                    if (string.IsNullOrEmpty(value))
                    {
                        if (!string.IsNullOrEmpty(variable.Default))
                            value = variable.Default;
                        else
                        {
                            Debug.LogError(
                                $"ReadInstanceData Error : {variable.Name} do not have a default value,please make sure the value is filled in excel or correct reg");
                            continue;
                        }
                    }

                    listItem.SetValue(variable.Name, value, variable.Type);
                }
            }

            list.AddListItem(listItem);
        }

        instance.SetValue(sheet.Parent.Name, list);
    }

    private static void UnpackBlock(object item, string block, Variable variable)
    {
        Type type = null;
        switch (variable.Type)
        {
            case "stringblock":
                type = typeof(string);
                break;
            case "intblock":
                type = typeof(int);
                break;
            case "floatblock":
                type = typeof(float);
                break;
            case "boolblock":
                type = typeof(bool);
                break;
        }

        var list = CreateList(type);
        string[] values = block.Split(variable.Split.ToCharArray());
        foreach (var value in values)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.Log($"UnpackBlock : {block} contains a blank value");
                continue;
            }

            list.AddListItem(value.Trim());
        }

        item.SetValue(variable.Name, list);
    }

    private static void UnpackSheetBlock(object instance, string block, Sheet sheet)
    {
        if (string.IsNullOrEmpty(block))
        {
            Debug.Log($"{sheet.SheetName}  contains a blank sheetblock");
            return;
        }

        Type type = GetTypeByClassName(sheet.ClassName);
        var list = CreateList(type);

        string splitOuter = sheet.Parent.Split;
        string splitInner = sheet.Split;

        var outerValues = block.Split(splitOuter.ToCharArray());
        foreach (var outerValue in outerValues)
        {
            var item = CreateInstance(sheet.ClassName);
            var innerValues = outerValue.Trim().Split(splitInner.ToCharArray());
            for (int i = 0; i < innerValues.Length; i++)
            {
                if (string.IsNullOrEmpty(innerValues[i]))
                {
                    Debug.Log($"UnpackSheetBlock : {block} contains a blank value");
                    continue;
                }

                item.SetValue(sheet.Variables[i].Name, innerValues[i].Trim(), sheet.Variables[i].Type);
            }

            list.AddListItem(item);
        }

        instance.SetValue(sheet.Parent.Name, list);
    }
    #endregion

    #endregion

    #region common.tools

    private static Dictionary<string, Sheet> ReadSheet(string name, ref string excelName, ref string xmlName,
        ref string className)
    {
        string regPath = RegFullPath(name);
        if (!File.Exists(regPath))
        {
            Debug.LogError($"{name} reg file dose not exist");

            return null;
        }

        XmlReader xmlReader = XmlReader.Create(regPath);
        XmlReaderSettings settings = new XmlReaderSettings {IgnoreComments = true};
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(xmlReader);
        XmlElement rootElement = xmlDocument.SelectSingleNode("data") as XmlElement;
        className = rootElement.GetAttribute("name");
        excelName = rootElement.GetAttribute("from");
        xmlName = rootElement.GetAttribute("to");

        Dictionary<string, Sheet> sheetDic = new Dictionary<string, Sheet>();

        ReadRegElement(rootElement, sheetDic, 0);
        xmlReader.Close();
        return sheetDic;
    }
    private static bool IsFileOccupied(string path)
    {
        bool result = false;
        if (!File.Exists(path)) return result;
        FileStream fileStream = null;
        try
        {
            fileStream = File.OpenRead(path);
        }
        catch (Exception e)
        {
            result = true;
        }
        finally
        {
            fileStream?.Close();
        }

        return result;
    }

    #endregion

    public class Sheet
    {
        public Variable Parent { get; set; }

        public int Depth { get; set; }

        public string ClassName { get; set; }
        public string SheetName { get; set; }
        public string MainKey { get; set; }
        public string Split { get; set; }
        public List<Variable> Variables;
    }

    public class Variable
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Col { get; set; }
        public string Default { get; set; }
        public string Foreign { get; set; }

        #region list

        public string Split { get; set; }
        public string ListClassName { get; set; }
        public string ListSheetName { get; set; }

        #endregion
    }

    public class SheetData
    {
        public List<string> Names = new List<string>();
        public List<string> Types = new List<string>();
        public List<RowData> Datas = new List<RowData>();
    }

    public class RowData
    {
        public string ForeignKey = "";
        public Dictionary<string, string> Dic = new Dictionary<string, string>();
    }

    #region test

    [MenuItem("Tools/Test/测试读取reg")]
    public static void ReadReg()
    {
        XmlReader xmlReader = null;
        try
        {
            string path = Application.dataPath + "/../Data/Reg/PokemonConfigData.reg.xml";
            xmlReader = XmlReader.Create(path);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlReader);
            XmlNode rootNode = xmlDocument.SelectSingleNode("data");
            XmlElement rootElement = (XmlElement) rootNode;


            string className = rootElement.GetAttribute("name");
            string excelName = rootElement.GetAttribute("from");
            string xmlName = rootElement.GetAttribute("to");
            foreach (XmlNode xmlNode in rootElement.ChildNodes)
            {
                XmlElement element = xmlNode as XmlElement;
                string name = element.GetAttribute("name");
                string type = element.GetAttribute("type");
                Debug.LogError($"{name}  {type}");
                XmlElement listElement = element.FirstChild as XmlElement;
                string listName = listElement.GetAttribute("name");
                string sheetName = listElement.GetAttribute("sheetname");
                string mainkey = listElement.GetAttribute("mainkey");
                Debug.LogError($"list : {listName} {sheetName} {mainkey}");
                foreach (var variable in listElement.ChildNodes)
                {
                    XmlElement classProperty = variable as XmlElement;
                    Debug.LogError($"{classProperty.GetAttribute("name")}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e
            );
        }
        finally
        {
            xmlReader?.Close();
        }
    }

    [MenuItem("Tools/Test/测试写入excel")]
    public static void WriteExcel()
    {
        string path = Application.dataPath + "/../Data/Excel/C宝可梦.xlsx";
        FileInfo xlsxFile = new FileInfo(path);
        if (xlsxFile.Exists)
        {
            xlsxFile.Delete();
            xlsxFile = new FileInfo(path);
        }

        using (ExcelPackage package = new ExcelPackage(xlsxFile))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Pokemon配置");
            worksheet.Cells.Style.WrapText = true;
            worksheet.Cells.AutoFitColumns();
            ExcelRange range = worksheet.Cells[1, 1];
            range.Value = "testewfweagewgwegwefgwaerfwa\nef...........";
            package.Save();
        }
    }

    [MenuItem("Tools/Test/测试已有类反射")]
    public static void TestObjectReflection()
    {
        TestInfo testInfo = new TestInfo()
        {
            Id = 1,
            Name = "lily",
            IsDie = true,
            Lines = new List<string>
            {
                "吃了吗您",
                "886"
            },
            TestInfoInners = new List<TestInfoInner>
            {
                new TestInfoInner()
                {
                    Content = "hahahaha"
                }
            }
        };
        object list =
            testInfo.GetMemberValue("TestInfoInners");
        int listCount = Convert.ToInt32(list.GetType().InvokeMember("get_Count",
            BindingFlags.Default | BindingFlags.InvokeMethod, null, list, null));
        for (int i = 0; i < listCount; i++)
        {
            var item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null,
                list, new object[] {i});
            var content =
                item.GetMemberValue("Content");

            Debug.Log(content);
        }
    }

    [MenuItem("Tools/Test/测试已有数据反射")]
    public static void TestDataReflection()
    {
        object obj = CreateInstance("TestInfo");

        obj.SetValue("Id", "1001", "int");
        obj.SetValue("Name", "Leyla", "string");
        obj.SetValue("Height", "516.25", "float");
        obj.SetValue("IsDie", "true", "bool");
        obj.SetValue("EnumTest", "Var1", "enum");

        Type type = typeof(string);
        object list = CreateList(type);
        object testInfoInners = CreateList(typeof(TestInfoInner));

        for (int i = 0; i < 3; i++)
        {
            var addItem = "测试填充数据";
            list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list,
                new object[] {addItem});
        }

        for (int i = 0; i < 3; i++)
        {
            object addItem = CreateInstance("TestInfoInner");
            addItem.SetValue("Content", "测试content", "string");
            testInfoInners.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null,
                testInfoInners, new object[] {addItem});
        }

        obj.GetType().GetProperty("Lines")?.SetValue(obj, list);
        obj.GetType().GetProperty("TestInfoInners")?.SetValue(obj, testInfoInners);
        TestInfo testInfo = obj as TestInfo;
        Debug.Log(
            $"  {testInfo.Id} {testInfo.Name} die? {testInfo.IsDie} height : {testInfo.Height}  {testInfo.EnumTest} {testInfo.Lines[0]} {testInfo.TestInfoInners[0].Content}");
    }
}

public enum EnumTest
{
    None,
    Var1,
    Var2
}


public class TestInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float Height { get; set; }
    public bool IsDie { get; set; }
    public EnumTest EnumTest { get; set; }
    public List<string> Lines { get; set; }
    public List<TestInfoInner> TestInfoInners { get; set; }
}

public class TestInfoInner
{
    public string Content { get; set; }
}

#endregion