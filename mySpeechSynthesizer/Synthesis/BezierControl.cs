using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 贝塞尔曲线相关函数
    /// </summary>
    class BezierControl
    {

        //三次贝塞尔曲线
        public static double bezier3funcX(double uu, mPoint[] controlP)
        {
            double part0 = controlP[0].x * uu * uu * uu;
            double part1 = 3 * controlP[1].x * uu * uu * (1 - uu);
            double part2 = 3 * controlP[2].x * uu * (1 - uu) * (1 - uu);
            double part3 = controlP[3].x * (1 - uu) * (1 - uu) * (1 - uu);
            return part0 + part1 + part2 + part3;
        }
        public static double bezier3funcY(double uu, mPoint[] controlP)
        {
            double part0 = controlP[0].y * uu * uu * uu;
            double part1 = 3 * controlP[1].y * uu * uu * (1 - uu);
            double part2 = 3 * controlP[2].y * uu * (1 - uu) * (1 - uu);
            double part3 = controlP[3].y * (1 - uu) * (1 - uu) * (1 - uu);
            return part0 + part1 + part2 + part3;
        }

        public static mPoint[] createCurve(mPoint[] orip)
        {
            List<mPoint> curvePoint = new List<mPoint>();
            int oripl = orip.Length;
            double scale = 0.16;


            mPoint[] ap = new mPoint[oripl];
            mPoint[] bp = new mPoint[oripl];

            ap[0] = new mPoint(
                orip[0].x + (orip[1].x - orip[0].x) * scale,
                orip[0].y + (orip[1].y - orip[0].y) * scale);
            bp[oripl - 2] = new mPoint(
                orip[oripl - 1].x - (orip[oripl - 1].x - orip[oripl - 2].x) * scale,
                orip[oripl - 1].y - (orip[oripl - 1].y - orip[oripl - 2].y) * scale);
            for (int i = 1; i < oripl - 1; i++)
            {
                ap[i] = new mPoint(
                orip[i].x + (orip[i + 1].x - orip[i - 1].x) * scale,
                orip[i].y + (orip[i + 1].y - orip[i - 1].y) * scale);
            }
            for (int i = 0; i < oripl - 2; i++)
            {
                bp[i] = new mPoint(
                orip[i + 1].x - (orip[i + 2].x - orip[i].x) * scale,
                orip[i + 1].y - (orip[i + 2].y - orip[i].y) * scale);
            }

            //生成4控制点，产生贝塞尔曲线
            for (int i = 0; i < oripl - 1; i++)
            {
                mPoint[] points = new mPoint[4];
                points[0] = orip[i];
                points[1] = ap[i];
                points[2] = bp[i];
                points[3] = orip[i + 1];
                double u = 1;
                double du = 1.0 / (points[3].x - points[0].x);
                for (int j = 0; j < points[3].x - points[0].x; j++)
                {
                    int px = (int)bezier3funcX(u, points);
                    int py = (int)bezier3funcY(u, points);
                    //u的步长决定曲线的疏密
                    u -= du;
                    mPoint tempP = new mPoint(px, py);
                    //存入曲线点 
                    curvePoint.Add(tempP);
                }
            }
            return curvePoint.ToArray();
        }
    }
}
