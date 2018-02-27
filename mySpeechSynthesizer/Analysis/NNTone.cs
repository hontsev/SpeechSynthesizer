using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 袅袅文件内的每个音的标记方式
    ///形如   a 0 75492 3774 33972 412.1 7984 9196
    ///表示：音名，wav起始帧，wav结束帧，声母开始帧，音尾开始帧，音高（Hz），声母音量强度，整音音量强度。
    ///顺便一提，NN所用音源wav的固定采样率是44100hz
    /// </summary>
    public class NNTone
    {
        /// <summary>
        /// 音名
        /// </summary>
        public string name;
        /// <summary>
        /// 起始帧
        /// </summary>
        public int begin;
        /// <summary>
        /// 帧长
        /// </summary>
        public int length;
        /// <summary>
        /// 母音起始帧
        /// </summary>
        public int inibegin;
        /// <summary>
        /// 尾帧
        /// </summary>
        public int tailbegin;
        /// <summary>
        /// 音高
        /// </summary>
        public double pitch;
        /// <summary>
        /// 基音强度
        /// </summary>
        public int inivolume;
        /// <summary>
        /// 音强
        /// </summary>
        public int volume;

        public NNTone()
        {

        }

        public NNTone(string oristr)
        {
            try
            {
                //切割字符串
                string[] data = oristr.Split(' ');
                name = data[0];
                begin = int.Parse(data[1]);
                length = int.Parse(data[2]);
                if (length <= 0) throw new Exception("now valid line");
                inibegin = int.Parse(data[3]);
                tailbegin = int.Parse(data[4]);
                pitch = double.Parse(data[5]);
                inivolume = int.Parse(data[6]);
                volume = int.Parse(data[7]);
            }
            catch
            {
                name = "error";
            }

        }
    }
}
