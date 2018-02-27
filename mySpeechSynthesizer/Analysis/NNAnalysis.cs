using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace mySpeechSynthesizer
{
    

    /// <summary>
    /// 袅袅文件分析
    /// </summary>
    public class NNAnalysis
    {
        public static int samplingRate = 44100;
        private static string DecodeBase64(string code, string code_type = "utf-8")
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(code);
            try
            {
                decode = Encoding.GetEncoding(code_type).GetString(bytes);
            }
            catch
            {
                decode = code;
            }
            return decode;
        }

        /// <summary>
        /// 截取袅袅音源voice.d的指定部分
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="begin"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static byte[] readVoiceD(string directory, int begin = 0, int len = -1)
        {
            string file = directory + "voice.d";
            byte[] res;

            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                using (BinaryReader sr = new BinaryReader(fs))
                {
                    if (len < 0) len = (int)fs.Length;
                    res = new byte[len];
                    fs.Seek(begin, SeekOrigin.Begin);
                    res = sr.ReadBytes(len);
                }
            }
            return res;
        }

        /// <summary>
        /// 读入袅袅音源文件，格式化为NNTone序列格式
        /// </summary>
        /// <param name="directory">音源路径</param>
        /// <returns></returns>
        public static NNTone[] getParams(string directory)
        {
            string file = directory + "inf.d";
            List<NNTone> res = new List<NNTone>();
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        string nowstr = DecodeBase64(sr.ReadLine()).Replace("\r", "").Replace("\n", "");
                        var tmp = new NNTone(nowstr);
                        if(tmp.name!="error") res.Add(tmp);
                    }
                }
            }
            ////去掉前两行非语音标注的信息
            //res.RemoveAt(0);
            //res.RemoveAt(0);
            return res.ToArray();
        }

        /// <summary>
        /// 读入袅袅音源文件，格式化为ToneList形式
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static ToneList getToneList(string directory)
        {
            
            NNTone[] ntlist = getParams(directory);
            byte[] datas = readVoiceD(directory);

            ToneList tl = new ToneList(datas);

            

            foreach (var nt in ntlist)
            {
                //临时只取前几个，为了debug快
                // if (tl.tones.Count > 10) break;

                //byte[] tonedata = new byte[nt.length];
                //Array.Copy(datas, nt.begin, tonedata, 0, tonedata.Length);
                //int[] data = WAVControl.getSample(tonedata);
                tl.tones[nt.name] = new ToneUnit(nt.begin, nt.length, nt.pitch);
            }
            return tl;
        }
    }
}
