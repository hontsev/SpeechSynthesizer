using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mySpeechSynthesizer.Analysis
{
    /// <summary>
    /// 系统内的国际音标表示格式
    /// 大致与IPA表对应，以左上角为坐标原点向下向右递增
    /// 参考维基百科的更为全面的辅音表做了扩展
    /// 
    /// type：音素类型。0气流辅音，1非气流辅音，2元音
    /// 
    /// 气流辅音的x：  发音部位。0双唇，  1唇齿，2舌唇，3齿，4齿龈，5龈后，6卷舌，7龈颚，8硬腭，9软腭，10小舌，11咽，12声门
    /// 气流辅音的y：  发音方式。0鼻音，1塞音，2有咝塞擦音，3无咝塞擦音，4有咝擦音，5无咝擦音，6近音，7闪音，8颤音，9边塞擦音，10边擦音，11边近音，12边闪音
    /// 
    /// 非气流辅音的x：发音部位。0双唇，  1唇齿，2舌唇，3齿，4齿龈，5龈后，6卷舌，7龈颚，8硬腭，9软腭，10小舌，11咽，12声门
    /// 非气流辅音的y：发音方式。0搭嘴音，1内爆音，  2挤喉音
    /// 
    /// 元音的x：      舌位。    0舌最前，1舌次前，  2舌中前，3舌中，4舌中后，  5舌次后，6舌最后
    /// 元音的y：      开口大小。0全闭，  1次全闭，  2半闭，  3一半，4半开，    5次全开，6全开
    /// 
    /// z：            清浊。    0清，    1浊。
    /// 
    /// sym：变音符号。一共32种。用序号数组表示
    /// 0成音节，1不成音节，2送气，3无声除阻，4鼻音除阻
    /// 5边音除阻，6清化，7浊化，8漏气音，9吱嘎音
    /// 10齿化，11舌唇化，12舌尖化，13舌叶化，14较前
    /// 15较后，16较央，17中央化，18较高，19较低
    /// 20更圆唇，21更展唇，22唇化，23颚化，24软颚化
    /// 25喉壁化，26硬颚化，27软颚化或喉壁化，28舌根前移，29舌根后移
    /// 30鼻音化，31R音化
    /// </summary>
    public class IPAUnit
    {
        public int x;
        public int y;
        public int z;
        public int type;
        public int[] sym;

        public IPAUnit(int type,int x,int y,int z,params int[] sym)
        {
            this.type = type;
            this.x = x;
            this.y = y;
            this.z = z;
            this.sym = sym;
        }
    }

    /// <summary>
    /// 国际音标处理类
    /// </summary>
    public class IPAManager
    {
        /// <summary>
        /// 拼音还原成声母、韵母列表
        /// </summary>
        /// <param name="pchar"></param>
        /// <returns></returns>
        public static string[] SplitPinyin(string pchar)
        {
            List<string> res = new List<string>();

            //v复原
            pchar = pchar.Replace("yu", "v");
            pchar = pchar.Replace("ju", "jv");
            pchar = pchar.Replace("qu", "qv");
            pchar = pchar.Replace("xu", "xv");

            //开头i u复原
            pchar = pchar.Replace("y", "i");
            pchar = pchar.Replace("w", "u");
            pchar = pchar.Replace("ii", "i").Replace("uu", "u");

            //iu、ui、un复原
            pchar = pchar.Replace("iu", "iou");
            pchar = pchar.Replace("ui", "uei");
            pchar = pchar.Replace("un", "uen");

            string[] ym = new string[] { "b", "p", "m", "f", "d", "t", "n", "l", "g", "k", "h", "j", "q", "x", "z", "c", "s", "r", "zh", "ch", "sh" };
            foreach(var item in ym)
            {
                if (pchar.StartsWith(item))
                {
                    res.Add(item);
                    pchar = pchar.Substring(item.Length);
                    break;
                }
            }
            if (!string.IsNullOrWhiteSpace(pchar)) res.Add(pchar);
            return res.ToArray();
        }

        public static IPAUnit[] Pinyin2IPA(string pchar)
        {
            Dictionary<string, IPAUnit[]> dict = new Dictionary<string, IPAUnit[]>
            {
                {"m",new IPAUnit[]{new IPAUnit(0,0,0,1) } },
                {"n",new IPAUnit[]{new IPAUnit(0,4,0,1) } },
                {"ng",new IPAUnit[]{new IPAUnit(0,9,0,1) } },
                {"b",new IPAUnit[]{new IPAUnit(0,0,1,0) } },
                {"d",new IPAUnit[]{new IPAUnit(0,4,1,0) } },
                {"g",new IPAUnit[]{new IPAUnit(0,9,1,0)} },
                {"p",new IPAUnit[]{new IPAUnit(0,0,1,0,2) } },
                {"t",new IPAUnit[]{new IPAUnit(0,4,1,0,2)} },
                {"k",new IPAUnit[]{new IPAUnit(0,9,1,0,2)} },
                {"z",new IPAUnit[]{new IPAUnit(0,4,2,0) } },
                {"zh",new IPAUnit[]{new IPAUnit(0,6,2,0) } },
                {"j",new IPAUnit[]{new IPAUnit(0,7,2,0) } },
                {"c",new IPAUnit[]{new IPAUnit(0,4,2,0,2) } },
                {"ch",new IPAUnit[]{new IPAUnit(0,6,2,0,2) } },
                {"q",new IPAUnit[]{new IPAUnit(0,7,2,0,2) } },
                {"f",new IPAUnit[]{new IPAUnit(0,1,5,0) } },
                {"s",new IPAUnit[]{new IPAUnit(0,4,4,0) } },
                {"sh",new IPAUnit[]{new IPAUnit(0,6,4,0)} },
                {"r",new IPAUnit[]{new IPAUnit(0,6,4,1) } },
                {"x",new IPAUnit[]{new IPAUnit(0,7,4,0) } },
                {"h",new IPAUnit[]{new IPAUnit(0,9,5,0) } },
                {"l",new IPAUnit[]{new IPAUnit(0,4,11,1) } },

                {"-i",new IPAUnit[]{new IPAUnit(1,3,0,0) } },
                {"i",new IPAUnit[]{new IPAUnit(1,0,0,0) } },
                {"u",new IPAUnit[]{new IPAUnit(1,6,0,1) } },
                {"v",new IPAUnit[]{new IPAUnit(1,0,0,1) } },
                {"a",new IPAUnit[]{new IPAUnit(1,4,6,0) } },
                {"ia",new IPAUnit[]{ new IPAUnit(1,0,0,0,1), new IPAUnit(1, 4, 6, 0) } },
                {"ua",new IPAUnit[]{ new IPAUnit(1,6,0,1,1), new IPAUnit(1, 4, 6, 0) } },
                {"o",new IPAUnit[]{new IPAUnit(1,6,4,1) } },
                {"uo",new IPAUnit[]{ new IPAUnit(1,6,0,1,1), new IPAUnit(1, 6, 2, 1) } },
                {"e",new IPAUnit[]{new IPAUnit(1,6,2,0) } },
                {"e^",new IPAUnit[]{new IPAUnit(1,2,4,0) } },
                {"ie",new IPAUnit[]{new IPAUnit(1,0,0,0,1),new IPAUnit(1,1,2,0) } },
                {"ve",new IPAUnit[]{new IPAUnit(1,0,0,1,1),new IPAUnit(1,1,2,0) } },
                {"er",new IPAUnit[]{new IPAUnit(1,4,5,0),new IPAUnit(1,4,3,0,1,31) } },
                {"ai",new IPAUnit[]{new IPAUnit(1,3,6,0),new IPAUnit(1,0,0,1,1) } },
                {"uai",new IPAUnit[]{ new IPAUnit(1,6,0,1,1), new IPAUnit(1,3,6,0),new IPAUnit(1,0,0,1,1) } },
                {"ei",new IPAUnit[]{new IPAUnit(1,1,2,0),new IPAUnit(1,0,0,1,1) } },
                {"uei",new IPAUnit[]{ new IPAUnit(1, 6, 0, 1, 1), new IPAUnit(1,1,2,0),new IPAUnit(1,0,0,1,1) } },
                {"ao",new IPAUnit[]{new IPAUnit(1,6,6,0),new IPAUnit(1,6,0,1,1) } },
                {"iao",new IPAUnit[]{ new IPAUnit(1, 0, 0, 1, 1),new IPAUnit(1,3,6,0),new IPAUnit(1,0,0,1,1) } },
                {"ou",new IPAUnit[]{new IPAUnit(1,6,2,1),new IPAUnit(1,6,0,1,1) } },
                {"iou",new IPAUnit[]{ new IPAUnit(1, 0, 0, 1, 1), new IPAUnit(1,6,2,1),new IPAUnit(1,6,0,1,1) } },
                {"an",new IPAUnit[]{new IPAUnit(1,3,6,0),new IPAUnit(0,4,0,1) } },
                {"ian",new IPAUnit[]{ new IPAUnit(1, 0, 0, 1, 1), new IPAUnit(1,2,4,0),new IPAUnit(0,4,0,1) } },
                {"uan",new IPAUnit[]{new IPAUnit(1,6,0,1,1), new IPAUnit(1, 3, 6, 0), new IPAUnit(0, 4, 0, 1) } },
                {"van",new IPAUnit[]{new IPAUnit(1,0,0,1,1),new IPAUnit(1,4,5,0), new IPAUnit(0, 4, 0, 1) } },
                {"en",new IPAUnit[]{new IPAUnit(1,4,3,0), new IPAUnit(0, 4, 0, 1) } },
                {"uen",new IPAUnit[]{new IPAUnit(1,6,0,1,1), new IPAUnit(1, 4, 3, 0), new IPAUnit(0, 4, 0, 1) } },
                {"in",new IPAUnit[]{new IPAUnit(1,0,0,0), new IPAUnit(0, 4, 0, 1) } },
                {"vn",new IPAUnit[]{new IPAUnit(1,0,0,1), new IPAUnit(0, 4, 0, 1) } },
                {"ang",new IPAUnit[]{new IPAUnit(1,6,6,0),new IPAUnit(0,9,0,1) } },
                {"iang",new IPAUnit[]{new IPAUnit(1,0,0,0,1), new IPAUnit(1, 6, 6, 0), new IPAUnit(0, 9, 0, 1) } },
                {"uang",new IPAUnit[]{new IPAUnit(1,6,0,1,1), new IPAUnit(1, 6, 6, 0), new IPAUnit(0, 9, 0, 1) } },
                {"eng",new IPAUnit[]{new IPAUnit(1,4,3,0),new IPAUnit(0,9,0,1) } },
                {"ing",new IPAUnit[]{new IPAUnit(1,0,0,0),new IPAUnit(0,9,0,1) } },
                {"ueng",new IPAUnit[]{new IPAUnit(1,6,0,1,1), new IPAUnit(1, 4, 3, 0), new IPAUnit(0, 9, 0, 1) } },
                {"ong",new IPAUnit[]{new IPAUnit(1,5,1,1),new IPAUnit(0,9,0,1) } },
                {"iong",new IPAUnit[]{new IPAUnit(1,0,0,0,1), new IPAUnit(1, 5, 1, 1), new IPAUnit(0, 9, 0, 1) } }
            };

            var pylist = SplitPinyin(pchar);
            List<IPAUnit> res = new List<IPAUnit>();
            foreach(var p in pylist)
            {
                if (!dict.ContainsKey(p)) continue;
                var tmp = dict[p];
                foreach(var titem in tmp)
                {
                    res.Add(titem);
                }
            }
            return res.ToArray();
        }
    }
}
