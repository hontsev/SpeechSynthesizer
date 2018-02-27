using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer
{
    public class MyDelegates
    {
        public delegate void sendStringDelegate(string str);
        public delegate void sendVoidDelegate();
        public delegate void sendBoolDelegate(bool b);
    }
}
