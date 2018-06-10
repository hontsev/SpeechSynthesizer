using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 音素
    /// </summary>
    public class Phone
    {
        public int begin;
        public int length;
        //public int 
        public double f1;
        public double f2;
        public double f3;
    }

    /// <summary>
    /// 音节
    /// </summary>
    public class Syllable
    {
        /// <summary>
        /// 原始音波数据
        /// </summary>
        //public int[] data;

        /// <summary>
        /// 分割后的基准单元
        /// </summary>
        public List<Phone> punit;
        public int begin;
        public int length;
        public double pitch;

        public Syllable(int begin,int length,double pitch)
        {
            this.punit = new List<Phone>();
            this.begin = begin;
            this.length = length;
            this.pitch = pitch;
            //this.data = odata;
        }
    }

    

    /// <summary>
    /// 音色合成数据
    /// </summary>
    public class Bank
    {
        public Dictionary<string, Syllable> sunit;
        public Dictionary<string, int[]> wav;
        public byte[] oridata;

        public int[] getTone(string name)
        {
            if (this.sunit.ContainsKey(name))
            {
                return WAVControl.getSample(oridata, sunit[name].begin, sunit[name].length);
            }
            return new int[] { };
        }

        public Bank(byte[] datas)
        {
            this.sunit = new Dictionary<string, Syllable>();
            this.oridata = datas;
        }
    }
}
