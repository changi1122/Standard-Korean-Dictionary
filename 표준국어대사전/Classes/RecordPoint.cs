using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 표준국어대사전.Classes
{
    public enum RecordType
    {
        Null,
        ClearSearchResult,  //검색 결과 삭제시
        ClearDefinition,    //뜻풀이 삭제시
        ClearAll            //모두 삭제시
    }

    class RecordPoint
    {
        //유형
        public RecordType Type;

        //검색어, 없는 경우 빈 문자열
        public string SearchText;

        //검색 결과, 삭제되지 않은 경우 null
        public ObservableCollection<SearchResultItem> SearchResults;

        //뜻풀이, 삭제되지 않은 경우 null
        public WordDetailItem Definition;

        //검색 결과 리스트뷰 선택된 인덱스, 값이 의미 없는 경우 -1
        public int SelectedIndex;

        
        public RecordPoint(RecordType type, string searchText = "",
                            ObservableCollection<SearchResultItem> searchResults = null, WordDetailItem definition = null,
                            int selectedIndex = -1)
        {
            this.Type = type;
            this.SearchText = searchText;
            if (searchResults == null)
            {
                this.SearchResults = null;
            }
            else
            {
                this.SearchResults = new ObservableCollection<SearchResultItem>();
                for (int i = 0; i < searchResults.Count; i++)
                {
                    this.SearchResults.Add(searchResults[i]);
                }
            }
            this.Definition = definition;
            this.SelectedIndex = selectedIndex;
        }
    }
}
