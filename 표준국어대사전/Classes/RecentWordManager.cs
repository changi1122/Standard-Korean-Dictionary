using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Spi;
using Windows.Media.Audio;
using 표준국어대사전.Classes;

namespace 표준국어대사전.Classes
{
    public static class RecentWordManager
    {
        const int WORD_LIMIT = 10;

        static List<string> RecentWords;

        public static void Append(string word)
        {
            if (RecentWords == null)
                LoadRecentWords();

            if (!RecentWords.Contains(word))
                RecentWords.Add(word);
            
            if (RecentWords.Count > WORD_LIMIT)
                RecentWords.RemoveAt(0);
            SaveRecentWords();
        }

        public static void Clear()
        {
            if (RecentWords == null)
                LoadRecentWords();

            RecentWords.Clear();
            SaveRecentWords();
        }

        public static List<string> GetWords()
        {
            if (RecentWords == null)
                LoadRecentWords();

            return RecentWords;
        }

        private static void LoadRecentWords()
        {
            RecentWords = new List<string>();

            string value = StorageManager.GetSetting<string>(StorageManager.RecentWord);
            if (value == null)
                value = "";

            string[] splitValue = value.Split(',');

            for (int i = 0; i < splitValue.Length; i++)
            {
                if (splitValue[i] != "")
                    RecentWords.Add(splitValue[i]);
            }
        }

        private static void SaveRecentWords()
        {
            string value = "";
            for (int i = 0; i < RecentWords.Count; i++)
            {
                value += RecentWords[i] + ',';
            }
            StorageManager.SetSetting<string>(StorageManager.RecentWord, value);
        }
    }
}
