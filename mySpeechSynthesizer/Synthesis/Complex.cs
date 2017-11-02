using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    /// <summary>  
    /// 复数类  
    /// </summary>  
    public class Complex
    {
        #region 字段

        //复数实部  
        private double real = 0.0;

        //复数虚部  
        private double imaginary = 0.0;

        #endregion

        #region 属性

        /// <summary>  
        /// 获取或设置复数的实部  
        /// </summary>  
        public double Real
        {
            get
            {
                return real;
            }
            set
            {
                real = value;
            }
        }

        /// <summary>  
        /// 获取或设置复数的虚部  
        /// </summary>  
        public double Imaginary
        {
            get
            {
                return imaginary;
            }
            set
            {
                imaginary = value;
            }
        }

        #endregion


        #region 构造函数

        /// <summary>  
        /// 默认构造函数，得到的复数为0  
        /// </summary>  
        public Complex()
            : this(0, 0)
        {

        }

        /// <summary>  
        /// 只给实部赋值的构造函数，虚部将取0  
        /// </summary>  
        /// <param name="dbreal">实部</param>  
        public Complex(double dbreal)
            : this(dbreal, 0)
        {

        }

        /// <summary>  
        /// 一般形式的构造函数  
        /// </summary>  
        /// <param name="dbreal">实部</param>  
        /// <param name="dbImage">虚部</param>  
        public Complex(double dbreal, double dbImage)
        {
            real = dbreal;
            imaginary = dbImage;
        }

        /// <summary>  
        /// 以拷贝另一个复数的形式赋值的构造函数  
        /// </summary>  
        /// <param name="other">复数</param>  
        public Complex(Complex other)
        {
            real = other.real;
            imaginary = other.imaginary;
        }

        #endregion

        #region 重载

        //加法的重载  
        public static Complex operator +(Complex comp1, Complex comp2)
        {
            return comp1.Add(comp2);
        }

        //减法的重载  
        public static Complex operator -(Complex comp1, Complex comp2)
        {
            return comp1.Substract(comp2);
        }

        //乘法的重载  
        public static Complex operator *(Complex comp1, Complex comp2)
        {
            return comp1.Multiply(comp2);
        }

        //==的重载  
        public static bool operator ==(Complex z1, Complex z2)
        {
            return ((z1.real == z2.real) && (z1.imaginary == z2.imaginary));
        }

        //!=的重载  
        public static bool operator !=(Complex z1, Complex z2)
        {
            if (z1.real == z2.real)
            {
                return (z1.imaginary != z2.imaginary);
            }
            return true;
        }

        /// <summary>  
        /// 重载ToString方法,打印复数字符串  
        /// </summary>  
        /// <returns>打印字符串</returns>  
        public override string ToString()
        {
            if (Real == 0 && imaginary == 0)
            {
                return string.Format("{0}", 0);
            }
            if (Real == 0 && (imaginary != 1 && imaginary != -1))
            {
                return string.Format("{0} i", imaginary);
            }
            if (imaginary == 0)
            {
                return string.Format("{0}", Real);
            }
            if (imaginary == 1)
            {
                return string.Format("i");
            }
            if (imaginary == -1)
            {
                return string.Format("- i");
            }
            if (imaginary < 0)
            {
                return string.Format("{0} - {1} i", Real, -imaginary);
            }
            return string.Format("{0} + {1} i", Real, imaginary);
        }

        #endregion

        #region 公共方法

        /// <summary>  
        /// 复数加法  
        /// </summary>  
        /// <param name="comp">待加复数</param>  
        /// <returns>返回相加后的复数</returns>  
        public Complex Add(Complex comp)
        {
            double x = real + comp.real;
            double y = imaginary + comp.imaginary;

            return new Complex(x, y);
        }

        /// <summary>  
        /// 复数减法  
        /// </summary>  
        /// <param name="comp">待减复数</param>  
        /// <returns>返回相减后的复数</returns>  
        public Complex Substract(Complex comp)
        {
            double x = real - comp.real;
            double y = imaginary - comp.imaginary;

            return new Complex(x, y);
        }

        /// <summary>  
        /// 复数乘法  
        /// </summary>  
        /// <param name="comp">待乘复数</param>  
        /// <returns>返回相乘后的复数</returns>  
        public Complex Multiply(Complex comp)
        {
            double x = real * comp.real - imaginary * comp.imaginary;
            double y = real * comp.imaginary + imaginary * comp.real;

            return new Complex(x, y);
        }

        public Complex Division(Complex comp)
        {
            double x = real * comp.real + imaginary * comp.imaginary;
            double y = imaginary * comp.real - real * comp.imaginary;
            double t = imaginary * imaginary + comp.imaginary * comp.imaginary;
            x /= t;
            y /= t;

            return new Complex(x, y);
        }

        /// <summary>  
        /// 获取复数的模/幅度  
        /// </summary>  
        /// <returns>返回复数的模</returns>  
        public double GetModul()
        {
            return Math.Sqrt(real * real + imaginary * imaginary);
        }

        /// <summary>  
        /// 获取复数的相位角，取值范围（-π，π]  
        /// </summary>  
        /// <returns>返回复数的相角</returns>  
        public double GetAngle()
        {
            #region 原先求相角的实现，后发现Math.Atan2已经封装好后注释
            ////实部和虚部都为0  
            //if (real == 0 && imaginary == 0)  
            //{  
            //    return 0;  
            //}  
            //if (real == 0)  
            //{  
            //    if (imaginary > 0)  
            //        return Math.PI / 2;  
            //    else  
            //        return -Math.PI / 2;  
            //}  
            //else  
            //{  
            //    if (real > 0)  
            //    {  
            //        return Math.Atan2(imaginary, real);  
            //    }  
            //    else  
            //    {  
            //        if (imaginary >= 0)  
            //            return Math.Atan2(imaginary, real) + Math.PI;  
            //        else  
            //            return Math.Atan2(imaginary, real) - Math.PI;  
            //    }  
            //}  
            #endregion

            return Math.Atan2(imaginary, real);
        }

        /// <summary>  
        /// 获取复数的共轭复数  
        /// </summary>  
        /// <returns>返回共轭复数</returns>  
        public Complex Conjugate()
        {
            return new Complex(this.real, -this.imaginary);
        }

        #endregion
    }  
}
