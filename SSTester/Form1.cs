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
using mySpeechSynthesizer.NNLink;

namespace nntest
{
    public partial class Form1 : Form
    {
        Form2 f2;
        public Form1()
        {
            InitializeComponent();
            f2 = new Form2();
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

        public static double[] getFFTResult(int[] data,int dx,int len=1024)
        {
            double[] result = new double[len];
            double[] resulti = new double[len];
            for(int i = 0; i < len; i++)
            {
                int index = i + dx;
                if (index >= 0 && index < data.Length) result[i] = data[index];
                else result[i] = 0;
                resulti[i] = 0;
            }
            result = addHanning(result);
            TWFFT.FFT(result, resulti);
            double[] result_len = new double[len];
            for (int i = 0; i < len; i++) result_len[i] = Math.Sqrt(result[i] * result[i] + resulti[i] * resulti[i]);
            return result;
        }

        public static double[] addHanning(double[] data)
        {
            int N = data.Length;
            double[] res = new double[N];

            double a = 0.46;
            for (int i = 0; i < N; i++)
            {
                res[i] = (int)(data[i] * ((1 - a) - a * Math.Cos(2 * Math.PI * i / (N - 1))));
            }
            return res;
        }

        public static double getMax(double[] data)
        {
            double max = 0;
            foreach(var d in data)
            {
                if (Math.Abs(d) > max)
                {
                    max = Math.Abs(d);
                }
            }
            return max;
        }

        public static Color getRGB(double x)
        {
            int r = 0;
            int g = 0;
            int b = 0;
            double q = 0.25;
            if (x <= q * 1)
            {
                r = 255;
                g = (int)(255 * (x / q));
                b = 0;
            }
            else if (x <= q * 2)
            {
                r = (int)(255 - 255*(x - q) / q);
                g = 255;
                b = 0;
            }
            else if (x <= q * 3)
            {
                r = 0;
                g = 255;
                b = (int)((x - 2 * q) / q);
            }
            else if (x <= q * 4)
            {
                r = 0;
                g = (int)(255 - 255*(x - 3 * q) / q);
                b = 255;
            }
            return Color.FromArgb(r, g, b);
        }

        public static Image getPPImage(int[] data)
        {
            int h = 300;
            int n = 2048;
            int dd = 200;
            int showw = 2;
            double hzbegin = 0.0;
            double hzend = 0.2;
            int w = data.Length / dd * showw;
            //int rh = (int)(h * (hzend - hzbegin));
            Bitmap bitmap = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            for (int dx = 0; dx < data.Length; dx += dd)
            {
                double[] res=getFFTResult(data, dx, n);
                //double max = getMax(res);
                double max = 500000;
                double im = Math.Min(res.Length, hzend * n);
                for (int i =(int)(hzbegin*n); i < im; i++)
                {
                    double r = Math.Min(1,Math.Pow(Math.Abs(res[i]) / max, 0.5));
                    int x = dx / dd * showw;
                    int y1 = (int)(h * (1 - i / im));
                    int y2 = (int)(h * (1 - (i + 1) / im));
                    g.DrawLine(
                        new Pen(getRGB(1-r), showw),
                        new Point(x, y1),
                        new Point(x, y2));
                }
            }
            
            return bitmap;
        }








        /// <summary>
        /// 用倒谱获得频谱包络
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static double[] GetEnvelopeFD(double[] data)
        {
            int n = data.Length;
            double[] data1 = new double[n];
            double[] data1i = new double[n];
            for (int i = 0; i < n; i++) data1[i] = Math.Log(data[i]);
            for (int i = 0; i < n; i++) data1i[i] = 0;
            TWFFT.FFT(data1, data1i);
            double therehold = 100;
            double[] data2 = new double[n];
            double[] data2i = new double[n];
            // 低通滤波
            for (int i = 0; i < n; i++)
                if (i < therehold || i > n - therehold)
                {
                    data2[i] = data1[i];
                    data2i[i] = data1i[i];
                }
                else
                {
                    data2[i] = 0;
                    data2i[i] = 0;
                }
            //for (int i = 0; i < n; i++) data2i[i] = 0;
            TWFFT.IFFT(data2, data2i);
            var res = FToneAnalysis.MiddleFilter(data2, 3);
            for (int i = 0; i < n; i++) res[i] = Math.Pow(Math.E, res[i]);
            return res;
        }

        

        public static Image getDFFTImage(int[] data,int beginx,int len,double pitch=1)
        {
            int w = 2000;
            int h = 1000;

            Bitmap bitmap = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //draw fft
            int n = 4096;
            double[] datas = new double[n];
            double[] datasi = new double[n];
            double[] datamod = new double[n];
            //if (data.Length <= n) return null;

            for (int i = 0; i < n; i++)
            {
                datasi[i] = 0;
                if (i > data.Length - 1) datas[i] = 0;
                else datas[i] = data[i];
            }
            TWFFT.FFT(datas, datasi);
            for (int i = 0; i < n; i++) datamod[i] = Math.Sqrt(datas[i] * datas[i] + datasi[i] * datasi[i]);
            var ddataenv = GetEnvelopeFD(datamod);
            int time = 2;
            for (int i = 1; i < n / time; i++)
            {
                g.DrawLine(new Pen(Color.Black, 1),
                    new Point((int)((double)(i - 1) / n * time * bitmap.Width), (int)(h - datamod[i - 1] / w)),
                    new Point((int)((double)i / n * time * bitmap.Width), (int)(h - datamod[i] / w)));
            }

            double[] ddatabase = new double[n];
            double[] ddataibase = new double[n];
            for (int i = 0; i < n; i++) ddatabase[i] = datas[i]/ ddataenv[i];
            for (int i = 0; i < n; i++) ddataibase[i] = datasi[i]/ ddataenv[i];
            double[] ddatabaser = new double[n];
            double[] ddataibaser = new double[n];
            // 对称共轭，所以分两半分别压缩
            ddatabaser[0] = datas[0];
            ddataibaser[0] = datasi[0];
            pitch = 2.4;
            for (int i = 1; i <= n / 2; i++)
            {
                ddatabaser[i] = getResampleValue(ddatabase, (double)i / pitch);
                ddatabaser[n - i] = ddatabaser[i];
                ddataibaser[i] = getResampleValue(ddataibase, (double)i / pitch);
                ddataibaser[n - i] = -ddataibaser[i];
            }
            for (int i = 0; i < n; i++) ddatabaser[i] = ddatabaser[i] * ddataenv[i];
            for (int i = 0; i < n; i++) ddataibaser[i] = ddataibaser[i] * ddataenv[i];
            for (int i = 0; i < n; i++) datamod[i] = Math.Sqrt(ddatabaser[i] * ddatabaser[i] + ddataibaser[i] * ddataibaser[i]);
            for (int i = 1; i < n / time; i++)
            {
                g.DrawLine(new Pen(Color.Red, 1),
                    new Point((int)((double)(i - 1) / n * time * bitmap.Width), (int)(h - datamod[i - 1] / w)),
                    new Point((int)((double)i / n * time * bitmap.Width), (int)(h - datamod[i] / w)));
            }


            return bitmap;
        }
        private static double getResampleValue(double[] data, double target)
        {
            int p1 = (int)target;
            int p2 = p1 + 1;
            if (target > data.Length / 2 || target < 1) return 0;// data[data.Length - 1];
            else return data[p1] + (target - p1) * (data[p2] - data[p1]);
        }

        public static Image getFFTImage(int[] data)
        {
            int w = 2000;
            int h = 1000;

            Bitmap bitmap = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            //draw fft
            int n = 4096;
            double[] datas = new double[n];
            double[] datas2 = new double[n];
            double[] datamod = new double[n];
            //if (data.Length <= n) return null;

            for (int i = 0; i < n; i++)
            {
                datas2[i] = 0;
                if (i > data.Length - 1) datas[i] = 0;
                else datas[i] = data[i];
            }
             TWFFT.FFT(datas, datas2);
            for (int i = 0; i < n; i++) datamod[i] = Math.Sqrt(datas[i] * datas[i] + datas2[i] * datas2[i]);
            int time = 2;
            for (int i = 1; i < n / time; i++)
            {
                g.DrawLine(new Pen(Color.Black, 1),
                    new Point((int)((double)(i - 1) / n * time * bitmap.Width), (int)(h - datamod[i-1] / w)),
                    new Point((int)((double)i / n * time * bitmap.Width), (int)(h - datamod[i] / w)));
            }

            double[] datad = new double[n];
            double[] datad2 = new double[n];
            for (int i = 0; i < n; i++) datad2[i] = 0;
            for (int i = 0; i < n; i++) datad[i] = Math.Log(datamod[i]);
            for (int i = 1; i < n / time; i++)
            {
                g.DrawLine(new Pen(Color.Red, 1),
                    new Point((int)((double)(i - 1) / n * time * bitmap.Width), (int)(h - datad[i - 1] * 50)),
                    new Point((int)((double)i / n * time * bitmap.Width), (int)(h - datad[i] * 50)));
            }

            TWFFT.FFT(datad, datad2);
            for (int i = 0; i < n; i++) datamod[i] = Math.Log(Math.Sqrt(datad[i] * datad[i] + datad2[i] * datad2[i]));

            double therehold = 100;
            double[] rdata = new double[n];
            double[] rdata2 = new double[n];
            for (int i = 0; i < n; i++) if (i < therehold || i > n-therehold) rdata[i] = datad[i]; else rdata[i] = 0;
            for (int i = 0; i < n; i++) rdata2[i] = datad2[i];
            TWFFT.IFFT(rdata, rdata2);
            for (int i = 1; i < n / time; i++)
            {
                g.DrawLine(new Pen(Color.Blue, 1),
                    new Point((int)((double)(i - 1) / n * time * bitmap.Width), (int)(h - rdata[i - 1] * 50)),
                    new Point((int)((double)i / n * time * bitmap.Width), (int)(h - rdata[i] * 50)));
            }



            return bitmap;
        }






        //public string[] 

        private void button1_Click(object sender, EventArgs e)
        {
            filepath = textBox1.Text + @"\";
            sounds = NBankManager.getParams(filepath);

            //tl = SoundAnalysis.analysisAll(filepath);
            //WAVAnalyzer.writeWAV(readVoiceD());

            //tl = SoundAnalysis.analysisAll(filepath);
            sa = new SoundAnalysis();
            sa.init(filepath,SourceType.Niaoniao);
            listBox1.Items.Clear();
            for (int i = 0; i < sounds.Length; i++)
            {
                listBox1.Items.Add(sounds[i].name);
            }
        }




        public double[][] getZipDatas(int toneNum)
        {
            double[][] datas;
            switch (toneNum)
            {
                case 1:
                    datas = new double[][] { new double[] { 0.5, 1 } };
                    break;
                case 2:
                    datas = new double[][] { new double[] { 0, 0.9 }, new double[] { 1, 1.1 } };
                    break;
                case 3:
                    datas = new double[][] { new double[] { 0, 0.85 }, new double[] { 0.4, 0.8 }, new double[] { 1, 0.9 } };
                    break;
                case 4:
                    datas = new double[][] { new double[] { 0, 1 }, new double[] { 0.5, 0.9 }, new double[] { 1, 0.8 } };
                    break;
                case 5:
                default:
                    datas = new double[][] { new double[] { 0, 0.6 }, new double[] { 1, 0.6 } };
                    break;
            }
            return datas;
        }


        public void showSoundInfo(int n)
        {
            var data = sa.tl.getTone(sounds[n].name);
            WAVControl.writeWAV(data, outputOri);

            System.Media.SoundPlayer player = new System.Media.SoundPlayer(outputOri);
            player.Play();

            //WAVControl.writeWAV(SoundAnalysis.getSampleByte(SoundAnalysis.getSample(outputOri)), output);

            //System.Media.SoundPlayer player2 = new System.Media.SoundPlayer(output);
            //player2.PlaySync();
            //double tbegin = double.Parse(textBox5.Text);
            //double tend = double.Parse(textBox6.Text);
            
            pictureBox1.Image = getPPImage(data);
            pictureBox3.Image = getFFTImage(data);
            pictureBox4.Image = getDFFTImage(data, 0, 1000, 1);
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
            string text = textBox2.Text;
            getBaiduSound(text, res["access_token"]);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
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
            this.pictureBox2.Image = getPPImage(res);
            //this.pictureBox4.Image = getFFTImage(res);
            //sw.Stop();
            //sw = new Stopwatch();
            //sw.Start();
            WAVControl.writeWAV(res, outputTone);

            //播放合成结果wav
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(outputTone);
            player.PlaySync();
            
            //sw.Stop();
            //double tbegin = double.Parse(textBox5.Text);
            //double tend = double.Parse(textBox6.Text);
            //pictureBox2.Image = getImage(outputTone, tbegin, tend);
            //pictureBox4.Image = getFFTImage(SoundAnalysis.getSample(outputTone));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            f2.Show();
        }



        private void button5_Click(object sender, EventArgs e)
        {
            filepath = textBox1.Text + @"\";
            

            //tl = SoundAnalysis.analysisAll(filepath);
            //WAVAnalyzer.writeWAV(readVoiceD());

            //tl = SoundAnalysis.analysisAll(filepath);
            sa = new SoundAnalysis();
            sa.init(filepath, SourceType.UTAU);

            sounds = sa.getNNTonesFromToneList();

            listBox1.Items.Clear();
            for (int i = 0; i < sounds.Length; i++)
            {
                listBox1.Items.Add(sounds[i].name);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string text = textBox2.Text;
            PinYinConverter pyc = new PinYinConverter();
            var res=pyc.getPinYinList(text);

            StringBuilder resstr = new StringBuilder();
            foreach(var v in res)
            {
                foreach(var vv in v)
                {
                    resstr.Append(vv + " ");
                }
                resstr.Append("\r\n");
            }

            textBox3.Text = resstr.ToString();
            //var res2 = res;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string text = textBox2.Text;
            HanSynthesis hs = new HanSynthesis(@"D:\Program Files\袅袅虚拟歌手v2.2%28附余袅袅音源%29\袅袅虚拟歌手v2.2\src\枸杞子音源库_v1.0");
            hs.soundSpeed = 190;
            hs.readSentence(text);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            string text = textBox2.Text;
            PinYinConverter pyc = new PinYinConverter();
            var pyres = pyc.getPinYinList(text);
            
            StringBuilder resstr = new StringBuilder();
            foreach (var v in pyres)
            {
                foreach (var vv in v)
                {
                    var ipa=mySpeechSynthesizer.Analysis.IPAManager.Pinyin2IPA(vv.Substring(0,vv.Length-1));
                    resstr.Append(vv + " ");
                }
                resstr.Append("\r\n");
            }

            textBox3.Text = resstr.ToString();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                string path = textBox1.Text;
                int[] data = WAVControl.getSample(path);
                pictureBox1.Image = getPPImage(data);
                pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox2.Image = getFFTImage(data);

            }
            catch
            {

            }

        }
    }
}
