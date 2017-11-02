using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// WAV文件控制
    /// </summary>
    public class WAVControl
    {
        //每个采样对应的字节数
        const int byteSample = 2;
        const int dataPosition = 40;

        /// <summary>
        /// byte[] => int
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int getHexToInt(byte[] x)
        {
            string retValue = "";
            for (int i = x.Length - 1; i >= 0; i--)
            {
                retValue += x[i].ToString("X").PadLeft(2, '0');
            }
            return Convert.ToInt32(retValue, 16);
        }

        /// <summary>
        /// byte[] => 16x byte[]
        /// </summary>
        /// <param name="x"></param>
        public static void getHex(byte[] x)
        {
            byte tmp;
            for (int i = 0; i < x.Length; i++)
            {
                tmp = Convert.ToByte(x[i].ToString("X").PadLeft(2, '0'), 16);
                x[i] = tmp;
            }
        }

        public static int[] getSample(byte[] x,int begin,int len)
        {
            int[] retValue = new int[len / byteSample];

            for (int i = 0; i < retValue.Length; i++)
            {
                string tmp = "";
                for (int j = (i + 1) * byteSample - 1; j >= i * byteSample; j--)
                {
                    tmp += x[begin+j].ToString("X").PadLeft(2, '0');
                }
                retValue[i] = (int)Convert.ToInt16(tmp, 16);
            }
            return retValue;
        }

        /// <summary>
        /// byte => int
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int[] getSample(byte[] x)
        {
            int[] retValue = new int[x.Length / byteSample];

            for (int i = 0; i < retValue.Length; i++)
            {
                string tmp = "";
                for (int j = (i + 1) * byteSample - 1; j >= i * byteSample; j--)
                {
                    tmp += x[j].ToString("X").PadLeft(2, '0');
                }
                retValue[i] = (int)Convert.ToInt16(tmp, 16);
            }
            return retValue;
        }

        /// <summary>
        /// int => byte
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static byte[] getSampleByte(int[] x)
        {
            byte[] retValue = new byte[x.Length * byteSample];

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] > Int16.MaxValue) x[i] = Int16.MaxValue;
                if (x[i] < Int16.MinValue) x[i] = Int16.MinValue;
                byte[] tmp = BitConverter.GetBytes(Convert.ToInt16(x[i]));
                for (int j = byteSample - 1; j >= 0; j--)
                {
                    //if (j >= tmp.Length) retValue[i * byteSample + j] = 0x00;
                    //else 
                    retValue[i * byteSample + j] = tmp[j];
                }
            }
            return retValue;
        }

        /// <summary>
        /// file => int[]
        /// </summary>
        /// <param name="wavfile"></param>
        /// <returns></returns>
        public static int[] getSample(string wavfile)
        {
            byte[] length = new byte[4];
            int[] sample;
            using (FileStream fs = new FileStream(wavfile, FileMode.Open, FileAccess.Read))
            {
                fs.Position = dataPosition;
                fs.Read(length, 0, 4);
                byte[] content = new byte[getHexToInt(length)];
                sample = new int[content.Length / byteSample];
                fs.Read(content, 0, content.Length);
                getHex(content);
                sample = getSample(content);
            }
            return sample;
        }


        /// <summary>
        /// int[] => file
        /// </summary>
        /// <param name="newdata"></param>
        /// <param name="savepath"></param>
        public static void writeWAV(int[] newdata, string savepath = null)
        {
            byte[] data = WAVControl.getSampleByte(newdata);
            writeWAV(data, savepath);

            //int length = data.Length;
            //if (string.IsNullOrWhiteSpace(savepath)) savepath = @"tmp_tone.wav";
            //using (FileStream fs = new FileStream(savepath, FileMode.Create))
            //{
            //    using (BinaryWriter sw = new BinaryWriter(fs, Encoding.GetEncoding(1252)))
            //    {
            //        sw.Write("RIFF".ToCharArray());
            //        sw.Write((uint)(length + 44));
            //        sw.Write("WAVEfmt ".ToCharArray());
            //        sw.Write((uint)16);
            //        //编码格式
            //        sw.Write((UInt16)1);
            //        //声道
            //        sw.Write((UInt16)1);
            //        //采样率
            //        sw.Write((uint)44100);
            //        //采样频率
            //        sw.Write((uint)88200);
            //        //每个采样对应的字节数 
            //        sw.Write((UInt16)byteSample);
            //        //采样大小
            //        sw.Write((UInt16)16);
            //        sw.Write("data".ToCharArray());
            //        sw.Write((uint)length);
            //        sw.Write(data);
            //    }
            //}
        }


        /// <summary>
        /// byte[] => file
        /// </summary>
        /// <param name="data"></param>
        /// <param name="savepath"></param>
        public static void writeWAV(byte[] data, string savepath = null)
        {
            int length = data.Length;
            if (string.IsNullOrWhiteSpace(savepath)) savepath = @"tmp_tone.wav";
            using (FileStream fs = new FileStream(savepath, FileMode.Create))
            {
                using (BinaryWriter sw = new BinaryWriter(fs, Encoding.GetEncoding(1252)))
                {
                    sw.Write("RIFF".ToCharArray());
                    sw.Write((uint)(length + 44));
                    sw.Write("WAVEfmt ".ToCharArray());
                    sw.Write((uint)16);
                    //编码格式
                    sw.Write((UInt16)1);
                    //声道
                    sw.Write((UInt16)1);
                    //采样率
                    sw.Write((uint)44100);
                    //采样频率
                    sw.Write((uint)88200);
                    //每个采样对应的字节数 
                    sw.Write((UInt16)byteSample);
                    //采样大小
                    sw.Write((UInt16)16);
                    sw.Write("data".ToCharArray());
                    sw.Write((uint)length);
                    sw.Write(data);
                }
            }

        }
    }
}
