using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer.UTAULink
{
    class UBankManager
    {
        public static int samplingRate = 44100;

        private static string[] getSoundList(string path)
        {
            var files = Directory.GetFiles(path, "*.wav");

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
        public static Bank getToneList(string path)
        {
            Bank tlist = new Bank(new byte[] { });

            List<byte> alldata = new List<byte>();
            string[] filelist = getSoundList(path);

            foreach (var file in filelist)
            {
                byte[] tmpdata = WAVControl.getSampleByte(WAVControl.getSample(file));
                int begin = alldata.Count;
                int length = tmpdata.Length;
                string name = getSoundName(file);
                foreach (var t in tmpdata) alldata.Add(t);
                tlist.sunit[name] = new Syllable(begin, length, 66);
            }

            tlist.oridata = alldata.ToArray();

            return tlist;
        }
    }
}
