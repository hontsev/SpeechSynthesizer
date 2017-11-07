using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 基音分析
    /// </summary>
    public class FToneAnalysis
    {

        //private static int[] getMax(int[] datas)
        //{
        //    //int[] tmp = new int[endx - nowx];
        //    int nowmax = int.MinValue;
        //    int nowp = 0;
        //    for (int i = 0; i < datas.Length; i++)
        //    {
        //        if (datas[i] > nowmax)
        //        {
        //            nowmax = datas[i];
        //            nowp = i;
        //        }
        //    }
        //    return new int[] { nowp, nowmax };
        //}


        //private static int[] getMin(int[] datas)
        //{
        //    //int[] tmp = new int[endx - nowx];
        //    int nowmin = int.MaxValue;
        //    int nowp = 0;
        //    for (int i = 0; i < datas.Length; i++)
        //    {
        //        if (datas[i] < nowmin)
        //        {
        //            nowmin = datas[i];
        //            nowp = i;
        //        }
        //    }
        //    return new int[] { nowp, nowmin };
        //}
        public static int getmin2x(int orilen)
        {
            int res = 2;
            while (res < orilen) res *= 2;
            return res;
        }


        /// <summary>
        /// 频率滤波。传入要保留的起止频率
        /// </summary>
        /// <param name="data"></param>
        /// <param name="beginf"></param>
        /// <param name="endf"></param>
        /// <returns></returns>
        public static int[] FrequencyFilter(int[] data, double beginf, double endf)
        {
            int k = data.Length;
            int dx = getmin2x(k);
            double dl = (double)NNAnalysis.samplingRate/ dx;
            double[] data1 = new double[dx];
            double[] dataimag1 = new double[dx];
            Array.Copy(data, data1, k);
            TWFFT.FFT(data1, dataimag1);

            //double[] data1t = new double[dx];
            //for (int i = 0; i < dx; i++) data1t[i] = Math.Sqrt(data1[i] * data1[i] + dataimag1[i] * dataimag1[i]);
            int beginx = (int)(beginf / dl);
            int endx = (int)(endf / dl);
            for (int i = 0; i < dx; i++)
            {
                if (i < beginx || i > dx - beginx || (i > endx && i < dx - endx))
                {
                    data1[i] = 0;
                    //dataimag1[i] = 0;
                }
            }

            TWFFT.IFFT(data1, dataimag1);
            int[] res = new int[k];
            for (int i = 0; i < k; i++)
            {
                res[i] = (int)data1[i];
            }
            return res;
        }

        /// <summary>
        /// 中值滤波。number务必传入正奇数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int[] MiddleFilter(int[] data,int number=3)
        {
            int[] res = new int[data.Length];
            for(int i = 0; i < number / 2; i++)
            {
                res[i] = data[i];
                res[data.Length - 1 - i] = data[data.Length - 1 - i];
            }
            for(int i = number / 2; i < data.Length - number / 2; i++)
            {
                int[] tmp = new int[number];
                Array.Copy(data, i - number / 2, tmp, 0, number);
                res[i] = getMiddle(tmp);
            }
            return res;
        }

        public static double[] MiddleFilter(double[] data,int number = 3)
        {
            double[] res = new double[data.Length];
            for (int i = 0; i < number / 2; i++)
            {
                res[i] = data[i];
                res[data.Length - 1 - i] = data[data.Length - 1 - i];
            }
            for (int i = number / 2; i < data.Length - number / 2; i++)
            {
                double[] tmp = new double[number];
                Array.Copy(data, i - number / 2, tmp, 0, number);
                res[i] = getMiddle(tmp);
            }
            return res;
        }

        /// <summary>
        /// 获取平均数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double getAverage(double[] data)
        {
            if (data.Length <= 0) return 0;
            double ave = 0;
            for (int i = 0; i < data.Length; i++) { ave += data[i]; }
            ave /= data.Length;
            return ave;
        }


        /// <summary>
        /// 获取中位数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static double getMiddle(double[] data, double percent = 0.5)
        {
            double[] tmp = new double[data.Length];
            Array.Copy(data, tmp, data.Length);
            Array.Sort(tmp);
            int index = (int)(data.Length * percent);
            return tmp[index];
        }

        /// <summary>
        /// 获取中位数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static int getMiddle(int[] data, double percent = 0.5)
        {
            double[] tmp = new double[data.Length];
            Array.Copy(data, tmp, data.Length);
            Array.Sort(tmp);
            int index = (int)(data.Length * percent);
            return (int)tmp[index];
        }

        /// <summary>
        /// 获取波形自相似曲线
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int getACF(int[] data,int begin,int length)
        {
            List<double> acf = new List<double>();
            for (int k = 0; k < length; k++)
            {
                double sum = 0.0;
                for (int i = 0; i < length; i++)
                {
                    if (begin + i + k >= data.Length) sum += 0;
                    else sum = sum + data[begin+i] * data[begin+i + k];
                }
                acf.Add(sum);
            }

            // find the max one  
            double max = Double.MinValue;
            int index = 0;
            for (int k = 0; k < length; k++)
            {
                if (k > 80 && acf[k] > max)
                {
                    max = acf[k];
                    index = k;
                }
            }
            //return (short)sampleRate / index;  


            return index;
        }



        /// <summary>
        /// 取得能量曲线
        /// </summary>
        /// <param name="data"></param>
        /// <param name="beginx"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static double[] Energy(int[] data, int window = 25)
        {
            double[] res = new double[data.Length - window];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = 0;
                for (int j = 0; j < window; j++)
                {
                    res[i] += Math.Pow(Math.Abs(data[i + j]), 2);
                }
                res[i] /= window;
                //res[i] = Math.Log(res[i]);
            }
            return res;

        }

        /// <summary>
        /// 获取某个区间上的最大值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="beginindex"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private static int getMaxIndex(int[] data,int beginindex,int width=3)
        {
            int res = beginindex - width;
            int maxval = int.MinValue;
            for(int i = Math.Max(0, res); i < Math.Min(data.Length, beginindex + width);i++)
            {
                if (data[i] > maxval)
                {
                    maxval = data[i];
                    res = i;
                }
            }
            return res;
        }


        public static int[] getCutList(int[] data, int beginx, int window = 200)
        {
            List<int> tmpcuts = new List<int>();

            List<int> cuts = new List<int>();
            //初步获得每个窗的基音周期
            for (int i = 0; i < data.Length - window - beginx; i += window)
            {
                tmpcuts.Add(getACF(data, beginx+i, window));

                //double tmp1 = 0;
                //double tmp2 = 0;
                //double tmp3 = 0;
                //for (int j = i; j < window - i; j++)
                //{
                //    tmp1 += data[j] * data[j + i];
                //    tmp2 += data[j] * data[j];
                //    tmp3 += data[j + i] * data[j + i];
                //}
                //if (tmp2 * tmp3 == 0) rm[i] = 0;
                //else rm[i] = Math.Max(Math.Min(tmp1 / Math.Sqrt(tmp2 * tmp3), 1.0), 0.0);
            }
            // 中值滤波来平滑周期
            var tmparray = MiddleFilter(tmpcuts.ToArray(),3);
            int minc = getMiddle(tmparray, 0.4);

            // 选择周期中最大值附近的参数作为切割点
            int baseindex = getMaxIndex(data, beginx+window/2, window / 2);

            cuts.Add(0);
            cuts.Add(baseindex);
            for (int i = 0; i < tmparray.Length; i++)
            {
                //if (tmparray[i] < minc && minc-tmparray[i]>10) { baseindex += tmparray[i]; continue; }
                while (baseindex < beginx + i * window)
                {
                    baseindex = getMaxIndex(data, baseindex + minc, window / 8);
                    cuts.Add(baseindex);
                }

                
            }
            cuts.Add(data.Length);
            return cuts.ToArray();
        }

        /// <summary>
        /// 短时过零率
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static int getPassZeroBegin(int[] data, int beginnum=0, int windowlen=220)
        {
            List<int> pzlist = new List<int>();
            for(int i = beginnum; i < data.Length-windowlen; i+=windowlen)
            {
                int passzeron = 0;
                for(int j = 1; j < windowlen; j++)
                {
                    if (data[i+j - 1] * data[i+j] <= 0) passzeron++;
                }
                pzlist.Add(passzeron);

            }
            int middle = getMiddle(pzlist.ToArray(), 0.9);
            int cutbegin = 0;
            for(int i = 0; i < pzlist.Count-1; i++)
            {
                int maxTime = Math.Max(middle - 1, 10);
                if (pzlist[i] < maxTime && pzlist[i + 1] < maxTime)
                {
                    cutbegin = beginnum + i * windowlen;
                    break;
                }
            }
            return cutbegin;
        }



        /// <summary>
        /// 获取有声片段
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int[] getSoundPart(int[] data)
        {
            
            int beginx = 0;
            int endx = 0;
            double[] energy = Energy(data);

            double ave = getMiddle(energy, 0.2);
            ave *= 0.2;
            ave = Math.Max(ave, 10000);
            

            int len = 3;

            //找有声片段的开始时刻
            for (int i = 0; i < energy.Length - len; i++)
            {
                bool getit = true;
                for (int j = 0; j < len; j++)
                {
                    if (energy[i + j] < ave)
                    {
                        getit = false;
                    }
                }
                if (getit)
                {
                    beginx = i;
                    break;
                }
            }
            //找有声片段的结尾时刻
            for (int i = energy.Length - len - 1; i > beginx; i--)
            {
                bool getit = true;
                for (int j = 0; j < len; j++)
                {
                    if (energy[i + j] < ave)
                    {
                        getit = false;
                    }
                }
                if (getit)
                {
                    endx = i;
                    break;
                }
            }

            int newlen = endx - beginx;
            int[] res = new int[newlen];
            Array.Copy(data, beginx, res, 0, newlen);
            return res;
        }


        

        /// <summary>
        /// 基音切割
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int[] cutIt(int[] data)
        {
            
            List<int> index = new List<int>();

            //获取声音部分
            //data = getSoundPart(data);

            //滤波60-900hz
            data = FrequencyFilter(data, 60, 900);

            //index.Add(0);

            int begin = getPassZeroBegin(data, 0);
            return getCutList(data, begin, 440);

            //return index.ToArray();
        }

        /// <summary>
        /// 对波形进行切割（切割成各个基音波形），返回切割点在原数据数组里的位置
        /// 弃用
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int[] cutItOld(int[] data)
        {
            int k = data.Length;

            // 分别计算波形正负方向的各自中值，用于找出峰值

            List<int> posdata = new List<int>();
            List<int> negdata = new List<int>();
            foreach (int d in data) if (d > 0) posdata.Add(d);
            foreach (int d in data) if (d < 0) negdata.Add(d);
            int posaverage = getMiddle(posdata.ToArray(), 0.6);
            int negaverage = getMiddle(negdata.ToArray(), 0.6);

            // 存储波形数据中那些作为波峰预选的帧的位置（高通滤波）
            List<int> tmpdata = new List<int>();
            for (int i = 0; i < k; i++)
            {
                if (data[i] > posaverage || data[i] < negaverage) tmpdata.Add(i);
            }


            // 以相邻两峰符号相反（且前正后负）作为判断切割点的准则

            List<int> tmpdata2 = new List<int>();
            for (int i = 1; i < tmpdata.Count; i++)
            {
                if (data[tmpdata[i]] * data[tmpdata[i - 1]] < 0 && data[tmpdata[i - 1]] > 0)
                {
                    tmpdata2.Add(tmpdata[i]);
                }
            }

            //暂存相邻两峰的间距，用于筛选出有用峰

            List<int> tmpdata3 = new List<int>();
            tmpdata3.Add(0);
            for (int i = 1; i < tmpdata2.Count; i++)
            {
                tmpdata3.Add(tmpdata2[i] - tmpdata2[i - 1]);
            }


            //double delta = getMid(tmpdata3.ToArray(), 0.5);
            //相邻两峰过近或过远都舍弃掉
            for (int i = 0; i < tmpdata3.Count; i++)
            {
                if (tmpdata3[i] > 200 || tmpdata3[i] < 50)
                {
                    tmpdata3[i] = -1;
                }
            }

            // 剔除异常值
            List<int> tmpdata3minmax = new List<int>();
            foreach (var i in tmpdata3) if (i > 0) tmpdata3minmax.Add(i);
            int max = tmpdata3minmax.Max();
            int min = tmpdata3minmax.Min();
            List<double> tmpdata4 = new List<double>();
            foreach (int i in tmpdata3) tmpdata4.Add((double)(i - min) / (max - min));

            List<int> res = new List<int>();
            double scale = 0.9;
            while (true)
            {
                int num = 0;
                for (int i = 0; i < tmpdata4.Count; i++)
                {
                    if (tmpdata4[i] > scale) num++;
                }
                if ((double)num / tmpdata4.Count > 0.3) break;
                else scale -= 0.1;
            }
            for (int i = 0; i < tmpdata4.Count; i++)
            {
                if (tmpdata4[i] > scale)
                {
                    res.Add(tmpdata2[i]);
                }
            }


            return res.ToArray();
        }



    }
}
