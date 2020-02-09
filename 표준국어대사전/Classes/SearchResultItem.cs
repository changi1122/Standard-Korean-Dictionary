using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 표준국어대사전.Classes
{
    public class SearchResultItem
    {
        //코드 번호
        public int target_code { get; set; }
        //단어 명
        public string word { get; set; }
        //어깨번호
        public int sup_no { get; set; }
        //표시 어깨번호
        //검색결과가 한 개일 경우, 특수한(더보기 기능) SearchResultItem의 경우 표시 안함.
        public string display_sup_no { get; set; }
        //간단 뜻풀이
        public string definition { get; set; }
    }

    public class SearchResultItemSample
    {
        public static List<SearchResultItem> GetWords()
        {
            var SearchResults = new List<SearchResultItem>();

            SearchResults.Add(new SearchResultItem { target_code = 495790, word = "한", sup_no = 1, display_sup_no = "1", definition = "111" });
            SearchResults.Add(new SearchResultItem { target_code = 361883, word = "한", sup_no = 2, display_sup_no = "2", definition = "222" });
            SearchResults.Add(new SearchResultItem { target_code = 361884, word = "한", sup_no = 3, display_sup_no = "3", definition = "333" });
            SearchResults.Add(new SearchResultItem { target_code = 361885, word = "한", sup_no = 4, display_sup_no = "4", definition = "444" });
            SearchResults.Add(new SearchResultItem { target_code = 361886, word = "한", sup_no = 5, display_sup_no = "5", definition = "555" });

            return SearchResults;
        }
    }
}
