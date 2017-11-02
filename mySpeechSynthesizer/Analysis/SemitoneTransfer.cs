using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    public class SemitoneTransfer
    {

        /// <summary>
        /// 根据调号和音阶序号，算出对应的 MIDI 数
        /// </summary>
        /// <param name="Node">调号，C为0，B为11</param>
        /// <param name="Octave">音阶号，-1到9</param>
        /// <returns></returns>
        public static int getTN(int Node,int Octave)
        {
            int TN = (Octave + 1) * 10 + Node;
            return TN;
        }

        /// <summary>
        /// 根据 MIDI 数，返回对应的频率
        /// </summary>
        /// <param name="TN">MIDI数，0~127</param>
        /// <param name="add">偏移，一般为-1.0~1.0</param>
        /// <returns></returns>
        public static double getF(int TN, double add = 0.0)
        {
            double dTN = TN + add;
            return getF(dTN);
        }

        /// <summary>
        /// 根据 MIDI 数，返回对应的频率
        /// </summary>
        /// <param name="TN">MIDI数，0~127</param>
        /// <returns></returns>
        public static double getF(double TN)
        {
            double f = 440 * Math.Pow(2, (TN - 69) / 12);
            return f;
        }

        /// <summary>
        /// 根据频率，返回MIDI数
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static double getTN(double f)
        {
            if (f <= 0) return 0;
            double TN = Math.Log(f / 440, 2) * 12 + 69;
            return TN;
        }
    }
}
