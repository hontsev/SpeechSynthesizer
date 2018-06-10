using mySpeechSynthesizer.NNLink;
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

        public static int[] getWAVdata(Bank tl,string tonename, double[][] pit, int length,double pitch)
        {
            //pit = getNewPit(pit, SemitoneTransfer.getTN( tl.sunit[tonename].pitch), pitch);
            int[] oridata = tl.getTone(tonename);
            oridata= FToneAnalysis.getSoundPart(oridata);
            int newlen = (int)((double)length / 1000 * NBankManager.samplingRate);

            int[] newdata = getChangeSoundTDPSOLA(oridata, pit, newlen);

            return newdata;


        }

        /// <summary>
        /// 最接近它的右侧帧
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="nowloc"></param>
        /// <returns></returns>
        private static int getCRFrame(int[] frames, int nowloc)
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

        /// <summary>
        /// 最接近它的左侧帧
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="nowloc"></param>
        /// <returns></returns>
        private static int getCLFrame(int[] frames, int nowloc)
        {
            if (nowloc > frames[frames.Length - 1])
                return frames.Length - 1;
            else if (nowloc > 0 && nowloc < frames[0])
                return 0;
            else
            {
                for (int i = 1; i < frames.Length; i++)
                {
                    if (nowloc >= frames[i - 1] && nowloc <= frames[i]) return i-1;
                }
            }
            return 0;
        }

        private static Phone getClosestFTone(Phone[] ftones,int loc)
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
        private static int[] Hanning(int[] n)
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

        private static int[] antiHanning(int[] n)
        {
            int N = n.Length;
            int[] res = new int[N];

            int[] hanning = Hanning(n);
            for(int i = 0; i < N; i++)
            {
                res[i] = (int)(n[i] /  hanning[i]);
            }

            return res;

        }

        /// <summary>
        /// 三角窗
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
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
            int framelen = NBankManager.samplingRate / 100;
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

        private static double getResampleValue(double[] data, double target)
        {
            int p1 = (int)target;
            int p2 = p1 + 1;
            if (target > data.Length/2 || target < 1) return 0;// data[data.Length - 1];
            else return data[p1] + (target - p1) * (data[p2] - data[p1]);
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
            var res = FToneAnalysis.MiddleFilter(data2, 5);
            for (int i = 0; i < n; i++) res[i] = Math.Pow(Math.E, res[i]);
            return res;
        }



        private static int[] ChangePitResutltFD(int[] data,double dpit)
        {
            // 傅里叶点数必须2的倍数
            int n = FToneAnalysis.getmin2x(data.Length * 2);
            n = 4096;
            double[] ddata = new double[n];
            for (int i = 0; i < n; i++) if (i < data.Length) ddata[i] = data[i]; else ddata[i] = 0;
            double[] ddatai = new double[n];
            for (int i = 0; i < n; i++) ddatai[i] = 0;
            TWFFT.FFT(ddata, ddatai);
            // ln(|X(n)|)
            double[] ddatalnmod = new double[n];
            for (int i = 0; i < n; i++) ddatalnmod[i] = Math.Sqrt(ddata[i] * ddata[i] + ddatai[i] * ddatai[i]);
            // 根据倒谱获取频谱包络，即声道滤波器
            var ddataenv = GetEnvelopeFD(ddatalnmod);
            // double[] ddataenv = new double[n];
            //for (int i = 0; i < n; i++) ddataenv[i] = Math.Pow(Math.E, ddatalnenv[i]);
            // 除以包络，得到激励谱
            double[] ddatabase = new double[n];
            double[] ddataibase = new double[n];
            for (int i = 0; i < n; i++) ddatabase[i] = ddata[i];/// ddataenv[i];
            for (int i = 0; i < n; i++) ddataibase[i] = ddatai[i];/// ddataenv[i];
            // 拉伸激励谱，得到新基频下的激励谱
            double[] ddatabaser = new double[n];
            double[] ddataibaser = new double[n];
            // 对称共轭，所以分两半分别压缩
            ddatabaser[0] = ddatabase[0];
            ddataibaser[0] = ddataibase[0];
            //dpit = 1.4;
            for(int i = 1; i < n / 2; i++)
            {
                int j = (int)(i - 3000 * (dpit - 1));
                if (j < 1 || j >= n / 2)
                {
                    ddatabaser[i] = 0;
                    ddataibaser[i] = 0;
                }
                else
                {
                    ddatabaser[i] = ddatabase[j];
                    ddataibaser[i] = ddataibase[j];
                }
                ddatabaser[n - i] = ddatabaser[i];
                ddataibaser[n - i] = -ddataibaser[i];
            }
            //for(int i = 1; i <= n/2; i++)
            //{
            //    ddatabaser[i] = getResampleValue(ddatabase, (double)i / dpit);
            //    ddatabaser[n - i] = ddatabaser[i];
            //    ddataibaser[i] = getResampleValue(ddataibase, (double)i / dpit);
            //    ddataibaser[n - i] = -ddataibaser[i];
            //}
            // 激励谱与包络相乘，重建频谱
            double[] ddatar = new double[n];
            double[] ddatair = new double[n];
            for (int i = 0; i < n; i++) ddatar[i] = ddatabaser[i];//* ddataenv[i];
            for (int i = 0; i < n; i++) ddatair[i] = ddataibaser[i]; //* ddataenv[i];
            TWFFT.IFFT(ddatar, ddatair);
            // int结果
            int[] datar = new int[data.Length];
            for (int i = 0; i < data.Length; i++) datar[i] = (int)ddatar[i];
            return datar;
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

        private static int[][] getCutPair(int[] oricut, int orilen,int tarlen,double[][] zip=null)
        {
            List<int[]> res = new List<int[]>();

            double tarp = 0;
            while (tarp < tarlen)
            {
                int orip = (int)(tarp * orilen / tarlen);
                int lf = getCLFrame(oricut, orip);
                //frame1只用一遍
                if (lf == 0 && tarp > 0) lf += 1;
                int rf = Math.Min(lf + 1, oricut.Length - 1);
                int oristart = oricut[lf];
                int oriend = oricut[rf];
                if (lf == rf) oriend = orilen ;
                res.Add(new int[3] { oristart, (int)tarp, oriend-oristart });
                tarp += (oriend - oristart) / getZipValue(zip, tarp / tarlen);
            }
            return res.ToArray();
        }


        private static double getZipValue(double[][] zip, double tar)
        {
            if (zip == null) return 1;
            if (tar <= 0 || tar >= 1) return 1;
            for(int i = 0; i < zip.Length; i++)
            {
                double x1 = zip[i][0];
                double x2 = i < zip.Length - 1 ? zip[i + 1][0] : 1;
                double y1 = zip[i][1];
                double y2 = i < zip.Length - 1 ? zip[i + 1][1] : 1;
                if (i == 0 && x1 > tar) return 1 + (y1 - 1) * (tar / x1);
                //else if (i == zip.Length - 1 && x <= tar) return y + (1 - y) * ((tar - x) / (1 - x));
                else if (tar >= x1 && tar < x2) return y1 + (y2 - y1) * ((tar - x1) / (x2 - x1));
            }
            return 1;
        }


        private static int[] getChangeSoundFDPSOLA(int[] oridata,double[][] zip,int tarlength)
        {
            int[] oricut = FToneAnalysis.cutIt(oridata);
            int[] resdata = new int[tarlength];
            int orilength = oridata.Length;
            int[][] rescut = getCutPair(oricut, orilength, tarlength);
            int overlaplen = 0;
            for(int i = 0; i < rescut.Length; i++)
            {
                int oristart = rescut[i][0];
                int tarstart = rescut[i][1];
                int orilen = rescut[i][2];
                int[] pitcdata = new int[orilen + overlaplen * 2];
                double pitc = getZipValue(zip, (double)i * zip.Length / rescut.Length);
                    
                for(int j = 0; j < pitcdata.Length; j++)
                {
                    int p = oristart + j - overlaplen;
                    if (p < 0 || p>=oridata.Length) pitcdata[j] = 0;
                    else pitcdata[j] = oridata[p];
                }
                pitcdata = ChangePitResutltFD(pitcdata, pitc);
                for (int j = 0; j < orilen; j++)
                {
                    int p = tarstart + j - overlaplen;
                    if (j < overlaplen && i != 0)
                    {
                        resdata[p] =
                            (int)(resdata[p] * (1 - (double)j / overlaplen)
                            + pitcdata[j] * (double)j / overlaplen);
                    }
                    else if  ( p>=0 && p < resdata.Length)
                    {
                        resdata[p] = pitcdata[j];
                    }
                }
            }

            return resdata;
        }

        private static void repairWaveShape(int[] oridata,int[] tardata)
        {
            int n = FToneAnalysis.getmin2x(oridata.Length);
            double[] odata = new double[n];
            double[] odatai = new double[n];
            for(int i = 0; i < n; i++)
            {
                if (i < oridata.Length) odata[i] = oridata[i];
                else odata[i] = 0;
                odatai[i] = 0;
            }
            TWFFT.FFT(odata, odatai);
            double[] odatamod = new double[n];
            for (int i = 0; i < n; i++) odatamod[i] = Math.Sqrt(odata[i] * odata[i] + odatai[i] * odatai[i]);
            var odataenv = GetEnvelopeFD(odatamod);

            double[] tdata = new double[n];
            double[] tdatai = new double[n];
            for (int i = 0; i < n; i++)
            {
                if (i < tardata.Length) tdata[i] = tardata[i];
                else tdata[i] = 0;
                tdatai[i] = 0;
            }
            TWFFT.FFT(tdata, tdatai);
            double[] tdatamod = new double[n];
            for (int i = 0; i < n; i++) tdatamod[i] = Math.Sqrt(tdata[i] * tdata[i] + tdatai[i] * tdatai[i]);
            var tdataenv = GetEnvelopeFD(tdatamod);

            for(int i = 1; i < n/2; i++)
            {
                //if (odataenv[i] >= tdataenv[i]) continue;
                tdata[i] = tdata[i] * (odataenv[i] / tdataenv[i]);
                tdatai[i] = tdatai[i] * (odataenv[i] / tdataenv[i]);
                tdata[n - i] = tdata[i];
                tdatai[n - i] = tdatai[i];
            }
            TWFFT.IFFT(tdata, tdatai);
            for(int i = 0; i < tardata.Length; i++)
            {
                tardata[i] = (int)tdata[i];
            }
        }

        /// <summary>
        /// TD-PSOLA法合成
        /// </summary>
        /// <param name="oridata"></param>
        /// <param name="zip"></param>
        /// <param name="tarlength"></param>
        /// <returns></returns>
        private static int[] getChangeSoundTDPSOLA(int[] oridata, double[][] zip,int tarlength)
        {
            int[] oricut = FToneAnalysis.cutIt(oridata);
            int[] resdata = new int[tarlength];
            int orilength = oridata.Length;
            int[][] rescut = getCutPair(oricut, orilength, tarlength,zip);
            //int overlaplen = 20;
            double k = (double)tarlength / orilength;

            for (int i = 0; i < rescut.Length; i++)
            {
                int oristart = rescut[i][0];
                int tarstart = rescut[i][1];
                int orilen = rescut[i][2];

                int laplen = orilen * 2;
                int orilapstart = oristart - laplen / 2;
                int tarlapstart = tarstart - laplen / 2;
                int[] oriframe = new int[laplen];
                for(int j = 0; j < laplen; j++)
                {
                    if (orilapstart + j < 0 || orilapstart+j>= orilength) oriframe[j] = 0;
                    else oriframe[j] = oridata[orilapstart + j];
                }
                var oriframeh = Hanning(oriframe);
                for(int j = 0; j < laplen; j++)
                {
                    if (tarlapstart + j < 0 || tarlapstart + j >= tarlength) continue;
                    else
                    {
                        resdata[tarlapstart+j] += oriframeh[j];
                    }
                }
                
            }
            int[] resdata2 = new int[tarlength];
            for (int i = 0; i < tarlength; i++) resdata2[i] = 0;
            int framelen = 1024;
            for (int i = 0; i < tarlength; i += framelen / 2)
            {
                int[] od = new int[framelen];
                int[] td = new int[framelen];
                for (int j = 0; j < framelen; j++)
                {
                    int d = (int)((double)i*orilength / tarlength)+j;
                    if (d >= oridata.Length) d = oridata.Length - framelen - 1 + j;
                    if (i + j >= tarlength) { td[j] = 0; continue; }
                    od[j] = oridata[d];
                    td[j] = resdata[i + j];
                }
                //od = Hanning(od);
                //td = Hanning(td);
                repairWaveShape(od, td);
                //td = antiHanning(td);
                for (int j = 0; j < framelen; j++)
                {
                    int d = i + j;
                    if (d >= tarlength) break;
                    if (j < framelen / 2) resdata2[d] = (int)(resdata2[d] * (1 - (double)j * 2 / framelen) + td[j] * ((double)j * 2 / framelen));
                    else
                        resdata2[d] += td[j];
                }
            }

            return resdata;



            //int framenum = cut.Length-1;

            //// 获取逐帧频率压缩因子，用于接下来获取指定位置的压缩因子
            //double[] framezip = getZipPoint(oridata.Length, zip);

            //int[] res = new int[length];

            ////将开头段先放进去再说
            ////Array.Copy(oridata, 0, res, 0, Math.Min(length, cut[1]));

            //int fbegin = cut[0];
            //int fend = cut[1];
            //double nowpos = 0;
            //int respos = 0;
            ////对小段重采样
            //int i = 1;
            //while (i < framenum)
            //{
            //    fbegin = cut[i - 1];
            //    fend = cut[i];

            //    int thislen = fend - fbegin;
            //    fbegin -= thislen / 2;
            //    fend += thislen / 2;


            //    double nowzip = 1 / Math.Max(framezip[Math.Max(fbegin, 0)], 0.1);

            //    //reset nowpos
            //    nowpos = fbegin;
            //    List<int> tmp = new List<int>();
            //    while (nowpos < fend)
            //    {
            //        int val = getResampleValue(oridata, nowpos);
            //        tmp.Add(val);
            //        nowpos += nowzip;
            //    }
            //    // 滤波
            //    int[] tmp2 = MiddleFilterCorrection( tmp.ToArray(), 0, tmp.Count, oridata, Math.Max(0,fbegin), fend);
            //    //tmp2 = FToneAnalysis.FrequencyFilter(tmp2, 0, 15000);
            //    // 用汉宁窗合并
            //    //var hanningtmp = tmp.ToArray();
            //    var hanningtmp = hanning(tmp2.ToArray());
            //    for (int j = 0; j < hanningtmp.Length; j++)
            //    {
            //        if (respos + j - cut[i - 1] + fbegin >= res.Length)
            //        {
            //            break;
            //        }
            //        else if (respos+j - cut[i - 1] + fbegin >= 0)
            //        {
            //            res[respos + j - cut[i - 1] + fbegin] += hanningtmp[j];
            //            //respos++;
            //        }
            //    }
            //    // 做中值平滑
            //    int width = thislen/2;
            //    int phstart = Math.Max(0, respos - width);
            //    int phlen = Math.Min(res.Length - phstart, width * 2);
            //    int[] phdata = new int[phlen];
            //    Array.Copy(res, phstart, phdata, 0, phlen);
            //    phdata = FToneAnalysis.MiddleFilter(phdata, 3);
            //    //phdata = FToneAnalysis.MiddleFilter(phdata, 5);
            //    Array.Copy(phdata, 0, res, phstart, phlen);


            //    respos += hanningtmp.Length / 2;


            //    i = getNearestFrameIndex(oridata.Length, length, cut, respos);


            //}
            //return res;
        }

        private static int[] getChangeSoundUTAU(int[] oridata, double[][] zip, int tarlength)
        {
            int[] oricut = FToneAnalysis.cutIt(oridata);
            int[] resdata = new int[tarlength];
            int orilength = oridata.Length;
            int[][] rescut = getCutPair(oricut, orilength, tarlength, zip);
            int overlaplen = 0;
            double k = (double)tarlength / orilength;

            for (int i = 0; i < rescut.Length; i++)
            {
                int oristart = rescut[i][0];
                int tarstart = rescut[i][1];
                int orilen = rescut[i][2];
                int[] pitcdata = new int[orilen + overlaplen * 2];

                int laplen = orilen * 2;
                int orilapstart = oristart - laplen / 2;
                int tarlapstart = tarstart - laplen / 2;
                int[] oriframe = new int[laplen];
                for (int j = 0; j < laplen; j++)
                {
                    if (orilapstart + j < 0 || orilapstart + j >= orilength) oriframe[j] = 0;
                    else oriframe[j] = oridata[orilapstart + j];
                }
                var oriframeh = Hanning(oriframe);
                for (int j = 0; j < laplen; j++)
                {
                    if (tarlapstart + j < 0 || tarlapstart + j >= tarlength) continue;
                    else resdata[tarlapstart + j] += oriframeh[j];
                }

            }

            //int framelen = 4096;
            //for(int i = 0; i < tarlength; i += framelen)
            //{
            //    int[] od = new int[framelen];
            //    int[] td = new int[framelen];
            //    for(int j = 0; j < framelen; j++)
            //    {
            //        int d = (int)(i * k) + j;
            //        if (d >= oridata.Length) d = oridata.Length - framelen - 1 + j;
            //        od[j] = oridata[d];
            //        td[j] = resdata[i + j];
            //    }
            //}

            return resdata;
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
                tmp = Hanning(tmp);
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

            
            int framelen = NBankManager.samplingRate / 100;
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
                    int nextframe = getCRFrame(frame, tmpnow);
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

    }
}
