using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 二维向量
    /// </summary>
    public class mPoint
    {
        public int x = 0;
        public int y = 0;
        public mPoint()
        {
            x = y = 0;
        }
        public mPoint(int tx, int ty)
        {
            x = tx;
            y = ty;
        }
        public mPoint(double tx, double ty)
        {
            x = (int)tx;
            y = (int)ty;
        }
    }
}
