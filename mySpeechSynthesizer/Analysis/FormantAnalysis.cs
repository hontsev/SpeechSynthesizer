using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 共振峰合成
    /// </summary>
    class FormantAnalysis
    {
        /// <summary>
        /// 获取频谱包络
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static int[,] getEnvelope(double[] sample)
        {

            int dx = sample.Length / 200;
            int[,] res = new int[200, 2];
            int nowx = 0;
            //int beforemax = 0;
            //int beforep = 0;
            int index = 0;
            while (nowx < sample.Length)
            {
                int endx = nowx + dx;
                if (endx > sample.Length) endx = sample.Length;

                double[] tmp = new double[endx - nowx];
                Array.Copy(sample, nowx, tmp, 0, Math.Min(tmp.Length, endx - nowx));

                //
                //int[] maxres = getMax(tmp);
                int[] maxres = new int[2];
                maxres[0] = (endx + nowx) / 2;
                maxres[1] = (int)tmp.Average();

                if (index >= res.GetLength(0)) break;
                res[index, 0] = maxres[0];
                res[index, 1] = maxres[1];
                index++;
                //beforep=nowp;
                //beforemax=nowmax;
                nowx = endx;
            }

            return res;
        }

        /// <summary>
        /// 以硬件半波滤波的方式找出包络
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="rct"></param>
        /// <returns></returns>
        public static int[,] getEnvelope2(double[] datas, int rct = 100)
        {
            //int[] tmp = Array.ConvertAll(datas, d => (int)d);
            //return getEnvelope(tmp);
            ENVDetector env = new ENVDetector(rct);
            double[] envdata = env.env_full(datas);
            int[,] res = new int[envdata.Length, 2];
            for (int i = 0; i < res.GetLength(0); i++)
            {
                res[i, 0] = i;
                res[i, 1] = (int)envdata[i];
            }
            return res;
        }

        /// <summary>
        /// 中值法取得包络
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static int[,] getEnvelope3(double[] datas)
        {
            int width = 3;
            int[,] res = new int[datas.Length, 2];
            for (int i = 0; i < datas.Length; i++)
            {
                res[i, 0] = i;
                res[i, 0] = i;
            }

            for (int i = width; i < datas.Length - width; i++)
            {
                double[] windowData = new double[width * 2 + 1];
                Array.Copy(datas, i - width, windowData, 0, width * 2 + 1);
                Array.Sort(windowData);
                double middle = windowData[width];
                double ave = 0;
                foreach (var v in windowData) ave += v;
                ave /= windowData.Length;
                res[i, 1] = (int)middle;
            }
            for (int i = 0; i < width; i++)
            {
                res[i, 1] = res[width, 1];
            }
            for (int i = datas.Length - width; i < datas.Length; i++)
            {
                res[i, 1] = res[datas.Length - width - 1, 1];
            }
            return res;
        }

    }
}
