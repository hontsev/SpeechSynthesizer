using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Net;
using System.Web;
using mySpeechSynthesizer;
using System.Threading;
using System.Diagnostics;

namespace nntest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        NNTone[] sounds;
        string filepath = "";
        string output = Application.StartupPath + @"\tmp.wav";
        string outputOri = Application.StartupPath + @"\tmp_origin.wav";
        string outputTone = Application.StartupPath + @"\tmp_tone.wav";

        SoundAnalysis sa;

        public static Image getImage(int[] data, double begin = 0, double end = 1)
        {
            data = FToneAnalysis.getSoundPart(data);

            int sampleLength = data.Length;

            int begink = (int)(sampleLength * begin);
            int endk = (int)(sampleLength * end);

            int w = (endk - begink) * 2;
            int h = 200;
            Bitmap bitmap = new Bitmap(w, h);

            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //draw x,y
            g.DrawLine(new Pen(Color.Black, 5), new Point(0, 0), new Point(0, h));
            g.DrawLine(new Pen(Color.Black, 5), new Point(0, h / 2), new Point(w, h / 2));

            //draw wave
            for (int i = begink; i < endk; i++)
            {
                Pen p = new Pen(Color.Green, 1);
                g.DrawLine(p,
                    new Point((int)(((double)i - begink) * w / (endk - begink)), h / 2),
                    new Point((int)(((double)i - begink) * w / (endk - begink)), h / 2 + (int)((double)data[i] / 60 / 10)));

            }

            //draw cut
            
            int[] res = FToneAnalysis.cutIt(data);
            for (int i = 1; i < res.Length; i++)
            {
                if (res[i] > 0.5)
                {
                    g.DrawLine(new Pen(Color.Red, 2),
                    new Point((int)(((double)res[i] - begink) * w / (endk - begink)), 0),
                    new Point((int)(((double)res[i] - begink) * w / (endk - begink)), h));
                }
            }

            return bitmap;
        }

        ///// <summary>
        ///// 绘制调试图像
        ///// </summary>
        ///// <param name="sample"></param>
        ///// <param name="begin"></param>
        ///// <param name="end"></param>
        ///// <returns></returns>
        //public static Image getImage(ToneUnit tone, double begin = 0, double end = 1)
        //{

        //    int sampleLength = tone.data.Length;

        //    int begink = (int)(sampleLength * begin);
        //    int endk = (int)(sampleLength * end);

        //    int w = (endk - begink) * 2;
        //    int h = 200;


        //    //int k = endk - begink;

        //    Bitmap bitmap = new Bitmap(w, h);

        //    Graphics g = Graphics.FromImage(bitmap);
        //    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        //    //draw x,y
        //    g.DrawLine(new Pen(Color.Black, 5), new Point(0, 0), new Point(0, h));
        //    g.DrawLine(new Pen(Color.Black, 5), new Point(0, h/2), new Point(w, h/2));


        //    //int average = getAverage(sample) / 60 + 290;
        //    //g.DrawLine(new Pen(Color.Black, 5), new Point(0, average), new Point(2000, average));



        //    ////draw acf
        //    //int windowlen = 220;
        //    //for (int i = windowlen; i < k; i += windowlen)
        //    //{
        //    //    int begin = i - windowlen;
        //    //    int end = i >= k ? k - 1 : i;
        //    //    int[] tmp = new int[windowlen];
        //    //    Array.Copy(sample, begin, tmp, 0, end - begin);
        //    //    int maxindex = begin + getACF(tmp);
        //    //    g.DrawLine(new Pen(Color.Yellow, 2), new Point(begin * bitmap.Width / k, 0), new Point(begin * bitmap.Width / k, 540));
        //    //    g.DrawLine(new Pen(Color.Yellow, 2), new Point(end * bitmap.Width / k, 0), new Point(end * bitmap.Width / k, 540));
        //    //    g.DrawLine(new Pen(Color.Red, 10),
        //    //        new Point(maxindex * bitmap.Width / k, 0),
        //    //        new Point(maxindex * bitmap.Width / k, 540));
        //    //}

        ////draw frame cut
        //int cutbegin = sample.Length / 4;
        //int cutend = sample.Length * 3 / 4;
        //int[] data = new int[cutend - cutbegin];
        //Array.Copy(sample, cutbegin, data, 0, data.Length);




        //    //int[] res = SoundAnalysis.cutIt(tone.data);
        //    //for (int i = 1; i < res.Length; i++)
        //    //{
        //    //    if (res[i] > 0.5)
        //    //    {
        //    //        g.DrawLine(new Pen(Color.Red, 2),
        //    //        new Point((int)(((double)res[i] - begink) * w / (endk - begink)), 0),
        //    //        new Point((int)(((double)res[i] - begink) * w / (endk - begink)), h));
        //    //    }
        //    //}


        //    ////draw 3-berier
        //    //int[] datas = new int[] { 10, 20, 50, 150, 20, 100, 150, 20, 30, 200, 20, 100, 50 };
        //    //mPoint[] points = new mPoint[datas.Length];
        //    //for (int i = 0; i < points.Length; i++)
        //    //{
        //    //    points[i] = new mPoint();
        //    //    points[i].x = i * 100;
        //    //    points[i].y = datas[i] * 2;
        //    //}
        //    //mPoint[] res1 = WAVAnalyzer.createCurve(points);
        //    //for (int i = 0; i < res1.Length; i++)
        //    //{
        //    //    g.DrawLine(new Pen(Color.Black, 2),
        //    //        new Point(res1[i].x * bitmap.Width / (endk - begink), h/2 + res1[i].y / 4),
        //    //        new Point(res1[i].x * bitmap.Width / (endk - begink), h/2 + (res1[i].y - 10) / 4));
        //    //}

        //    //draw waveform

        //    for (int i = begink; i < endk; i++)
        //    {
        //        Pen p = new Pen(Color.Green, 1);
        //        g.DrawLine(p,
        //            new Point((int)(((double)i - begink) * w / (endk - begink)), h / 2),
        //            new Point((int)(((double)i - begink) * w / (endk - begink)), h / 2 + (int)((double)tone.data[i] / 60 / 4)));

        //    }

        //    ////draw cut temp
        //    //double[] cutdata = SoundAnalysis.getCutList(sample, sample.Length * 1 / 2);
        //    //double max = cutdata[0];
        //    //foreach (var v in cutdata) if (v > max) max = v;
        //    //for (int i = 0; i < cutdata.Length; i++)
        //    //{
        //    //    g.DrawLine(new Pen(Color.OrangeRed, 1),
        //    //        new Point((int)((i + 2.0) * 3), h / 2),
        //    //        new Point((int)((i + 2.0) * 3), h / 2 + (int)((double)cutdata[i] / max * -100)));
        //    //}

        //    ////draw wav energy
        //    //double[] edata = SoundAnalysis.Energy(sample);
        //    //double emax = edata[0];
        //    //foreach (var v in edata) if (v > emax) emax = v;
        //    //for (int i = 0; i < edata.Length; i++)
        //    //{
        //    //    g.DrawLine(new Pen(Color.PaleVioletRed, 1),
        //    //        new Point((int)((i + 2.0) * 3), h / 2),
        //    //        new Point((int)((i + 2.0) * 3), h / 2 + (int)((double)edata[i] / emax * -100)));
        //    //}



        //    ////draw envelope
        //    //double[] sampled = new double[sample.Length];
        //    //for (int i = 0; i < sample.Length; i++) sampled[i] = sample[i];
        //    //var envelope = WAVAnalyzer.getEnvelope3(sampled);
        //    //int k = envelope.GetLength(0);
        //    //for (int i = 1; i < k; i++)
        //    //{
        //    //    g.DrawLine(new Pen(Color.Red, 5),
        //    //        new Point((int)((double)(i - 1) * bitmap.Width / k), 290 + (int)((double)envelope[i - 1, 1] / 60)),
        //    //        new Point((int)((double)i * bitmap.Width / k), 290 + (int)((double)envelope[i, 1] / 60)));
        //    //}

        //    return bitmap;
        //}


        public static Image getFFTImage(int[] data)
        {
            int w = 2000;
            int h = 1000;

            Bitmap bitmap = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            //draw fft
            int n = 1024;
            double[] datas = new double[n];
            double[] datas2 = new double[n];

            if (data.Length <= n) return null;

            for (int i = 0; i < n; i++)
            {
                datas2[i] = 0;
                datas[i] = data[i];
            }
            int num = TWFFT.FFT(datas, datas2);
            int time = 5;
            for (int i = 1; i < n / time; i++)
            {
                double mod = Math.Sqrt(datas[i] * datas[i] + datas2[i] * datas2[i]) / w;
                g.DrawLine(new Pen(Color.Black, 3),
                    new Point((int)((double)i / n * time * bitmap.Width), 800),
                    new Point((int)((double)i / n * time * bitmap.Width), 800 - (int)mod));
            }

            //draw envelope
            double[] envori = new double[n / time];
            for (int i = 0; i < n / time; i++)
            {
                envori[i] = Math.Sqrt(datas[i] * datas[i] + datas2[i] * datas2[i]);
            }
            //Array.Sort(envori);

            //int[,] env = SoundAnalysis.getEnvelope3(envori);
            //int k = env.GetLength(0);
            //for (int i = 1; i < k; i++)
            //{
            //    g.DrawLine(new Pen(Color.Red, 5),
            //        new Point((int)((double)(i - 1) / k * bitmap.Width), 800 - (int)((double)env[i - 1, 1] / w)),
            //        new Point((int)((double)i / k * bitmap.Width), 800 - (int)((double)env[i, 1] / w)));
            //}

            return bitmap;
        }






        //public string[] 

        private void button1_Click(object sender, EventArgs e)
        {
            filepath = textBox1.Text + @"\";
            sounds = NNAnalysis.getParamsFromNN(filepath);

            //tl = SoundAnalysis.analysisAll(filepath);
            //WAVAnalyzer.writeWAV(readVoiceD());

            //tl = SoundAnalysis.analysisAll(filepath);
            sa = new SoundAnalysis();
            sa.init(filepath);
            listBox1.Items.Clear();
            for (int i = 0; i < sounds.Length; i++)
            {
                listBox1.Items.Add(sounds[i].name);
            }
        }




        public double[] getZipDatas(int toneNum)
        {
            double[] datas;
            switch (toneNum)
            {
                case 1:
                    datas = new double[] { 1};
                    break;
                case 2:
                    datas = new double[] { 0.9,1.0 };
                    break;
                case 3:
                    datas = new double[] { 0.85,0.8,0.82};
                    break;
                case 4:
                    datas = new double[] { 1,1,0.95,0.8 };
                    break;
                case 5:
                default:
                    datas = new double[] { .60, .60, .60, .60, .60 };
                    break;
            }
            return datas;
        }


        public void showSoundInfo(int n)
        {
            WAVControl.writeWAV(NNAnalysis.readVoiceD(filepath, sounds[n].begin, sounds[n].length), outputOri);

            System.Media.SoundPlayer player = new System.Media.SoundPlayer(outputOri);
            player.Play();

            //WAVControl.writeWAV(SoundAnalysis.getSampleByte(SoundAnalysis.getSample(outputOri)), output);

            //System.Media.SoundPlayer player2 = new System.Media.SoundPlayer(output);
            //player2.PlaySync();
            //double tbegin = double.Parse(textBox5.Text);
            //double tend = double.Parse(textBox6.Text);
            var data = sa.tl.getTone(sounds[n].name);
            pictureBox1.Image = getImage(data);
            pictureBox3.Image = getFFTImage(data);
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                showSoundInfo(listBox1.SelectedIndex);
            }
        }

        public Dictionary<string, string> getBaiduToken()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://openapi.baidu.com/oauth/2.0/token");
            request.CookieContainer = new CookieContainer();
            request.Accept = "Accept:text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Headers["Accept-Language"] = "zh-CN,zh;q=0.";
            request.Headers["Accept-Charset"] = "GBK,utf-8;q=0.7,*;q=0.3";
            request.UserAgent = "User-Agent:Mozilla/5.0 (Windows NT 5.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
            request.KeepAlive = false;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            Encoding encoding = Encoding.UTF8;
            byte[] postData = encoding.GetBytes(string.Format("grant_type={0}&client_id={1}&client_secret={2}",
                "client_credentials",
                "q6uhWBgzBYDVcHLEuleYBj9r",
                "DZKH7t1c3NP9HHj9mWNoGqmutXxyEu1o"));
            request.ContentLength = postData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(postData, 0, postData.Length);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, encoding);
            string retString = streamReader.ReadToEnd();

            string[] res = retString.Replace("\n", "").Split(',');
            dict["access_token"] = res[0].Split(':')[1].Split('\"')[1];
            dict["session_key"] = res[1].Split(':')[1].Split('\"')[1];
            dict["scope"] = res[2].Split(':')[1].Split('\"')[1];
            dict["refresh_token"] = res[3].Split(':')[1].Split('\"')[1];
            dict["session_secret"] = res[4].Split(':')[1].Split('\"')[1];
            dict["expires_in"] = res[5].Split(':')[1].Replace("}\"", "");

            streamReader.Close();
            responseStream.Close();

            return dict;
        }


        private clsMCI cm;
        public void getBaiduSound(string str,string token)
        {
            if (cm == null)
            {
                cm = new clsMCI();
            }
            else
            {
                cm.StopT();
                cm = new clsMCI();
            }
            str = System.Web.HttpUtility.UrlEncode(str);
            string url = string.Format("http://tsn.baidu.com/text2audio?tex={0}&lan={1}&tok={2}&ctp={3}&cuid={4}",
                str,
                "zh",
                token,
                1,
                "001");
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();
                        
            string filename="baidu.mp3";
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
            }
            responseStream.Close();
            
            cm.FileName = filename;
            cm.play();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var res=getBaiduToken();
            string text = textBox9.Text;
            getBaiduSound(text, res["access_token"]);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //获取声调名称
            int toneindex = listBox1.SelectedIndex;
            if (toneindex < 0) return;
            string tonename = sounds[toneindex].name;
            double pitch = sounds[toneindex].pitch;
            pitch = SemitoneTransfer.getTN(pitch);

            //获取声调包络
            int pit = int.Parse(numericUpDown1.Value.ToString());
            var pitdata = getZipDatas(pit);

            //获取长度比例
            int len = int.Parse(numericUpDown2.Value.ToString());

            //初始化音调信息
            //NNTone tmp = sounds[toneindex];
            //int begin = tmp.begin;
            //int length = tmp.length;
            //int head = tmp.inibegin;
            //int foot = tmp.tailbegin;
            //double rbegin = double.Parse(textBox7.Text);
            //double rend = double.Parse(textBox8.Text);

            //合成

            SynTone synt = new SynTone(tonename, pitdata, len, new double[] { 1 }, 0, pitch);
            int[] res = sa.getSoundTone(synt);
            this.pictureBox2.Image = getImage(res);
            this.pictureBox4.Image = getFFTImage(res);
            sw.Stop();
            sw = new Stopwatch();
            sw.Start();
            WAVControl.writeWAV(res, outputTone);

            //播放合成结果wav
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(outputTone);
            player.PlaySync();
            sw.Stop();
            //double tbegin = double.Parse(textBox5.Text);
            //double tend = double.Parse(textBox6.Text);
            //pictureBox2.Image = getImage(outputTone, tbegin, tend);
            //pictureBox4.Image = getFFTImage(SoundAnalysis.getSample(outputTone));
        }

    }
}
