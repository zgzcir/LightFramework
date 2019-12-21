using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.IO;
using System.Xml;
using System.ComponentModel;
using System.Net;
using System.Security.Permissions;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.Style;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using Object = UnityEngine.Object;
using static ReflectionExtensions;
using  static RLFramework.Base.Logger;

public class ConfigEditor
{
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

                string xmlPath = PathDefineEditor.GetClass2XmlSaveWithNamePath(name);
                SerializeOption.XmlSerialize(xmlPath, instance);
                Debug.Log($"{name} to xml done : {xmlPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ClassToXmlProcess Error : {e}");
            throw;
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
        string xmlPath = PathDefineEditor.GetClass2XmlSaveFullPath();
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
                string xmlPath = PathDefineEditor.GetClass2XmlSaveWithNamePath(name);
                System.Object obj = SerializeOption.XmlDeserializeEditor(xmlPath, type);
                string binaryPath = PathDefineEditor.GetClassToBinarySaveWithNamePath(name);
                SerializeOption.BinarySerialize(binaryPath, obj);
                Debug.Log($"Xml {name} to binary done : {binaryPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Xml2BinaryProcess Error : {e}");
            throw;
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

    [MenuItem("Tools/配置/全体xml生成Excel")]
    public static void AssetsXmlGenExcelUnite()
    {
        Object[] objs = Selection.objects;
        string xmlPath = PathDefineEditor.GetClass2XmlSaveFullPath();
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
            ReadSheetData(data, t, sheetDic, sheetDataDic);
        }
        string xlslPath = Application.dataPath.Replace("Assets", "Data/Excel/" + excelName);
        if (IsFileOccupied(xlslPath))
        {
            Debug.LogError("File is already occupied, please close and try again.");
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
                    for (int i = 0; i < sheetData.ColNames.Count; i++)
                    {
                        var range = worksheet.Cells[1, i + 1];
                        range.Value = sheetData.ColNames[i];
                    }

                    for (int i = 0; i < sheetData.Datas.Count; i++) //->>>行
                    {
                        RowData rowData = sheetData.Datas[i];
                        for (int j = 0; j < sheetData.Datas[i].Dic.Count; j++) //->>>列
                        {
                            var range = worksheet.Cells[i + 2, j + 1];
                            range.Value = rowData.Dic[sheetData.ColNames[j]];
                        }
                    }

                    worksheet.Cells.AutoFitColumns();
                }

                excelPackage.Save();
                Debug.Log($"生成{xlslPath}成功");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e
            );
            throw;
        }
    }

    #region tools

    private static object GetInstanceFromXml(string name)
    {
        object instance = null;
        Type type = GetTypeByClassName(name);
        if (type != null)
        {
            string xmlPath = PathDefineEditor.GetClass2XmlSaveWithNamePath(name);
            instance = SerializeOption.XmlDeserializeEditor(xmlPath, type);
        }

        return instance;
    }

    private static Dictionary<string, Sheet> ReadSheet(string name, ref string excelName, ref string xmlName,
        ref string className)
    {
        string regPath = Application.dataPath + $"/../Data/Reg/{name}.reg.xml";
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

                    ListName = list.GetAttribute("name"),
                    ListSheetName = list.GetAttribute("sheetname")
                };
                Sheet sheet = new Sheet()
                {
                    Parent = parent,
                    Name = list.GetAttribute("name"),
                    SheetName = list.GetAttribute("sheetname"),
                    MainKey = list.GetAttribute("mainkey"),
                    Split = list.GetAttribute("split"),
                    Children = new List<Variable>(),
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
                                childVariable.ListName = childList.GetAttribute("name");
                                childVariable.ListSheetName = childList.GetAttribute("sheetname");
                                ReadRegElement(childList, sheetDic, depth);
                            }
                            sheet.Children.Add(childVariable);
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
    private static void ReadSheetData(object data, Sheet sheet, Dictionary<string, Sheet> sheetDic,
        Dictionary<string, SheetData> sheetDataDic)
    {
        List<Variable> variables = sheet.Children;
        var parent = sheet.Parent;
        object dataList =
            data.GetMemberValue(parent.Name);
        int listCount = dataList.GetListCount();
        SheetData sheetData = new SheetData();
        for (int i = 0; i < variables.Count; i++)
        {
            var variable = variables[i];
            if (!string.IsNullOrEmpty(variable.Col))
            {
                sheetData.ColNames.Add(variable.Col);
                sheetData.Types.Add(variable.Type);
            }
        }

        for (int i = 0; i < listCount; i++) //--->一条数据
        {
            object member = dataList.GetListItemValue(i);
            var rowData = new RowData();
            for (int j = 0; j < variables.Count; j++) //每条数据里的每个变量
            {
                
                var variable = variables[j];
                if (variable.Type == "list") // todo->>>sheetname
                {
                    Sheet innerSheet = sheetDic[variable.ListSheetName];
                    ReadSheetData(member, innerSheet, sheetDic, sheetDataDic);
                }
                else if (variable.Type.Equals("stringblock") || variable.Type.Equals("floatblock") ||
                         variable.Type.Equals("intblock") || variable.Type.Equals("boolblock"))
                {
                    string block = GetBlockContent(member, variable);
                    rowData.Dic.Add(variable.Col,block);
                }
                else
                {
                    var value = member.GetMemberValue(variable.Name);
                    if (value != null)
                    {
                        rowData.Dic.Add(variable.Col, value.ToString());
                    }
                    else
                    {
                        LogError($"Can not get member value : {variable.Name},check your reg.");
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

    private static string GetBlockContent(object data, Variable variable)
    {
        if (string.IsNullOrEmpty(variable.Split))
        {
            LogError("split is isNullOrEmpty");
        }
        string split = variable.Split;
        StringBuilder content = new StringBuilder();
        object block = data.GetMemberValue(variable.Name);
        var blockListCount = block.GetListCount();
        for (int i = 0; i < blockListCount; i++)
        {
            content.Append(block.GetListItemValue(i));
            content.Append(split);
        }
        content.Remove(content.Length-1,1);
        return content.ToString();
    }

    #region fileTools

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



    #endregion

    public class Sheet
    {
        public Variable Parent { get; set; }

        public int Depth { get; set; }

        public string Name { get; set; }
        public string SheetName { get; set; }
        public string MainKey { get; set; }
        public string Split { get; set; }
        public List<Variable> Children;
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
        public string ListName { get; set; }
        public string ListSheetName { get; set; }

        #endregion
    }

    public class SheetData
    {
        public List<string> ColNames = new List<string>();
        public List<string> Types = new List<string>();
        public List<RowData> Datas = new List<RowData>();
    }

    public class RowData
    {
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
            Debug.LogError(e);
            throw;
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