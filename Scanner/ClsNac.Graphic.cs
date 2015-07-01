using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ClsNac.Graphic
{
    class PlotBMP
    {
        static Bitmap bmp(double[,] data,bool ExceptZero=false) 
        {
            Bitmap img = new Bitmap(data.GetLength(0), data.GetLength(1));


            return img;
        }

        static double GetMax(double[,] data, bool ExceptZero = false)
        {
            int iMax = data.GetLength(0);
            int jMax = data.GetLength(1);
            double max = Double.MinValue;

            for(int i=0;i<iMax;i++)
            {
                for(int j=0;j<jMax;j++)
                {
                    if (ExceptZero && data[i, j] == 0.0) continue;
                    max = max < data[i, j] ? data[i, j] : max;
                }
            }
            return max;
        }

        static double GetMin(double[,] data, bool ExceptZero = false)
        {
            int iMax = data.GetLength(0);
            int jMax = data.GetLength(1);
            double min = Double.MaxValue;

            for (int i = 0; i < iMax; i++)
            {
                for (int j = 0; j < jMax; j++)
                {
                    if (ExceptZero && data[i, j] == 0.0) continue;
                    min = min > data[i, j] ? data[i, j] : min;
                }
            }
            return min;
        }
    }

    public class Plot2dPlane
    {
        public enum enumGradation
        {
            mono,
            fullcolor1,
            fullcolor2,
            fullcolor3
        }


        //グラデーションモードの設定
        public enumGradation gradationMode = enumGradation.fullcolor3;

        private int intDataColumn, intDataRow, intBmpColumn, intBmpRow, intPictureColumn, intPictureRow;

        //ビットマップを作る際の画像縦横のズームサイズ
        private int intColumnZoom, intRowZoom;

        //描画値の最大最小
        public double dbleDataMax, dbleDataMin;

        //二色バージョン時最大値最小値
        public double dbleData1Max, dbleData1Min, dbleData2Max, dbleData2Min;

        private Bitmap bmpGraph;

        private PictureBox pictureBoxGraph;

        //コンストラクタ(引数：結果を描画するピクチャボックス)
        public Plot2dPlane(PictureBox pictureGraphArea)
        {
            try
            {
                //ピクチャサイズの取得
                intPictureColumn = pictureGraphArea.Width;
                intPictureRow = pictureGraphArea.Height;

                this.pictureBoxGraph = pictureGraphArea;
                this.pictureBoxGraph.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\r\nグラフの初期化に失敗しました。", "Class:KimGraph Method:Plot2dPlane");
            }
        }

        //グラフの描画命令
        public bool Draw(double[,] data,bool ExceptZero=false)
        {
            //データの行数列数の取得
            intDataColumn = data.GetLength(0);
            intDataRow = data.GetLength(1);

            intColumnZoom = (int)(this.intPictureColumn / this.intDataColumn) + 1;
            intRowZoom = (int)(this.intPictureRow / this.intDataRow) + 1;

            intBmpColumn = this.intColumnZoom * this.intDataColumn;
            intBmpRow = this.intRowZoom * this.intDataRow;

            this.bmpGraph = new Bitmap(this.intBmpColumn, this.intBmpRow);

            //データ最大値最小値の取得
            bool test = this.GetMaxMin(data, ExceptZero);

            //ビットマップの作成
            test = this.SetBmp(data);

            //ピクチャボックスへの描画
            this.pictureBoxGraph.Image = this.bmpGraph;

            this.bmpGraph.Save("test.bmp");

            return true;
        }

        //最大値最小値読込みバージョン
        public bool Draw(double max, double min, double[,] data)
        {
            //データの行数列数の取得
            intDataColumn = data.GetLength(0);
            intDataRow = data.GetLength(1);

            intColumnZoom = (int)(this.intPictureColumn / this.intDataColumn) + 1;
            intRowZoom = (int)(this.intPictureRow / this.intDataRow) + 1;

            intBmpColumn = this.intColumnZoom * this.intDataColumn;
            intBmpRow = this.intRowZoom * this.intDataRow;

            this.bmpGraph = new Bitmap(this.intBmpColumn, this.intBmpRow);

            //データ最大値最小値の取得
            bool test = this.GetMaxMin(data);

            //ビットマップの作成
            test = this.SetBmp(max, min, data);

            //ピクチャボックスへの描画
            this.pictureBoxGraph.Image = this.bmpGraph;

            this.bmpGraph.Save("test.bmp");

            return true;
        }

        /// <summary>
        /// 二色色分けで二つのデータを比較グラフ
        /// </summary>
        /// <param name="max1">データ１の最大値</param>
        /// <param name="min1">データ１の最小値</param>
        /// <param name="data1">データ１</param>
        /// <param name="max2">データ２の最大値</param>
        /// <param name="min2">データ２の最小値</param>
        /// <param name="data2">データ２</param>
        /// <returns></returns>
        public bool Draw(double max1, double min1, double[,] data1, double max2, double min2, double[,] data2)
        {
            //データの行数列数の取得
            intDataColumn = data1.GetLength(0);
            intDataRow = data1.GetLength(1);

            intColumnZoom = (int)(this.intPictureColumn / this.intDataColumn) + 1;
            intRowZoom = (int)(this.intPictureRow / this.intDataRow) + 1;

            intBmpColumn = this.intColumnZoom * this.intDataColumn;
            intBmpRow = this.intRowZoom * this.intDataRow;

            this.bmpGraph = new Bitmap(this.intBmpColumn, this.intBmpRow);

            //データ最大値最小値の取得
            this.dbleData1Max = max1;
            this.dbleData1Min = min1;
            this.dbleData2Max = max2;
            this.dbleData2Min = min2;

            //ビットマップの作成
            bool test = this.SetBmp(data1, data2);

            //ピクチャボックスへの描画
            this.pictureBoxGraph.Image = this.bmpGraph;

            this.bmpGraph.Save("test.bmp");

            return true;
        }

        //ビットマップへのデータ描画
        private bool SetBmp(double[,] data)
        {
            try
            {
                Rectangle rect = new Rectangle(0, 0, this.intBmpColumn, this.intBmpRow);
                BitmapData bmpData = this.bmpGraph.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                int stride = bmpData.Stride;

                unsafe
                {
                    byte* p = (byte*)(void*)bmpData.Scan0;
                    int nResidual = stride - this.bmpGraph.Width * 3;

                    for (int i = 0; i < this.intDataRow; i++)
                    {
                        for (int n = 0; n < this.intRowZoom; n++)
                        {
                            for (int j = 0; j < this.intDataColumn; j++)
                            {
                                for (int m = 0; m < this.intColumnZoom; m++)
                                {
                                    switch (this.gradationMode)
                                    {
                                        case (enumGradation.mono):
                                            {
                                                if (data[j, i] > dbleDataMax)
                                                {
                                                    p[2] = (byte)255.0;
                                                    p[1] = (byte)255.0;
                                                    p[0] = (byte)255.0;
                                                }
                                                else if (data[j, i] < dbleDataMin)
                                                {
                                                    p[2] = (byte)0.0;
                                                    p[1] = (byte)0.0;
                                                    p[0] = (byte)0.0;
                                                }
                                                else
                                                {
                                                    p[2] = (byte)(((data[j, i] - (this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin)) * 255.0));
                                                    p[1] = (byte)(((data[j, i] - (this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin)) * 255.0));
                                                    p[0] = (byte)(((data[j, i] - (this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin)) * 255.0));
                                                }
                                                break;
                                            }
                                        case (enumGradation.fullcolor1):
                                            {
                                                if (data[j, i] > dbleDataMax)
                                                {
                                                    p[2] = (byte)255.0;
                                                    p[1] = (byte)0.0;
                                                    p[0] = (byte)0.0;
                                                }
                                                else if (data[j, i] < dbleDataMin)
                                                {
                                                    p[2] = (byte)0.0;
                                                    p[1] = (byte)0.0;
                                                    p[0] = (byte)255.0;
                                                }
                                                else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 2.0 + this.dbleDataMin))
                                                {
                                                    p[2] = (byte)(((data[j, i] - ((this.dbleDataMax + this.dbleDataMin) / 2.0)) / ((this.dbleDataMax - this.dbleDataMin) / 2.0) * 255.0));
                                                    p[1] = (byte)(255 - ((data[j, i] - ((this.dbleDataMax + this.dbleDataMin) / 2.0)) / ((this.dbleDataMax - this.dbleDataMin) / 2.0) * 255.0));
                                                    p[0] = (byte)0;
                                                }
                                                else
                                                {
                                                    p[2] = (byte)0;
                                                    p[1] = (byte)((((data[j, i] - this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 2.0) * 255.0));
                                                    p[0] = (byte)(255 - ((data[j, i] - (this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 2.0) * 255.0));
                                                }
                                                break;
                                            }
                                        case (enumGradation.fullcolor2):
                                            {
                                                if (data[j, i] > dbleDataMax)
                                                {
                                                    p[2] = (byte)255.0;
                                                    p[1] = (byte)0.0;
                                                    p[0] = (byte)0.0;
                                                }
                                                else if (data[j, i] < dbleDataMin)
                                                {
                                                    p[2] = (byte)0.0;
                                                    p[1] = (byte)0.0;
                                                    p[0] = (byte)255.0;
                                                }
                                                else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 3.0 + this.dbleDataMin))
                                                {
                                                    p[2] = (byte)255;
                                                    p[1] = (byte)(255 - ((data[j, i] - ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 3.0 + this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                    p[0] = (byte)0;
                                                }
                                                else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 2.0 + this.dbleDataMin))
                                                {
                                                    p[2] = (byte)(((data[j, i] - ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 2.0 + this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                    p[1] = (byte)255;
                                                    p[0] = (byte)0;
                                                }
                                                else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 1.0 + this.dbleDataMin))
                                                {
                                                    p[2] = (byte)0;
                                                    p[1] = (byte)255;
                                                    p[0] = (byte)(255 - ((data[j, i] - ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 1.0 + this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                }
                                                else
                                                {
                                                    p[2] = (byte)0;
                                                    p[1] = (byte)((((data[j, i] - this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                    p[0] = (byte)255;
                                                }
                                                break;
                                            }
                                        case (enumGradation.fullcolor3):
                                            {
                                                if (data[j, i] == 0.0)
                                                {
                                                    p[2] = (byte)0;
                                                    p[1] = (byte)0;
                                                    p[0] = (byte)0;
                                                }
                                                else if (data[j, i] > dbleDataMax)
                                                {
                                                    p[2] = (byte)255.0;
                                                    p[1] = (byte)0.0;
                                                    p[0] = (byte)0.0;
                                                }
                                                else if (data[j, i] < dbleDataMin)
                                                {
                                                    p[2] = (byte)0.0;
                                                    p[1] = (byte)0.0;
                                                    p[0] = (byte)255.0;
                                                }
                                                else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 3.0 + this.dbleDataMin))
                                                {
                                                    p[2] = (byte)255;
                                                    p[1] = (byte)(255 - ((data[j, i] - ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 3.0 + this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                    p[0] = (byte)0;
                                                }
                                                else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 2.0 + this.dbleDataMin))
                                                {
                                                    p[2] = (byte)(((data[j, i] - ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 2.0 + this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                    p[1] = (byte)255;
                                                    p[0] = (byte)0;
                                                }
                                                else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 1.0 + this.dbleDataMin))
                                                {
                                                    p[2] = (byte)0;
                                                    p[1] = (byte)255;
                                                    p[0] = (byte)(255 - ((data[j, i] - ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 1.0 + this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                }
                                                else
                                                {
                                                    p[2] = (byte)0;
                                                    p[1] = (byte)((((data[j, i] - this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                    p[0] = (byte)255;
                                                }
                                                break;
                                            }

                                    }
                                    p += 3;
                                }
                            }
                            p += nResidual;
                        }
                    }

                    this.bmpGraph.UnlockBits(bmpData);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\r\nビットマップへのデータの描画に失敗しました。", "Glass:KimGraph Method:SetBmp");
                return false;
            }
        }

        //ビットマップへのデータ描画:最大値最小値指定バージョン
        private bool SetBmp(double dbleInnerDataMax, double dbleInnerDataMin, double[,] data)
        {
            try
            {
                Rectangle rect = new Rectangle(0, 0, this.intBmpColumn, this.intBmpRow);
                BitmapData bmpData = this.bmpGraph.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                int stride = bmpData.Stride;

                unsafe
                {
                    byte* p = (byte*)(void*)bmpData.Scan0;
                    int nResidual = stride - this.bmpGraph.Width * 3;

                    for (int i = 0; i < this.intDataRow; i++)
                    {
                        for (int n = 0; n < this.intRowZoom; n++)
                        {
                            for (int j = 0; j < this.intDataColumn; j++)
                            {
                                for (int m = 0; m < this.intColumnZoom; m++)
                                {
                                    switch (this.gradationMode)
                                    {
                                        case (enumGradation.mono):
                                            {
                                                if (data[j, i] > dbleInnerDataMax)
                                                {
                                                    p[2] = (byte)255.0;
                                                    p[1] = (byte)255.0;
                                                    p[0] = (byte)255.0;
                                                }
                                                else if (data[j, i] < dbleInnerDataMin)
                                                {
                                                    p[2] = (byte)0.0;
                                                    p[1] = (byte)0.0;
                                                    p[0] = (byte)0.0;
                                                }
                                                else
                                                {
                                                    p[2] = (byte)(((data[j, i] - (dbleDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin)) * 255.0));
                                                    p[1] = (byte)(((data[j, i] - (dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin)) * 255.0));
                                                    p[0] = (byte)(((data[j, i] - (dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin)) * 255.0));
                                                }
                                                break;
                                            }
                                        case (enumGradation.fullcolor1):
                                            {
                                                if (data[j, i] > dbleInnerDataMax)
                                                {
                                                    p[2] = (byte)255.0;
                                                    p[1] = (byte)0.0;
                                                    p[0] = (byte)0.0;
                                                }
                                                else if (data[j, i] < dbleInnerDataMin)
                                                {
                                                    p[2] = (byte)0.0;
                                                    p[1] = (byte)0.0;
                                                    p[0] = (byte)255.0;
                                                }
                                                else if (data[j, i] >= ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 2.0 + dbleInnerDataMin))
                                                {
                                                    p[2] = (byte)(((data[j, i] - ((dbleInnerDataMax + dbleInnerDataMin) / 2.0)) / ((dbleInnerDataMax - dbleInnerDataMin) / 2.0) * 255.0));
                                                    p[1] = (byte)(255 - ((data[j, i] - ((dbleInnerDataMax + dbleInnerDataMin) / 2.0)) / ((dbleInnerDataMax - dbleInnerDataMin) / 2.0) * 255.0));
                                                    p[0] = (byte)0;
                                                }
                                                else
                                                {
                                                    p[2] = (byte)0;
                                                    p[1] = (byte)((((data[j, i] - dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin) / 2.0) * 255.0));
                                                    p[0] = (byte)(255 - ((data[j, i] - (dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin) / 2.0) * 255.0));
                                                }
                                                break;
                                            }
                                        case (enumGradation.fullcolor2):
                                            {
                                                if (data[j, i] > dbleInnerDataMax)
                                                {
                                                    p[2] = (byte)255.0;
                                                    p[1] = (byte)0.0;
                                                    p[0] = (byte)0.0;
                                                }
                                                else if (data[j, i] < dbleInnerDataMin)
                                                {
                                                    p[2] = (byte)0.0;
                                                    p[1] = (byte)0.0;
                                                    p[0] = (byte)255.0;
                                                }
                                                else if (data[j, i] >= ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 3.0 + dbleInnerDataMin))
                                                {
                                                    p[2] = (byte)255;
                                                    p[1] = (byte)(255 - ((data[j, i] - ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 3.0 + dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin) / 4.0) * 255.0));
                                                    p[0] = (byte)0;
                                                }
                                                else if (data[j, i] >= ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 2.0 + dbleInnerDataMin))
                                                {
                                                    p[2] = (byte)(((data[j, i] - ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 2.0 + dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin) / 4.0) * 255.0));
                                                    p[1] = (byte)255;
                                                    p[0] = (byte)0;
                                                }
                                                else if (data[j, i] >= ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 1.0 + dbleInnerDataMin))
                                                {
                                                    p[2] = (byte)0;
                                                    p[1] = (byte)255;
                                                    p[0] = (byte)(255 - ((data[j, i] - ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 1.0 + dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin) / 4.0) * 255.0));
                                                }
                                                else
                                                {
                                                    p[2] = (byte)0;
                                                    p[1] = (byte)((((data[j, i] - dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin) / 4.0) * 255.0));
                                                    p[0] = (byte)255;
                                                }
                                                break;
                                            }
                                    }
                                    p += 3;
                                }
                            }
                            p += nResidual;
                        }
                    }

                    this.bmpGraph.UnlockBits(bmpData);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\r\nビットマップへのデータの描画に失敗しました。", "Glass:KimGraph Method:SetBmp");
                return false;
            }
        }

        //ビットマップへのデータ描画：二色バージョン
        private bool SetBmp(double[,] data1, double[,] data2)
        {
            try
            {
                Rectangle rect = new Rectangle(0, 0, this.intBmpColumn, this.intBmpRow);
                BitmapData bmpData = this.bmpGraph.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                int stride = bmpData.Stride;

                unsafe
                {
                    byte* p = (byte*)(void*)bmpData.Scan0;
                    int nResidual = stride - this.bmpGraph.Width * 3;

                    for (int i = 0; i < this.intDataRow; i++)
                    {
                        for (int n = 0; n < this.intRowZoom; n++)
                        {
                            for (int j = 0; j < this.intDataColumn; j++)
                            {
                                for (int m = 0; m < this.intColumnZoom; m++)
                                {
                                    p[1] = (byte)0.0;

                                    if (data1[j, i] > dbleData1Max)
                                    {
                                        p[0] = (byte)255.0;
                                    }
                                    else if (data1[j, i] < dbleData1Min)
                                    {
                                        p[0] = (byte)0.0;
                                    }
                                    else
                                    {
                                        p[0] = (byte)(((data1[j, i] - (this.dbleData1Min)) / ((this.dbleData1Max - this.dbleData1Min)) * 255.0));
                                    }

                                    if (data2[j, i] > dbleData2Max)
                                    {
                                        p[2] = (byte)255.0;
                                    }
                                    else if (data2[j, i] < dbleData2Min)
                                    {
                                        p[2] = (byte)0.0;
                                    }
                                    else
                                    {
                                        p[2] = (byte)(((data2[j, i] - (this.dbleData2Min)) / ((this.dbleData2Max - this.dbleData2Min)) * 255.0));
                                    }

                                    p += 3;
                                }
                            }
                            p += nResidual;
                        }
                    }

                    this.bmpGraph.UnlockBits(bmpData);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\r\nビットマップへのデータの描画に失敗しました。", "Glass:KimGraph Method:SetBmp");
                return false;
            }
        }


        //データの最大値最小値の取得
        public bool GetMaxMin(double[,] data,bool ExceptZero=false)
        {
            try
            {
                int column, row;

                //要素数の取得
                column = data.GetLength(0);
                row = data.GetLength(1);

                //最大値最小値の初期化
                this.dbleDataMax = -1.0e100;
                this.dbleDataMin = 1.0e100;

                //最大値最小値の取得
                for (int i = 0; i < column; i++)
                {
                    for (int j = 0; j < row; j++)
                    {
                        if (ExceptZero && data[i, j] == 0.0) continue;

                        if (data[i, j] > dbleDataMax)
                        {
                            dbleDataMax = data[i, j];
                        }
                        else if (data[i, j] < dbleDataMin)
                        {
                            dbleDataMin = data[i, j];
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\r\n最大値最小値の取得に失敗しました。", "Glass:KimGraph Method:GetMaxMin");
                return false;
            }
        }
    }

}
