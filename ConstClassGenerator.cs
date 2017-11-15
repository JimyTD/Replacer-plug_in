using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// 用于生成管理类的工具
/// </summary>
public class ConstClassGenerator : MonoBehaviour {


    [System.Serializable]
    public class strConstData
    {
        [ColumnMapping("file")]
        public string codeFile;
        [ColumnMapping("中文")]
        public string strConst;
    }
    static FileStream referedFile;// 生成的文件
    static DirectoryInfo codeFile;//代码中文件
    static FileStream config;
    static strConstData[] datas;
    /// <summary>
    /// 初始化并生成链接文件
    /// </summary>
    static bool Initial()
    {
        if (!File.Exists("Assets/wordsTest.txt"))
        {
            Debug.Log("No Config!");
            return false;
        }
        datas = CsvImporter.Parser<strConstData>(File.ReadAllBytes("Assets/wordsTest.txt"));
        if (File.Exists("Assets/constStrings.cs"))
        {
            File.Delete("Assets/constStrings.cs");
        }//删除老的配置
        referedFile = File.Open("Assets/constStrings.cs", FileMode.OpenOrCreate);
        if (!File.Exists("Assets/constStrings.cs"))
        {
            Debug.Log("Create failed!");
            return false;
        }
        //写入类信息
        string temp="class constStrings{\n";
        byte[] barr = System.Text.Encoding.UTF8.GetBytes(temp);
        referedFile.Write(barr, 0, barr.Length);
        Debug.Log("fileOpenSuccess");
        return true;
    }
   

    /// <summary>
    /// 搜索文件
    /// </summary>
    static string searchFile(ref string fileName,int i)
    {
        if (datas[i].codeFile != "")
            fileName = datas[i].codeFile;
        string[] fileArr = Directory.GetFiles("Assets/", fileName+".cs", SearchOption.AllDirectories);//获取文件
        int  a=fileArr.Length;
        if (a<1)
        {
            Debug.Log("The file may be wrong:"+fileName);
            return null;
        }
        return fileArr[0];
    }
    /// <summary>
    /// 搜索全部cs脚本.
    /// </summary>
    /// <returns></returns>
    static string[] searchAllFiles()
    {
        string[] fileArr = Directory.GetFiles("Assets/", "*.cs", SearchOption.AllDirectories);//获取所有脚本文件
        if (fileArr.Length < 1)
        {
            Debug.Log("The file may be wrong:");
            return null;
        }
        return fileArr;
    }

    /// <summary>
    /// 搜索代码,todo
    /// </summary>
    static void searchCode(string path,string fileName,int i)
    {
        //byte[] byteArray=File.ReadAllBytes(name);
        //string Ascstr = new string(System.Text.Encoding.ASCII.GetString(byteArray).ToCharArray());//上面采用的是读正常字符集的办法，File类不如StreamReader灵活，下面指定了UTF-8读入
        StreamReader sr = new StreamReader(path, System.Text.Encoding.UTF8);
        string str = "";
        str = sr.ReadToEnd();
        string temp = str;
        if(str=="")
        {
            Debug.Log("Read fault..");
        }
        sr.Close();
        string objectName = "constStrings."+fileName + i.ToString();//引用的字符串变量
        string changedStr=str.Replace(@"""" + datas[i].strConst + @"""", objectName);
        if (changedStr.Equals(temp))//未修改，表示未找到替换部分
        {
            Debug.Log("Fail to find string!" + datas[i].strConst);
            return;
        }
        string objectString = "    public const string " + fileName + i.ToString() + @" =""" + datas[i].strConst + @""";"+"\n";
        byte[] barr = System.Text.Encoding.UTF8.GetBytes(objectString);
        referedFile.Write(barr, 0, barr.Length);//写入参照程序
        StreamWriter sw = new StreamWriter(path,false, System.Text.Encoding.UTF8);//false表示全部重写
        sw.Write(changedStr);
        sw.Flush();
        sw.Close();

    }

    /// <summary>
    /// 生成特殊的替代字符串，针对有“+”类的分类字符串进行处理
    /// </summary>
    /// <returns></returns>
    static string generateSpecialReplacingString()
    {
        return null;
    }
    
    /// <summary>
    /// 清理并完成最后内容
    /// </summary>
    static void distruct()
    {
        string temp = "};";
        byte[] barr = System.Text.Encoding.UTF8.GetBytes(temp);
        referedFile.Write(barr, 0, barr.Length);
        referedFile.Flush();
        referedFile.Close();
        Debug.Log("fileCloseSuccess");
    }
    
    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// 搜索需要转义的字符串
    /// </summary>
    [MenuItem("ConstString/搜索特殊字符串")]
    public static void searchSpecialString()
    {
        string[] fileArr = searchAllFiles();
        for (int i = 0; i < fileArr.Length; i++)//对每一个文件进行搜索
        {
            StreamReader sr = new StreamReader(fileArr[i], System.Text.Encoding.UTF8);
            string str = "";
            str = sr.ReadToEnd();
            string pattern = @"("".*[\u4E00-\u9FA5]+.*?""\s*\+)|(\+\s*"".*[\u4E00-\u9FA5].*?"")";//带有加号的特殊字符串
            //Match match = Regex.Match(str, pattern);
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (regex.IsMatch(str))
            {
                MatchCollection matchCollection = regex.Matches(str);
                foreach (Match match in matchCollection)
                {
                    string value = match.Value;//获取到的
                    Debug.Log(value);//tester
                }
            }
            //Debug.Log("读取文件" + fileArr[i]);
        }
        Debug.Log("扫描完成");
    }

    /// <summary>
    /// 扫描中文生成配置的方法,todo
    /// </summary>
    [MenuItem("ConstString/扫描中文生成配置")]
    public static void buildConfig()
    {
        int count = 0;
        string[] fileArr = searchAllFiles();
        for (int i=0;i<fileArr.Length; i++)//对每一个文件进行搜索
        {
            ///
            
            ///
            StreamReader sr = new StreamReader(fileArr[i], System.Text.Encoding.UTF8);
            string str = "";
            str = sr.ReadToEnd();
            string pattern= @"""\w*[\u4E00-\u9FA5]+.*?""";//中文匹配的正则表达式 
            //Match match = Regex.Match(str, pattern);
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (regex.IsMatch(str))
            {
                MatchCollection matchCollection = regex.Matches(str);
                foreach (Match match in matchCollection)
                {
                    string value = match.Value;//获取到的
                    count++;
                    Debug.Log(value+count.ToString());//tester
                }
            }
            Debug.Log("读取文件" + fileArr[i]);
        }
        Debug.Log("扫描完成");

    }
    /// <summary>
    /// 生成代码的接口
    /// </summary>
    [MenuItem("ConstString/生成链接")]
    public static void generate(){
        Initial();
        string fileName = "";
        for (int i = 0; i < datas.Length; i++)
        {
            string name=searchFile(ref fileName,i);
            if(name!=null)searchCode(name,fileName,i);

        }
        distruct();
    }

}
