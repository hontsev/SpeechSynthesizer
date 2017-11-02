using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 分析主入口
    /// </summary>
    public class SoundAnalysis
    {
        public ToneList tl;
        public SoundAnalysis()
        {

        }

        /// <summary>
        /// 初始化音源
        /// </summary>
        /// <param name="directory"></param>
        public void init(string directory)
        {
            this.tl = analysisAll(directory);
        }

        /// <summary>
        /// 根据音节信息得到其合成波形
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public int[] getSoundTone(SynTone t)
        {
            var res = synthesis(tl, t.name, t.pit, t.length,t.pitch);
            return res;
        }

        /// <summary>
        /// 根据音节信息对其合成，合成结果写回原对象
        /// </summary>
        /// <param name="t"></param>
        public void synTone(SynTone t)
        {
            t.setSynResult(getSoundTone(t));
        }

        /// <summary>
        /// 根据单元信息来合成结果波形
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public int[] getSoundAll(List<SynTone> t)
        {
            List<int> res = new List<int>();

            for(int i = 0; i < t.Count; i++)
            {
                if (!t[i].isSynFinish) synTone(t[i]);

                //音量合成
                var data = VolumeSynthesis.getVolumeChangeResult(t[i].data, t[i].volume);

                for(int j = 0; j < data.Length; j++)
                {
                    int nowpos = j + t[i].begin;
                    if (res.Count > nowpos)
                        for (int k = res.Count; k < nowpos; k++) res.Add(0);
                    res[nowpos] = data[j];
                }
            }

            return res.ToArray();
        }




        /// <summary>
        /// 准备阶段。根据音源文件来切分基音段等
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static ToneList analysisAll(string directory)
        {
            ToneList tl = NNAnalysis.getToneListFromNN(directory);
           // List<string> keys = new List<string>();
            //foreach (var key in tl.tones.Keys) keys.Add(key);
            //for(int i = 0; i < keys.Count; i++)
            //{
            //    analysis(tl, keys[i]);
            //}

            return tl;
        }

        ///// <summary>
        ///// 对单个声音片段进行分析
        ///// </summary>
        ///// <param name="tl"></param>
        ///// <param name="tonename"></param>
        //public static void analysis(ToneList tl,string tonename)
        //{
        //    try
        //    {
        //        ToneUnit tu = tl.tones[tonename];
                
        //        //将其转化为其有声片段
        //        //tu.data = FToneAnalysis.getSoundPart(tu.data);
        //        //将其切分为多个基音片段
        //        //var cut = FToneAnalysis.cutIt(tu.data);
        //        //tu.cut = cut;
        //        tl.tones[tonename] = tu;
        //    }
        //    catch (Exception e)
        //    {

        //    }
            
        //}



        /// <summary>
        /// 合成
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pit"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static int[] synthesis(ToneList tl,string name,double[] pit,int length,double pitch)
        {
            int[] res = WidthSynthesis.getWAVdata(tl, name, pit, length,pitch);
            
            return res;
        }

    }




















}
