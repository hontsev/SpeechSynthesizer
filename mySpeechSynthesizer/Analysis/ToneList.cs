using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 声音波形合成基准单元
    /// </summary>
    public class FTone
    {
        public int begin;
        public int length;
        //public int 
        public double f1;
        public double f2;
        public double f3;
    }

    /// <summary>
    /// 音频片段
    /// </summary>
    public class ToneUnit
    {
        /// <summary>
        /// 原始音波数据
        /// </summary>
        //public int[] data;

        /// <summary>
        /// 分割后的基准单元
        /// </summary>
        public List<FTone> cut;
        public int begin;
        public int length;
        public double pitch;

        public ToneUnit(int begin,int length,double pitch)
        {
            this.cut = new List<FTone>();
            this.begin = begin;
            this.length = length;
            this.pitch = pitch;
            //this.data = odata;
        }
    }

    /// <summary>
    /// 音色合成数据
    /// </summary>
    public class ToneList
    {
        public Dictionary<string, ToneUnit> tones;
        public byte[] oridata;

        public int[] getTone(string name)
        {
            if (this.tones.ContainsKey(name))
            {
                return WAVControl.getSample(oridata, tones[name].begin, tones[name].length);
            }
            return new int[] { };
        }

        public ToneList(byte[] datas)
        {
            this.tones = new Dictionary<string, ToneUnit>();
            this.oridata = datas;
        }
    }
}
