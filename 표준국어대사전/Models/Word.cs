using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 표준국어대사전.Models
{
    public class Word
    {
        public string Javascript
        {
            get;
            set;
        }
        public string WordTitle
        {
            get;
            set;
        }

        public string WordPronounce
        {
            get;
            set;
        }
        public string WordDefinition
        {
            get;
            set;
        }
    }

    public class WordManager
    {
        public static List<Word> GetWords()
        {
            var words = new List<Word>();

            words.Add(new Word { WordTitle = "예시01(例示)", Javascript = "javascript:fncGoPage('/search/View.jsp','458683','1','');", WordPronounce = "", WordDefinition = ""});
            words.Add(new Word { WordTitle = "예시02(例時)", Javascript = "javascript:fncGoPage('/search/View.jsp','240740','1','');", WordPronounce = "", WordDefinition = "" });
            words.Add(new Word { WordTitle = "예시03(睨視)", Javascript = "javascript:fncGoPage('/search/View.jsp','240741','1','');", WordPronounce = "", WordDefinition = "" });
            words.Add(new Word { WordTitle = "예시04(豫示)", Javascript = "javascript:fncGoPage('/search/View.jsp','458685','1','');", WordPronounce = "", WordDefinition = "" });
            words.Add(new Word { WordTitle = "예시05(豫試)", Javascript = "javascript:fncGoPage('/search/View.jsp','240746','1','');", WordPronounce = "", WordDefinition = "" });

            return words;
        }
    }
}
