using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 汉语合成
    /// </summary>
    class HanSynthesis
    {



        //private static int[] getZipSound(int[] origin, int newlen, double zippoint, double overlapcut)
        //{
        //    //ori端点焊接
        //    int len = (int)(origin.Length * overlapcut);
        //    int[] tmp = new int[len];
        //    for (int i = 0; i < len; i++)
        //    {
        //        tmp[i] = (int)(origin[i] * ((double)i / len) + origin[origin.Length - len + i] * (1 - (double)i / len));
        //    }
        //    int[] neworigin = new int[origin.Length - len];
        //    //Array.Copy(tmp, len / 2, neworigin, 0, len / 2);
        //    Array.Copy(origin, 0, neworigin, 0, origin.Length - len );
        //    Array.Copy(tmp, 0, neworigin, neworigin.Length - len, len);

        //    int[] res = new int[newlen];
        //    double j = 0;
        //    for (int i = 0; i < newlen; i++)
        //    {
        //        res[i] = neworigin[(int)(j) % neworigin.Length];
        //        j += zippoint;
        //    }
        //    return res;
        //}

    }
    }
