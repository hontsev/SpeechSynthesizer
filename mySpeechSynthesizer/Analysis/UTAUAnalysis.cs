using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace mySpeechSynthesizer
{
    public class UTAUAnalysis
    {
        public static int samplingRate = 44100;

        private static string[] getSoundList(string path)
        {
            var files = Directory.GetFiles(path,"*.wav");

            return files;
        }

        private static string getSoundName(string filename)
        {
            string tmpname = Path.GetFileNameWithoutExtension(filename);

            return tmpname;
        }



        /// <summary>
        /// 从UTAU音源文件夹获取音源数据
        /// 没有pitch
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ToneList getToneList(string path)
        {
            ToneList tlist = new ToneList(new byte[] { });

            List<byte> alldata = new List<byte>();
            string[] filelist = getSoundList(path);

            foreach (var file in filelist)
            {
                byte[] tmpdata = WAVControl.getSampleByte(WAVControl.getSample(file));
                int begin = alldata.Count;
                int length =  tmpdata.Length;
                string name = getSoundName(file);
                foreach (var t in tmpdata) alldata.Add(t);
                tlist.tones[name] = new ToneUnit(begin, length, 66);
            }

            tlist.oridata = alldata.ToArray();

            return tlist;
        }
    }
}
