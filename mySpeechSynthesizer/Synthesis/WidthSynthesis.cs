using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 时域合成
    /// </summary>
    class WidthSynthesis
    {
        /// <summary>
        /// 获取与音频数据一一对应的频率调整参数
        /// </summary>
        /// <param name="len"></param>
        /// <param name="zipdata"></param>
        /// <returns></returns>
        public static double[] getZipPoint(int len, double[] zipdata)
        {
            double[] res = new double[len];

            if (zipdata.Length == 0)
            {
                for (int i = 0; i < len; i++) res[i] = 1;
            }
            else if (zipdata.Length == 1)
            {
                for (int i = 0; i < len; i++) res[i] = 1.0/zipdata[0];
            }
            else if (zipdata.Length == 2)
            {
                for (int i = 0; i < len; i++) res[i] = 1.0/(zipdata[0] + ((zipdata[1] - zipdata[0]) * i / len));
            }
            else
            {
                mPoint[] zippoints = new mPoint[zipdata.Length];
                for (int i = 0; i < zipdata.Length; i++) zippoints[i] = new mPoint();
                for (int i = 0; i < zipdata.Length; i++)
                {
                    zippoints[i].x = len * i / (zipdata.Length - 1);
                    zippoints[i].y = (int)(zipdata[i] * 10000);
                }
                mPoint[] zipres = BezierControl.createCurve(zippoints);
                for (int i = 0; i < len; i++) res[i] =1.0/((double)zipres[i].y / 10000);
            }

            return res;
        }

        /// <summary>
        /// 根据pitch数值，确定新的pit参数序列。
        /// </summary>
        /// <param name="pit"></param>
        /// <param name="oldpitch"></param>
        /// <param name="newpitch"></param>
        /// <returns></returns>
        private static double[] getNewPit(double[] pit, double oldpitch, double newpitch)
        {
            double changepos = newpitch / oldpitch-1;
            double[] res = new double[pit.Length];
            for(int i = 0; i < res.Length; i++)
            {
                res[i] = changepos + pit[i];
            }
            return res;
        }

        public static int[] getWAVdata(ToneList tl,string tonename, double[] pit, int length,double pitch)
        {
            pit = getNewPit(pit, SemitoneTransfer.getTN( tl.tones[tonename].pitch), pitch);
            int[] oridata = tl.getTone(tonename);
            oridata= FToneAnalysis.getSoundPart(oridata);
            int newlen = (int)((double)length / 1000 * NNAnalysis.samplingRate);
            int[] newdata = getSound(oridata, pit, newlen);

            //if (newlen <= head)
            //{
            //    Array.Copy(ddata, 0, newdata, 0, newlen);
            //}
            //else if (newlen <= head + ddata.Length - foot)
            //{
            //    Array.Copy(ddata, 0, newdata, 0, head);
            //    Array.Copy(ddata, ddata.Length - (newlen - head), newdata, head, newlen - head);
            //}
            //else
            //{
            //    Array.Copy(ddata, 0, newdata, 0, head);

            //    int midoribegin = (int)(head + (foot - head) * begincut);
            //    int midoriend = (int)(foot - (foot - head) * endcut);
            //    int[] midori = new int[midoriend - midoribegin];
            //    Array.Copy(ddata, head, midori, 0, midori.Length);
            //    int midlen=newlen - (ddata.Length - foot ) - head;
            //    int[] mid = getZipSound(midori, midlen, pit, overlapcut);
            //    ////整形
            //    //int[] midori2 = new int[foot - head];
            //    //Array.Copy(ddata, head, midori2, 0, midori2.Length);
            //    //int[,] envori = getEnvelope(midori2);
            //    //for (int s = 0; s < 1; s++)
            //    //{
            //    //    int[,] env = getEnvelope(mid);
            //    //    for (int i = 1; i < 101; i++)
            //    //    {
            //    //        if (env[i, 1] == 0) env[i, 1] = 1;
            //    //        int dy = (envori[i, 1] - env[i, 1]) / 3;
            //    //        for (int j = env[i - 1, 0]; j < env[i, 0]; j++)
            //    //        {
            //    //            mid[j] = (int)(mid[j] + (mid[j] > 0 ? dy : -dy));
            //    //        }
            //    //        //mid[i] = mid[i] + envo - env[i];
            //    //    }
            //    //}

            //    Array.Copy(mid, 0, newdata, head, midlen);

            //    Array.Copy(ddata, foot, newdata, newlen - (ddata.Length - foot), ddata.Length - foot);
            //}

            ////全局整形

            //int dx = 1024;

            //int nowx1 = 0;
            //int nowx2 = 0;
            //while (nowx1 < ddata.Length)
            //{
            //    float[] data1 = new float[dx];
            //    float[] data2 = new float[dx];
            //    float[] dataimag1 = new float[dx];
            //    for (int i = 0; i < dx; i++) dataimag1[i] = 0;
            //    float[] dataimag2 = new float[dx];
            //    for (int i = 0; i < dx; i++) dataimag2[i] = 0;
            //    for (int i = 0; i < dx; i++)
            //    {

            //        data1[i] = nowx1 + i < ddata.Length ? ddata[nowx1 + i] : 0;
            //        data2[i] = nowx2 + i < newdata.Length ? (float)newdata[nowx2 + i] : 0;
            //    }
            //    TWFFT.FFT(data1, dataimag1);
            //    TWFFT.FFT(data2, dataimag2);
            //    int[,] envori = getEnvelope(data1);
            //    int[,] env = getEnvelope(data2);
            //    for (int i = 1; i < 101; i++)
            //    {
            //        if (env[i, 1] == 0) env[i, 1] = 1;
            //        int dy = envori[i, 1] - env[i, 1];
            //        for (int j = env[i - 1, 0]; j < env[i, 0]; j++)
            //        {
            //            data2[j] = (float)(data2[j] + dy);
            //        }
            //    }
            //    TWFFT.IFFT(data2, dataimag2);
            //    for (int i = 0; i < dx; i++)
            //        if (nowx2 + i < newdata.Length)
            //            newdata[nowx2 + i] = (int)data2[i];
            //    nowx2 += dx;
            //    nowx1 += (int)((double)dx * ddata.Length / newdata.Length);
            //}
            return newdata;


        }



        /// <summary>
        /// 最接近它的右侧帧
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="nowloc"></param>
        /// <returns></returns>
        private static int getClosestFrame(int[] frames, int nowloc)
        {
            if (nowloc > frames[frames.Length - 1])
                return frames.Length - 1;
            else if (nowloc > 0 && nowloc < frames[0])
                return 1;
            else
            {
                for (int i = 1; i < frames.Length; i++)
                {
                    if (nowloc >= frames[i - 1] && nowloc <= frames[i]) return i;
                }
            }
            return 0;
        }

        private static FTone getClosestFTone(FTone[] ftones,int loc)
        {
            if (ftones.Length <= 0) return null;
            foreach(var ftone in ftones)
            {
                if (loc >= ftone.begin && loc <= ftone.begin + ftone.length) return ftone;
            }
            return ftones[0];
        }

        /// <summary>
        /// 用逐点重采样来变调
        /// </summary>
        /// <param name="oridata"></param>
        /// <param name="zip"></param>
        /// <returns></returns>
        private static int[] getFChangeSound1(int[] oridata, double[] zip)
        {
            double[] mzip = getZipPoint(oridata.Length, zip);
            List<int> res = new List<int>();
            double nowpoint = 0;
            for (int i = 0; i < oridata.Length; i++)
            {
                //int lastindex = (int)nowpoint;
                //nowpoint += zip[i];
                int lastpoint = (int)nowpoint;
                double num =  1 / Math.Max(mzip[i], 0.1);
                nowpoint += num;

                for(int j = lastpoint; j < (int)nowpoint; j++)
                {
                    res.Add(oridata[i]);
                }
            }
            
            return res.ToArray();
        }

        /// <summary>
        /// 获得加汉宁窗后的结果
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static int[] hanning(int[] n)
        {
            int N = n.Length;
            int[] res = new int[N];

            double a = 0.46;
            for(int i = 0; i < N; i++)
            {
                res[i] = (int)(n[i] * ((1 - a) - a * Math.Cos(2 * Math.PI * i / (N - 1))));
            }
           
            return res;
        }

        private static int[] triangle(int[] n)
        {
            int N = n.Length;
            int[] res = new int[N];

            for (int i = 0; i < N / 2; i++) res[i] = n[i] * (2 * i / N);
            for (int i = N / 2; i < N; i++) res[i] = n[i] * (2 - 2 * i / N);

            return res;
        }

        private static int[] getFChangeSound2(int[] oridata, double[] zip)
        {
            int framelen = NNAnalysis.samplingRate / 100;
            int windowlen = framelen * 2;
            int framenum = oridata.Length / framelen;
            double[] framezip = getZipPoint(framenum, zip);
            //前后补0
            int[] tmpdata = new int[oridata.Length + windowlen];
            Array.Copy(oridata, 0, tmpdata, windowlen / 2, oridata.Length);
            //对小段重采样
            List<int> res = new List<int>();
            for(int i = 0; i < framenum+1; i++)
            {
                List<int> newdata = new List<int>();
                double scale = framezip[Math.Min(i, framenum - 1)];
                double nowpoint = 0;
                for (int j = 0; j < windowlen; j++)
                {
                    int lastpoint = (int)nowpoint;
                    double num = 1 / Math.Max(scale, 0.1);
                    nowpoint += num;
                    for (int s = lastpoint; s < (int)nowpoint; s++)
                    {
                        newdata.Add(oridata[j]);
                    }
                }
                int[] output = triangle(newdata.ToArray());
                for(int j = 0; j < output.Length; j++)
                {
                    int nowpos = i * framelen + framelen - output.Length / 2 + j;
                    if (nowpos < 0) continue;
                    if (res.Count > nowpos) res[nowpos] += output[j];
                    else res.Add(output[j]);
                }
            }

            return res.ToArray();
        }

        /// <summary>
        /// 线性插值，重采样
        /// </summary>
        /// <param name="data"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static int getResampleValue(int[] data,double target)
        {
            int p1 = (int)target;
            int p2 = p1 + 1;
            if (p2 >= data.Length) return data[data.Length - 1];
            else if (p1 < 0) return 0;
            return (int)(data[p1] + (target - p1) * (data[p2] - data[p1]));
        }

        /// <summary>
        /// 离此位置最近的基音序号
        /// </summary>
        /// <param name="orilen"></param>
        /// <param name="reslen"></param>
        /// <param name="cut"></param>
        /// <param name="nowframeindex"></param>
        /// <returns></returns>
        private static int getNearestFrameIndex(int orilen, int reslen, int[] cut, int nowframeindex)
        {
            int index = 0;

            if (nowframeindex < 0) index = 1;
            //else if (nowframeindex >= cut.Length) index = cut.Length;
            else
            {
                int newpos = (int)((double)nowframeindex * orilen / reslen);
                if (newpos <= cut[1])
                {
                    //不能重复取起始帧，所以后续的只能取第1帧
                    index = 2;
                }
                else if (newpos >= cut[cut.Length-1])
                {
                    //最后一帧了，取cut.Length
                    index = cut.Length;
                }
                else
                {
                    for (int i = 1; i < cut.Length; i++)
                    {
                        if (cut[i - 1] < newpos && cut[i] >= newpos)
                        {
                            index = i;
                            break;
                        }
                    }
                    
                }

            }
            return index;
        }

        /// <summary>
        /// PSOLA法合成
        /// </summary>
        /// <param name="oridata"></param>
        /// <param name="zip"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static int[] getChangeSound2(int[] oridata, double[] zip,int length)
        {
            // 获取基音分割结果
            int[] cut = FToneAnalysis.cutIt(oridata);

            int framenum = cut.Length-1;

            // 获取逐帧频率压缩因子，用于接下来获取指定位置的压缩因子
            double[] framezip = getZipPoint(oridata.Length, zip);

            int[] res = new int[length];

            //将开头段先放进去再说
            //Array.Copy(oridata, 0, res, 0, Math.Min(length, cut[1]));

            int fbegin = cut[0];
            int fend = cut[1];
            double nowpos = 0;
            int respos = 0;
            //对小段重采样
            int i = 1;
            while (i < framenum)
            {
                fbegin = cut[i - 1];
                fend = cut[i];

                int thislen = fend - fbegin;
                fbegin -= thislen / 2;
                fend += thislen / 2;


                double nowzip = 1 / Math.Max(framezip[Math.Max(fbegin, 0)], 0.1);

                //reset nowpos
                nowpos = fbegin;
                List<int> tmp = new List<int>();
                while (nowpos < fend)
                {
                    int val = getResampleValue(oridata, nowpos);
                    tmp.Add(val);
                    nowpos += nowzip;
                }
                // 滤波
                int[] tmp2 = MiddleFilterCorrection( tmp.ToArray(), 0, tmp.Count, oridata, Math.Max(0,fbegin), fend);
                //tmp2 = FToneAnalysis.FrequencyFilter(tmp2, 0, 15000);
                // 用汉宁窗合并
                //var hanningtmp = tmp.ToArray();
                var hanningtmp = hanning(tmp2.ToArray());
                for (int j = 0; j < hanningtmp.Length; j++)
                {
                    if (respos + j - cut[i - 1] + fbegin >= res.Length)
                    {
                        break;
                    }
                    else if (respos+j - cut[i - 1] + fbegin >= 0)
                    {
                        res[respos + j - cut[i - 1] + fbegin] += hanningtmp[j];
                        //respos++;
                    }
                }
                // 做中值平滑
                int width = thislen/2;
                int phstart = Math.Max(0, respos - width);
                int phlen = Math.Min(res.Length - phstart, width * 2);
                int[] phdata = new int[phlen];
                Array.Copy(res, phstart, phdata, 0, phlen);
                phdata = FToneAnalysis.MiddleFilter(phdata, 3);
                //phdata = FToneAnalysis.MiddleFilter(phdata, 5);
                Array.Copy(phdata, 0, res, phstart, phlen);


                respos += hanningtmp.Length / 2;


                i = getNearestFrameIndex(oridata.Length, length, cut, respos);


            }
            return res;
        }



        /// <summary>
        /// 频域修正
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="begin1"></param>
        /// <param name="end1"></param>
        /// <param name="data2"></param>
        /// <param name="begin2"></param>
        /// <param name="end2"></param>
        /// <returns></returns>
        public static int[] MiddleFilterCorrection(int[] data1,int begin1,int end1,int[] data2,int begin2,int end2)
        {
            int[] res = new int[end1-begin1];

            int k = Math.Max(end1 - begin1, end2 - begin2);
            int dx = FToneAnalysis.getmin2x(k);
            //double dl = (double)NNAnalysis.samplingRate / dx;

            double[] fftdata1 = new double[dx];
            double[] fftdataimag1 = new double[dx];
            Array.Copy(data1, begin1, fftdata1, 0, Math.Min(k, end1 - begin1));
            TWFFT.FFT(fftdata1, fftdataimag1);

            double[] fftdata2 = new double[dx];
            double[] fftdataimag2 = new double[dx];
            Array.Copy(data2, begin2, fftdata2, 0, Math.Min(k, end2 - begin2));
            TWFFT.FFT(fftdata2, fftdataimag2);

            int num = 15;
            double[] lineori1 = new double[dx];
            double[] lineori2 = new double[dx];
            for(int i = 0; i < dx; i++)
            {
                lineori1[i] = Math.Abs(fftdata1[i]);
                lineori2[i] = Math.Abs(fftdata2[i]);
                //lineori1[i] = Math.Sqrt(Math.Pow(fftdata1[i], 2) + Math.Pow(fftdataimag1[i], 2));
                //lineori2[i] = Math.Sqrt(Math.Pow(fftdata2[i], 2) + Math.Pow(fftdataimag2[i], 2));
            }
            double[] line1 = FToneAnalysis.MiddleFilter(lineori1, num);
            //double[] linei1 = FToneAnalysis.MiddleFilter(fftdataimag1, num);
            double[] line2 = FToneAnalysis.MiddleFilter(lineori2, num);
            //double[] linei2 = FToneAnalysis.MiddleFilter(fftdataimag2, num);
            for (int i = 0; i < line1.Length; i++)
            {
                //Complex c1 = new Complex(fftdata1[i], fftdataimag1[i]);
                //Complex cl1 = new Complex(line1[i], linei1[i]);
                //Complex cl2 = new Complex(line2[i], linei2[i]);
                //c1 = c1.Multiply(cl1.Division(cl2));
                fftdata1[i] = (int)((double)fftdata1[i] *(line2[i] / line1[i]));
                //fftdataimag1[i] += (int)(linei2[i]-linei1[i]);
            }
            for(int i = dx/4; i < dx/2; i++)
            {
                if (line2[i] < line1[i])
                {
                    fftdata1[i] = fftdata2[i];
                    fftdata1[dx - i] = fftdata2[dx - i];
                }
            }

            TWFFT.IFFT(fftdata1, fftdataimag1);
            for (int i = 0; i < res.Length; i++) res[i] = (int)fftdata1[i];

            return res;
        }

        public static int[] getFCorrection(int[] oridata,int[] data,int windowlen=440)
        {
            int[] res = new int[data.Length];
            int nowpos = 0;
            int oripos = 0;
            while (nowpos < data.Length - windowlen * 2)
            {
                int k = Math.Min(windowlen * 2, data.Length - nowpos);
                oripos = (int)(nowpos * ((double)oridata.Length / data.Length));
                var tmp = MiddleFilterCorrection(data, nowpos, nowpos + k, oridata, oripos, oripos + k);
                tmp = hanning(tmp);
                for(int i = 0; i < tmp.Length; i++)
                {
                    res[nowpos + i] += tmp[i];
                }
                nowpos += windowlen;
            }
            return res;
        }


        /// <summary>
        /// 获取最相似的位置，按平方差来计算
        /// </summary>
        /// <param name="oridata"></param>
        /// <param name="compdata"></param>
        /// <returns></returns>
        private static int getSamePosition(int[] oridata,int begin,int len,int compbegin,int complen)
        {
            int pos = begin;
            double mindif = double.MaxValue;
            for(int i = begin; i < begin+len - complen; i++)
            {
                double dif = 0;
                for(int j = 0; j < complen; j++)
                {
                    dif += Math.Pow(oridata[i + j] - oridata[compbegin + j], 2);
                    if (dif > mindif) break;
                }
                if (dif < mindif)
                {
                    pos = i;
                    mindif = dif;
                    break;
                }
            }
            return pos;
        }

        /// <summary>
        /// 用WSOLA变长不变调
        /// </summary>
        /// <param name="oridata"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static int[] getWChangeSound1(int[] oridata,int length)
        {
            int[] res = new int[length];

            
            int framelen = NNAnalysis.samplingRate / 100;
            int framenum = length / framelen;

            double scale = (double)length / oridata.Length;
            //int copytarget = 0;
            int oritarget = 0;
            int sametarget = 0;
            int findlen = framelen * 3;
            int comp1start = 0;
            int comp2len = 0;
            for (int i = 0; i < framenum; i++)
            {
                Array.Copy(oridata, oritarget, res, i * framelen, Math.Min(framelen, oridata.Length - oritarget));
                if (oritarget>=oridata.Length-framelen)
                {
                    Array.Copy(oridata, oritarget, res, res.Length - (oridata.Length - oritarget), oridata.Length - oritarget);
                    break;
                }
                sametarget = oritarget + framelen;
                //set new oritarget
                // 目标位置
                oritarget = (int)( i * framelen/scale);
                // 比较附近相似度，找出最相似的位置
                comp1start = Math.Min(Math.Max(oritarget - framelen, 0), oridata.Length - findlen - 1);
                comp2len = Math.Min(framelen, oridata.Length - sametarget - 1);
                //int[] comp1 = new int[findlen];
                //int[] comp2 = new int[comp2len];
                //Array.Copy(oridata, comp1start, comp1, 0, findlen);
                //Array.Copy(oridata, sametarget, comp2, 0, comp2len);
                oritarget = getSamePosition(oridata,comp1start,findlen,sametarget,comp2len);

            }
            return res;
        }

        private static int getFrameLength(int[] frame,int index)
        {
            if (index > 0 && index < frame.Length) return frame[index] - frame[index - 1];
            else if (index == 0) return frame[index];
            return 0;
        }

        private static int[] getChangeSoundOld(int[] oridata, double[] pit,int length)
        {
            
            int[] res=new int[length];            
            int[] frame = FToneAnalysis.cutIt(oridata);
            var zips = getZipPoint(length, pit);
            //for (int i = 0; i < frame.Length; i++) frame[i] += cutbegin;
            if (frame.Length == 0 || length < (int)(frame[0] * zips[0]))
            {
                // 目标长度小于第一个基音的长度，就不再合成，直接截断当作输出
                double j = 0;
                for (int i = 0; i < length; i++)
                {
                    res[i] = oridata[(int)j];
                    j += zips[0];
                }
            }
            else
            {
                //步长缩放因子
                double scale = (double)oridata.Length / length;

                int nowloc = 0;
                while (nowloc < length)
                {
                    int tmpnow = (int)(nowloc * scale);
                    int nextframe = getClosestFrame(frame, tmpnow);
                    int framelen = getFrameLength(frame, nextframe);
                    if (nowloc > length - (oridata.Length - frame[frame.Length - 1]) / zips[nowloc])
                    {
                        //use the final frame (tail)
                        double j = 0;
                        double oldj = 0;
                        int stilltime = 1;
                        int stilltimenow = stilltime;
                        double olddelta = 1;
                        for (int i = 0; i < oridata.Length - frame[frame.Length - 1]; i++)
                        {

                            if (nowloc + (int)j >= length || frame[frame.Length - 1] + i >= oridata.Length) break;
                            else
                            {
                                //interpolation
                                for (int s = (int)oldj; s < (int)j; s++)
                                {
                                    if (nowloc + (int)s >= res.Length) continue;
                                    res[nowloc + (int)s] = oridata[frame[nextframe] - framelen + i];
                                }
                                double delta = 1 / zips[nowloc + (int)j];
                                if (delta > 100) delta = 100;
                                oldj = j;
                                if (stilltimenow++ >= stilltime)
                                {
                                    olddelta = delta;
                                    stilltimenow = 0;
                                }
                                j += olddelta;
                            }

                        }
                        break;
                    }
                    else
                    {
                        //copy this frame
                        double j = 0;
                        double oldj = 0;

                        int stilltime = 1;
                        int stilltimenow = stilltime;
                        double olddelta = 1;
                        for (int i = 0; i < framelen; i++)
                        {
                            //interpolation
                            for (int s = (int)oldj; s < (int)j; s++)
                            {
                                if (nowloc + (int)s >= res.Length) break;
                                res[nowloc + (int)s] = oridata[Math.Min(oridata.Length - 1, frame[nextframe] - framelen + i)];
                            }
                            if (nowloc + (int)j >= zips.Length) continue;

                            double delta = 1 / zips[nowloc + (int)j];
                            if (delta > 100) delta = 100;
                            oldj = j;
                            if (stilltimenow++ >= stilltime)
                            {
                                olddelta = delta;
                                stilltimenow = 0;
                            }
                            j += olddelta;
                        }
                        nowloc += (int)oldj;
                    }
                }
            }




            ////全局整形
            //int[] newres = new int[res.Length];
            //int dx = 1024;

            //int nowx1 = 0;
            //int nowx2 = 0;
            //while (nowx1 < oridata.Length)
            //{
            //    double[] data1 = new double[dx];
            //    double[] data2 = new double[dx];
            //    double[] dataimag1 = new double[dx];
            //    for (int i = 0; i < dx; i++) dataimag1[i] = 0;
            //    double[] dataimag2 = new double[dx];
            //    for (int i = 0; i < dx; i++) dataimag2[i] = 0;
            //    for (int i = 0; i < dx; i++)
            //    {

            //        data1[i] = nowx1 + i < oridata.Length ? oridata[nowx1 + i] : 0;
            //        data2[i] = nowx2 + i < res.Length ? (double)res[nowx2 + i] : 0;
            //    }
            //    TWFFT.FFT(data1, dataimag1);
            //    TWFFT.FFT(data2, dataimag2);

            //    double[] data1t = new double[dx];
            //    double[] data2t = new double[dx];
            //    for (int i = 0; i < dx; i++) data1t[i] = Math.Sqrt(data1[i] * data1[i] + dataimag1[i] * dataimag1[i]);
            //    for (int i = 0; i < dx; i++) data2t[i] = Math.Sqrt(data2[i] * data2[i] + dataimag2[i] * dataimag2[i]);
            //    int[,] envori = FormantAnalysis.getEnvelope3(data1t);
            //    int[,] env = FormantAnalysis.getEnvelope3(data2t);
            //    double[] envdelta = new double[env.GetLength(0)];
            //    //整形
            //    for (int i = 0; i < envdelta.Length; i++)
            //    {
            //        if (env[i, 1] == 0) envdelta[i] = 0;
            //        else envdelta[i] = (double)envori[i, 1] / env[i, 1];
            //    }
            //    for (int i = 1; i < envdelta.Length; i++)
            //    {
            //        for (int j = env[i - 1, 0]; j < env[i, 0]; j++)
            //        {
            //            data2[j] *= envdelta[i];
            //            dataimag2[j] *= envdelta[i];
            //        }
            //    }

            //    ////高通滤波
            //    //for (int i = 0; i < dx; i++)
            //    //{
            //    //    if (data1t[i] < 1000)
            //    //    {
            //    //        data2[i] = data1[i];
            //    //        dataimag2[i] = dataimag1[i];
            //    //    }

            //    //}


            //for (int i = (int)(dx/2 * rbegin); i < (int)(dx/2 * rend); i++)
            //{
            //    data2[i] = 0;
            //    dataimag2[i] = 0;
            //}
            //for (int i = (int)(dx - dx / 2 * rbegin)-1; i >= (int)(dx - dx / 2 * rend); i--)
            //{
            //    data2[i] = 0;
            //    dataimag2[i] = 0;
            //}
            //    TWFFT.IFFT(data2, dataimag2);
            //    for (int i = 0; i < dx; i++)
            //        if (nowx2 + i < res.Length)
            //            newres[nowx2 + i] += (int)((double)data2[i] * (i < dx / 2 ? (double)i * 2.0 / dx : 2.0 - (double)(i) * 2.0 / dx));
            //    //newres[nowx2 + i] += (int)((double)data2[i] * Math.Sin(Math.PI * ((double)i / dx)));
            //    nowx2 += dx / 2;
            //    nowx1 += (int)((double)dx / 2 * oridata.Length / res.Length);
            //}
            //Array.Copy(newres, res, res.Length);



            ////填充空白
            //for (int m = 0; m < 2; m++)
            //{
            //    int t = 1 + m;
            //    for (int i = t; i < newlen - t; i++)
            //    {
            //        if (Math.Abs(res[i - t] * res[i + t]) > 100000 && res[i] < 20000)
            //        {
            //            res[i] = (res[i - t] + res[i + t]) / 2;
            //        }
            //    }
            //}



            //收尾
            double percent = 0.1;
            int num = (int)(length * percent);
            //double scale = 1 / num;
            for (int i = length - num; i < length; i++)
            {
                res[i] = (int)(res[i] * (double)(length - i) / num);

            }


            //for (int i = 0; i < newlen; i++)
            //{
            //    if (Math.Abs(res[i]) <= 0)
            //    {
            //        res[i] = 0;
            //    }
            //}

            return res;
        }


        /// <summary>
        /// 合成
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="newlen"></param>
        /// <param name="pit"></param>
        /// <param name="rbegin"></param>
        /// <param name="rend"></param>
        /// <returns></returns>
        private static int[] getSound(int[] oridata, double[] pit,int length)
        {
            
            int[] res = oridata;
            
            //double[] zips = HanSynthesis.getZipPoint(oridata.Length, pit);
            //变调
            res = getChangeSound2(res, pit,length);
            //变长
            //int[] res = getWChangeSound1(oridata, length);
            //int[] res = getChangeSoundOld(oridata, pit, length);
            res = FToneAnalysis.MiddleFilter(res, 3);
            //res = getFCorrection(oridata, res);
            
           // res = FToneAnalysis.MiddleFilter(res, 3);
            //res = FToneAnalysis.FrequencyFilter(res, 60, 9000);

            return res;
        }
    }
}
