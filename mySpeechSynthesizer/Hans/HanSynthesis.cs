using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{

    public class HanSynthesis
    {

        private PinYinConverter pyconv;

        public double soundheight;
        public double soundSpeed;

        NNTone[] sounds;
        SoundAnalysis sa = new SoundAnalysis();
        public string filepath = "";
        string output = @"output\tmp.wav";
        string outputOri = @"output\tmp_origin.wav";
        string outputTone = @"output\tmp_tone.wav";
        public double defaultpitch = 69;

        public HanSynthesis(string sourcePath,int speed=120,int height=100,int pitch=69)
        {
            pyconv = new PinYinConverter();
            soundSpeed = speed;
            soundheight = height;
            defaultpitch = pitch;
            //init
            filepath = sourcePath;

            //string soundpath = @"D:\Program Files\UTAU\voice\DFZZ_CHN.Autumn\-";
            //soundpath = @"D:\Program Files\袅袅虚拟歌手v2.2%28附余袅袅音源%29\袅袅虚拟歌手v2.2\src\枸杞子音源库_v1.0";
            

            sa.init(filepath, SourceType.Niaoniao);
            sounds = sa.getNNTonesFromToneList();

        }

 

        public double[] getZipDatas(int toneNum, int beforeToneNum = -1, int nextToneNum = -1)
        {
            double[] datas;
            switch (toneNum)
            {
                case 1:
                    if (nextToneNum == 5)
                    {
                        datas = new double[] { 1.1, 1.1, 0.9 };
                    }
                    else
                    {
                        datas = new double[] { 1 };
                    }

                    break;
                case 2:
                    datas = new double[] { 0.9, 1 };
                    break;
                case 3:
                    datas = new double[] { 0.95, 0.88, 0.9 };
                    break;
                case 4:
                    datas = new double[] { 1.1, 0.98, 0.92 };
                    break;
                case 5:
                default:
                    datas = new double[] { 0.9, 0.87 };
                    break;
            }

            //datas = new int[2] { 70, 130 };

            return datas;
        }



        public int[] getSoundData(string name, double[] pitdata, int len, double[] volume)
        {
            SynTone st = new SynTone(name, pitdata, len, volume, 0, defaultpitch);
            var res = sa.getSoundTone(st);
            return res;
        }

        public void writeWAV(int[] wavdata, string filename = null)
        {
            if (filename == null) filename = outputTone;
            WAVControl.writeWAV(wavdata, filename);
        }

        public void playSound(string filename)
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(filename);
            player.PlaySync();
        }

        public int[] showSound(string str)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string[] sentences = pyconv.cutSentencesAll(str);
            sw.Stop();
            int num = 0;
            int nowindex = 0;


            List<int> allres = new List<int>();

            foreach (var sentence in sentences)
            {
                for (int s = 0; s < 10000 * ((double)100 / soundSpeed); s++)
                {
                    allres.Add(0);
                }
                List<List<string>> pinyin = pyconv.getPinYinList(sentence);
                foreach (var p in pinyin)
                {
                    int d;
                    int duration = 500;
                    for (int i = 0; i < p.Count; i++)
                    {
                        if (i < p.Count - 1)
                            //是一个词的中间字，因此间隔小
                            d = 0;
                        else
                            d = 1;

                        if (p[i].EndsWith("5"))
                        {
                            //轻声
                            duration = 425;
                            //if (p.Count == 1 && index > 0) index -= 1;
                        }
                        else if (p[i].EndsWith("3"))
                        {
                            //上声，念得长
                            duration = 575;
                        }
                        else if (p[i].EndsWith("4") && i == p.Count - 1)
                        {
                            //去声且在结尾，念得短
                            duration = 500;
                        }
                        else if (p[i].EndsWith("2") && i < p.Count - 1)
                        {
                            //阳平且在句中，念得短
                            duration = 475;
                        }
                        else
                        {
                            duration = 500;
                        }
                        string beforep = "";
                        int beforetonenum = -1;
                        if (i >= 1)
                        {
                            beforep = p[i - 1];
                            beforetonenum = int.Parse(beforep.Substring(beforep.Length - 1));
                        }
                        string nextp = "";
                        int nexttonenum = -1;
                        if (i < p.Count - 1)
                        {
                            nextp = p[i + 1];
                            nexttonenum = int.Parse(nextp.Substring(nextp.Length - 1));
                        }
                        int tonenum = int.Parse(p[i].Substring(p[i].Length - 1));
                        string ch = p[i].Substring(0, p[i].Length - 1);

                        double sneeze = 0.22;
                        double part = 1.0 * (sneeze / 2);
                        int[] thisSoundFrame = getSoundData(ch, getZipDatas(tonenum, beforetonenum, nexttonenum), (int)(duration / (soundSpeed / 100)), new double[] { this.soundheight / 100 });
                        for (int k = 0; k < thisSoundFrame.Length; k++)
                        {
                            thisSoundFrame[k] = (int)((double)thisSoundFrame[k] * soundheight / 100);
                        }

                        int partlen = (int)(thisSoundFrame.Length * part);

                        for (int k = 0; k < partlen; k++)
                        {
                            allres[allres.Count - partlen + k] = (int)(allres[allres.Count - partlen + k] * 1 + thisSoundFrame[k] * 1);

                        }
                        for (int k = partlen; k < thisSoundFrame.Length; k++)
                        {
                            allres.Add(thisSoundFrame[k]);
                        }


                        if (d > 0)
                        {
                            for (int s = 0; s < 4200 * ((double)100 / soundSpeed); s++)
                            {
                                allres.Add(0);
                            }
                        }
                        num++;
                    }
                }
            }


            return allres.ToArray();
        }

        public int outputCount = 0;
        public bool isReading = false;
        public void createMYSSwavs(object sentences)
        {
            string bufferpath = @"tmp_talking\";
            string[] sentence = sentences as string[];
            for (int i = 0; i < sentence.Length; i++)
            {
                int count = Interlocked.Increment(ref outputCount);
                if (!isReading) break;
                string filename = string.Format(@"{0}tmp_{1}.wav", bufferpath, count);
                int[] tmp = showSound(sentence[i]);
                writeWAV(tmp, filename);
            }
        }

        /// <summary>
        /// 主入口，将一句话朗读出来。会阻塞，建议用额外线程来运行
        /// </summary>
        /// <param name="str"></param>
        public void readSentence(object str)
        {
            isReading = true;
            //setGUIStatus(false);
            //print("开始合成");
            PinYinConverter pyconv = new PinYinConverter();
            string[] sentences = pyconv.cutSentencesAll(str as string);
            //print(string.Format("拆分句子完毕：共{0}个短句", sentences.Length));

            string bufferpath = @"tmp_talking\";
            if (Directory.Exists(bufferpath)) Directory.Delete(bufferpath, true);
            Directory.CreateDirectory(bufferpath);
            new Thread(createMYSSwavs).Start(sentences);
            for (int i = 0; i < sentences.Length; i++)
            {
                try
                {
                    if (!isReading) break;
                    string filename = string.Format(@"{0}tmp_{1}.wav", bufferpath, i);
                    //int[] tmp = myssc.showSound(sentences[i], this.print);
                    while (!File.Exists(filename)) if (!isReading) break;
                    playSound(filename);
                }
                catch
                {
                    //break;
                }

            }
            //setGUIStatus(true);
            //isReading = true;
            isReading = false;
        }
    }
}
