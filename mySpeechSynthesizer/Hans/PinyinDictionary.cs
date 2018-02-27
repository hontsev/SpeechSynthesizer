using System;
using System.Collections.Generic;
using System.IO;

namespace mySpeechSynthesizer
{
    class PinyinDictionary
    {
        //定义词典
        private Dictionary<string, string> dictionary;
        public Dictionary<string, string> Dictionary
        {
            get { return dictionary; }
        }

        public Dictionary<string, string> dictionary2;

        private Dictionary<string, string> getFileInfo(string filename)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (!Directory.Exists(Path.GetDirectoryName(filename))) Directory.CreateDirectory(Path.GetDirectoryName(filename));
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] tmp = sr.ReadLine().Split('|');
                        if (tmp.Length == 2)
                        {
                            try
                            {
                                dictionary[tmp[0]] = tmp[1];
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }

            return dictionary;
        }


        /// <summary>
        /// 构造函数将生成词典
        /// </summary>
        public PinyinDictionary()
        {
            string filename1 = @"Dictionary\Dictionary.txt";
            string filename2 = @"Dictionary\SimpleWordAdd.txt";

            //指定储存字典的对象
            dictionary = getFileInfo(filename1);

            //存储修复的单字缺省项
            dictionary2 = getFileInfo(filename2);
        }

        /// <summary>
        /// 根据给出的中文词汇，在字典中查找对应拼音。
        /// 找不到就返回null。
        /// 注意：dictionary不能为空
        /// </summary>
        /// <param name="cn"></param>
        /// <returns>中文词汇，对应的拼音</returns>
        public string GetPinyin(string cn)
        {
            //中文不能为空
            if (string.IsNullOrEmpty(cn))
                return null;

            //词典不能为空
            if (dictionary == null)
                return null;

            //词典必须有内容
            if (dictionary.Count == 0)
                return null;

            //在字典中查找指定中文的拼音，找不到就返回null
            if (dictionary.ContainsKey(cn))
                return dictionary[cn];
            else
                return null;
        }

        /// <summary>
        /// 根据中文词汇，通过查字典，得到拆分后，每个中文单字对应的拼音
        /// </summary>
        /// <param name="cn"></param>
        /// <returns>每个单字对应拼音，组成的字典</returns>
        public Dictionary<char, string> GetCnCharPinyin(string cn)
        {
            //中文不能为空
            if (string.IsNullOrEmpty(cn))
                return null;

            //词典不能为空
            if (dictionary == null)
                return null;

            //词典必须有内容
            if (dictionary.Count == 0)
                return null;

            //如果在字典中不存在指定词条，就返回null
            if (!dictionary.ContainsKey(cn))
                return null;

            //定义保存中文单字和对应拼音的词典
            Dictionary<char, string> cnCharPinyin = new Dictionary<char, string>();


            //把词汇对应的拼音，拆分成单个中文字的拼音
            string[] pinyins = dictionary[cn].Split(' ');

            //把中文词汇拆分成单字
            char[] cnChars = cn.ToCharArray();

            //将单字和拼音对应起来，添加到单字拼音字典
            for (int i = 0; i < cn.Length; i++)
            {
                cnCharPinyin.Add(cnChars[i], pinyins[i]);
            }

            //返回这个词汇的单字拼音字典
            return cnCharPinyin;
        }



    }
}
