using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>
    /// 包络检波功能，模拟了硬件半波检波和全波检波功能 
    /// </summary>
    class ENVDetector
    {
        double m_rct;
        double m_old;

        /** \brief 初始化 
         * 
         * \param rct 为RC低通滤波的时间常数 
         * \return 
         */
        public ENVDetector(double rct=100)
        {
            m_rct = rct;
            m_old = 0.0;
        }

        /** \brief 半波包络检测 
         * 
         * \param in 输入波形，每次传入一个数据点 
         * \return 输出波形 
         */
        public double env_half(double tin)
        {
            if (tin > m_old)
            {
                m_old = tin;
            }
            else
            {
                m_old *= m_rct / (m_rct + 1);
            }
            return m_old;
        }
        /** \brief 半波包络检测 
         * 
         * \param in[] 输入波形 
         * \param N 数组的点数 
         * \param out[] 输出波形 
         * \return 
         */
        public double[] env_half(double[] tin)
        {
            int N = tin.Length;
            double[] tout = new double[N];
            for (int i = 0; i < N; i++)
            {
                if (tin[i] > m_old)
                {
                    m_old = tin[i];
                    tout[i] = m_old;
                }
                else
                {
                    m_old *= m_rct / (m_rct + 1);
                    tout[i] = m_old;
                }
            }
            return tout;
        }

        /** \brief 全波包络检测 
         * 
         * \param in 输入波形，每次传入一个数据点 
         * \return 输出波形 
         */
        public double env_full(double tin)
        {
            double abs_in = Math.Abs(tin);
            if (abs_in > m_old)
            {
                m_old = abs_in;
            }
            else
            {
                m_old *= m_rct / (m_rct + 1);
            }
            return m_old;
        }
        /** \brief 全波包络检测 
         * 
         * \param in[] 输入波形 
         * \param N 数组的点数 
         * \return 
         */
        public double[] env_full(double[] tin)
        {
            int N = tin.Length;
            double[] tout = new double[N];
            double abs_in;
            for (int i = 0; i < N; i++)
            {
                abs_in = Math.Abs(tin[i]);
                if (abs_in > m_old)
                {
                    m_old = abs_in;
                    tout[i] = m_old;
                }
                else
                {
                    m_old *= m_rct / (m_rct + 1);
                    tout[i] = m_old;
                }
            }
            return tout;
        }
    }
}
