using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.IO;
using Excel;

/// <summary>
/// Excel解析工具
/// </summary>
public class ExcelTool : MonoBehaviour
{
    /// <summary>
    /// 读取excel文件内容
    /// 自己写方法解析
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="columnNum">行数</param>
    /// <param name="rowNum">列数</param>
    /// <returns></returns>
    public static DataRowCollection ReadExcel(string filePath, ref int columnNum, ref int rowNum, int table = 0)
    {
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

        DataSet result = excelReader.AsDataSet();
        //Tables[0] 下标0表示excel文件中第一张表的数据
        columnNum = result.Tables[table].Columns.Count;
        rowNum = result.Tables[table].Rows.Count;
        return result.Tables[table].Rows;
    }
    
    /// <summary>
    /// 直接解析成二维string数组格数
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static string[,] ReadAsString(string filePath, int table = 0)
    {
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

        DataSet result = excelReader.AsDataSet();
        //Tables[0] 下标0表示excel文件中第一张表的数据
        int columnNum = 0, rowNum = 0;
        DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);

        string[,] str = new string[rowNum + 1, columnNum + 1];
        for (int j = 0; j < columnNum + 1; j++) 
        {
            for (int i = 0; i < rowNum + 1; i++) 
            {
                if (i == 0 || j == 0)
                    str[i, j] = "0";
                else
                    str[i, j] = collect[i - 1][j - 1].ToString();
            }
        }
        return str;
    }
}
