using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace ClsNk
{

    /// <summary>
    /// 2014/07/12更新
    /// </summary>
    public static class FileIO
    {

        #region FileInput


        public static void readFile(string filePath, out double[,] readData)
        {
            //データ読み込み用変数
            string subStr;
            string[] subStrArr;
            string[] subStrArrColumn;
            string[] sep = new string[] { "  ", " ", "\t", ",", ",          " };
            string[] sepCol = new string[] { "\r\n", "\n", "\r" };
            
            StreamReader sr = new StreamReader(filePath);
            subStr = sr.ReadToEnd();
            subStrArrColumn = subStr.Split(sepCol, StringSplitOptions.RemoveEmptyEntries);

            readData = new double[subStrArrColumn[0].Split(sep, StringSplitOptions.RemoveEmptyEntries).Length, subStrArrColumn.Length];

            //'08/11/11追記
            int intX = subStrArrColumn[0].Split(sep, StringSplitOptions.RemoveEmptyEntries).Length;

            for (int i = 0; i < subStrArrColumn.Length; i++)
            {
                subStrArr = subStrArrColumn[i].Split(sep, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < intX; j++)
                {
                    Double.TryParse(subStrArr[j], out readData[j, i]);
                }
            }
            sr.Close();
        }



        public static void readFile(string filePath, out string[,] readString)
        {
            //データ読み込み用変数
            string subStr;
            string[] subStrArr;
            string[] subStrArrColumn;
            string[] sep = new string[] { "  ", " ", "\t", ",", ",          " };
            string[] sepCol = new string[] { "\r\n", "\n", "\r" };

            StreamReader sr = new StreamReader(filePath, System.Text.Encoding.GetEncoding("shift_jis"));
            subStr = sr.ReadToEnd();
            subStrArrColumn = subStr.Split(sepCol, StringSplitOptions.RemoveEmptyEntries);

            readString = new string[subStrArrColumn[0].Split(sep, StringSplitOptions.RemoveEmptyEntries).Length, subStrArrColumn.Length];

            for (int i = 0; i < subStrArrColumn.Length; i++)
            {
                subStrArr = subStrArrColumn[i].Split(sep, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < subStrArr.Length; j++)
                {
                    readString[j, i] = subStrArr[j];
                }
            }
            sr.Close();
        }

        public static void readFileTab(string filePath, out string[,] readString)
        {
            //データ読み込み用変数
            string subStr;
            string[] subStrArr;
            string[] subStrArrColumn;
            string[] sep = new string[] {"\t"};
            string[] sepCol = new string[] { "\r\n", "\n", "\r" };

            StreamReader sr = new StreamReader(filePath, System.Text.Encoding.GetEncoding("shift_jis"));
            subStr = sr.ReadToEnd();
            subStrArrColumn = subStr.Split(sepCol, StringSplitOptions.RemoveEmptyEntries);

            readString = new string[subStrArrColumn[0].Split(sep, StringSplitOptions.RemoveEmptyEntries).Length, subStrArrColumn.Length];

            for (int i = 0; i < subStrArrColumn.Length; i++)
            {
                subStrArr = subStrArrColumn[i].Split(sep, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < subStrArr.Length; j++)
                {
                    readString[j, i] = subStrArr[j];
                }
            }
            sr.Close();
        }

        /// <summary>
        /// 一列のデータ読み込み用：列がたくさんある場合は最初の列の値を読み込む
        /// </summary>
        /// <param name="filePath">読み込みファイルネーム</param>
        /// <param name="readData">出力データファイル</param>
        public static void readFile(string filePath, out double[] readData)
        {
            //データ読み込み用変数
            string subStr;
            string[] subStrArr;
            string[] subStrArrColumn;
            string[] sep = new string[] { "  ", " ", "\t", ",", ",          " };
            string[] sepCol = new string[] { "\r\n", "\n", "\r" };
            
            StreamReader sr = new StreamReader(filePath);
            subStr = sr.ReadToEnd();
            subStrArrColumn = subStr.Split(sepCol, StringSplitOptions.RemoveEmptyEntries);

            readData = new double[subStrArrColumn.Length];

            for (int i = 0; i < subStrArrColumn.Length; i++)
            {
                subStrArr = subStrArrColumn[i].Split(sep, StringSplitOptions.RemoveEmptyEntries);

                //この"0"の値を変えることで異なる列の値を読みだすことが可能
                readData[i] = Convert.ToDouble(subStrArr[0]);
            }
            sr.Close();
        }

        /// <summary>
        /// 配列の数が変わるとき用
        /// ジャグ配列で読込
        /// [j][i]になっている
        /// </summary>
        /// <param name="filePath">読込ファイルパス</param>
        /// <param name="readData">出力データファイル</param>
        public static void readFile(string filePath, out double[][] readData)
        {
            //データ読み込み用変数
            string subStr;
            string[] subStrArr;
            string[] subStrArrColumn;
            string[] sep = new string[] { "  ", " ", "\t", ",", ",          " };
            string[] sepCol = new string[] { "\r\n", "\n", "\r" };

            StreamReader sr = new StreamReader(filePath);
            subStr = sr.ReadToEnd();
            subStrArrColumn = subStr.Split(sepCol, StringSplitOptions.RemoveEmptyEntries);

            readData=new double[subStrArrColumn.Length][];

            //readData = new double[subStrArrColumn.Length][subStrArrColumn[0].Split(sep, StringSplitOptions.RemoveEmptyEntries).Length, ];

            //'08/11/11追記

            for (int j = 0; j < subStrArrColumn.Length; j++)
            {
                int intX = subStrArrColumn[j].Split(sep, StringSplitOptions.RemoveEmptyEntries).Length;

                readData[j] = new double[intX];
                subStrArr = subStrArrColumn[j].Split(sep, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < intX; i++)
                {
                    Double.TryParse(subStrArr[i], out readData[j][i]);
                }
            }
            sr.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Lock:true</returns>
        public static bool IsFileLocked(string path)
        {
            FileStream fs = null;

            try
            { fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None); }
            catch { return true; }
            finally
            {
                if(fs!=null)
                { fs.Close(); }
            }
            return false;
        }

        #endregion FileInput

        #region FileOutput

        /// <summary>
        /// データ書込
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="writeData">書き込むデータ</param>
        /// <param name="append">false:上書き true:追記</param>
        public static void writeFile<Type>(string filePath, Type writeData, bool append = false)
        {
            StringBuilder sbData = new StringBuilder();
            sbData.AppendLine(Convert.ToString(writeData));
            if (append)
                File.AppendAllText(filePath, sbData.ToString());
            else
                File.WriteAllText(filePath, sbData.ToString());
        }

        /// <summary>
        /// 1次元データ書込
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="writeData">書き込むデータ</param>
        /// <param name="append">false:上書き true:追記</param>
        public static void writeFile<Type>(string filePath, Type[] writeData, bool append = false)
        {
            StringBuilder sbData = new StringBuilder();
            for (int i = 0; i < writeData.Length; i++)
                sbData.AppendLine(Convert.ToString(writeData[i]));

            if (append)
                File.AppendAllText(filePath, sbData.ToString());
            else
                File.WriteAllText(filePath, sbData.ToString());
        }

        /// <summary>
        /// 2次元データ書込
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="writeData">書き込むデータ</param>
        /// <param name="append">false:上書き true:追記</param>
        public static void writeFile<Type>(string filePath, Type[,] writeData, bool append = false)
        {
            StringBuilder sbData = new StringBuilder();
            for (int j = 0; j < writeData.GetLength(1); j++)
            {
                for (int i = 0; i < writeData.GetLength(0); i++)
                    sbData.Append(Convert.ToString(writeData[i, j])).Append(" ");
                sbData.AppendLine("");
            }

            if (append)
                File.AppendAllText(filePath, sbData.ToString());
            else
                File.WriteAllText(filePath, sbData.ToString());

        }

        /// <summary>
        /// 一次元×2データ書き込み
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="writeData1"></param>
        /// <param name="writeData2"></param>
        /// <param name="append"></param>
        public static void writeFile<Type>(string filePath, Type[] writeData1, Type[] writeData2, bool append = false)
        {
            StringBuilder sbData = new StringBuilder();
            for (int i = 0; i < writeData1.Length; i++)
                sbData.AppendLine(Convert.ToString(writeData1[i]) + "," + Convert.ToString(writeData2[i]));

            if (append)
                File.AppendAllText(filePath, sbData.ToString());
            else
                File.WriteAllText(filePath, sbData.ToString());

        }

        #endregion FileOutput
    }


}
