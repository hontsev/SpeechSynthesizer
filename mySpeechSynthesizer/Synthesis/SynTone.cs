using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 合成单元
    /// </summary>
    public class SynTone
    {
        public string name;
        public int length;
        public double[][] pit;
        public double[] volume;

        public double pitch;

        /// <summary>
        /// 是否已生成波形数据的标识。
        /// 如果是true，就可以直接将之用于混音。
        /// 要注意，混音时会调用volume参数，而会无视掉name，length，pit参数。
        /// </summary>
        public bool isSynFinish;

        public int[] data;

        /// <summary>
        /// 在合成源处的偏移值，用于混音
        /// </summary>
        public int begin;

        public SynTone()
        {
            name = "";
            isSynFinish = true;
            pitch = 0;
            length = 1000;
            pit = new double[][] { new double[] { 0.5, 1 } };
            volume = new double[] { 1 };
            data = new int[] { };
        }

        /// <summary>
        /// 添加待合成的音素单元。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pit"></param>
        /// <param name="length"></param>
        /// <param name="volume"></param>
        /// <param name="begin"></param>
        /// <param name="pitch"></param>
        public SynTone(string name, double[][] pit,int length,double[] volume,int begin,double pitch)
        {
            isSynFinish = false;
            this.name = name;
            this.pit = pit;
            this.pitch = pitch;
            this.length = length;
            this.volume = volume;
            this.begin = begin;
            this.data = null;
        }

        /// <summary>
        /// 写回合成结果
        /// </summary>
        /// <param name="data"></param>
        public void setSynResult(int[] data)
        {
            isSynFinish = true;
            this.data = data;
        }


        /// <summary>
        /// 添加无需合成的音素单元。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="volume"></param>
        public SynTone(int[] data,int begin,double[] volume=null)
        {
            isSynFinish = true;
            this.data = data;
            if (volume == null) volume = new double[] { 1 };
            this.volume = volume;
            this.begin = begin;
            length = 0;
            name = null;
            pit = null;
            
        }
    }
}
