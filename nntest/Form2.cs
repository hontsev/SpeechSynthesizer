using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nntest
{
    public partial class Form2 : Form
    {
        public delegate void printDelegate(string str);
        private Process p;

        private void print(string str)
        {
            if (textBox2.InvokeRequired)
            {
                printDelegate pe = new printDelegate(print);
                Invoke(pe, (object)str);
            }
            else
            {
                textBox2.AppendText(str + "\r\n");
            }
        }

        public Form2()
        {
            InitializeComponent();



        }

        private void getVoice(string filepath,string outputpath = @"D:\video\output\test.aac")
        {
            executeCommand(string.Format("-i {0} -y -acodec copy -vn {1}",filepath,outputpath));
            executeCommand(string.Format("-i {0} {1}", outputpath, Path.GetDirectoryName(outputpath) + "/" + Path.GetFileNameWithoutExtension(outputpath) + ".wav"));
        }

        private void getPic(string filepath,double time,string outputpath = @"D:\video\output\test.jpg")
        {
            executeCommand(string.Format("-i {0} -y -f image2 -t {1} {2}", filepath, time, outputpath));
        }

        private void executeCommand(string arguments)
        {
            string path = @"D:\Downloads\ffmpeg-20171130-83ecdc9-win64-static\ffmpeg-20171130-83ecdc9-win64-static\bin\";
            try
            {
                p.Close();
                p.Dispose();
            }
            catch { }
            p = new Process();
            p.StartInfo.FileName = path + "ffmpeg.exe";
            //string arguments = string.Format("-i {0} -y -f image2 -t 0.001 -s 352x240 {1}", filepath, outputPath+"test.jpg");
            //p.StartInfo.Arguments = "-i " + srcFileName + " -y  -vcodec h264 -b 500000 " + destFileName;    //执行参数
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);
            p.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
            p.StartInfo.UseShellExecute = false;
            p.Start();
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.BeginErrorReadLine();

            //p.WaitForExit();//阻塞等待进程结束

            //p.Close();//关闭进程

            //p.Dispose();//释放资源
        }


        

        private void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            print(e.Data);
            //throw new NotImplementedException();
        }

        private void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            print(e.Data);
            //throw new NotImplementedException();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filepath = "\"" + textBox1.Text + "\"";
            getVoice(filepath);
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                p.Close();//关闭进程

                p.Dispose();//释放资源
            }
            catch
            {

            }
            //p.WaitForExit();//阻塞等待进程结束


        }
    }
}
