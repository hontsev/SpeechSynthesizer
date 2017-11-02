using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 音量调整
    /// </summary>
    class VolumeSynthesis
    {


        /// <summary>
        /// 获取与音量曲线一一对应的音量缩放比例
        /// </summary>
        /// <param name="len"></param>
        /// <param name="zipdata"></param>
        /// <returns></returns>
        public static double[] getZipPoint(int len, double[] zipdata)
        {
            double[] res = new double[len];

            if (zipdata.Length == 0)
            {
                for (int i = 0; i < len; i++) res[i] = 1;
            }
            else if (zipdata.Length == 1)
            {
                for (int i = 0; i < len; i++) res[i] = zipdata[0] ;
            }
            else if (zipdata.Length == 2)
            {
                for (int i = 0; i < len; i++) res[i] = zipdata[0] + ((zipdata[1] - zipdata[0]) * i / zipdata.Length);
            }
            else
            {
                mPoint[] zippoints = new mPoint[zipdata.Length];
                for (int i = 0; i < zipdata.Length; i++) zippoints[i] = new mPoint();
                for (int i = 0; i < zipdata.Length; i++)
                {
                    zippoints[i].x = len * i / (zipdata.Length - 1);
                    zippoints[i].y = (int)(zipdata[i] * 10000);
                }
                mPoint[] zipres = BezierControl.createCurve(zippoints);
                for (int i = 0; i < len; i++) res[i] = (double)zipres[i].y / 10000;
            }

            return res;
        }


        /// <summary>
        /// 按比例增减振幅，来改变相对音量
        /// </summary>
        /// <param name="data"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static int[] getVolumeChangeResult(int[] data,double[] scale)
        {
            int[] res = new int[data.Length];
            double[] dscale = getZipPoint(data.Length, scale);
            for (int i = 0; i < data.Length; i++)
            {
                res[i] = (int)((double)data[i] * dscale[i]);
            }
            return res;
        }
    }
}
