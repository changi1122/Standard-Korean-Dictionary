using System;

namespace 표준국어대사전.Models
{
    /// <summary>검색 방식 (오픈 API method 값)</summary>
    public enum SearchMethod
    {
        Exact,      // exact: 일치
        Include,    // include: 포함
        Start,      // start: 시작
        End         // end: 끝
    }

    /// <summary>찾을 대상 (오픈 API target 값)</summary>
    public enum SearchTarget
    {
        Headword = 1,   // 표제어 (관용구·속담 포함)
        Definition = 8  // 뜻풀이
    }

    /// <summary>
    /// 검색어와 검색 옵션을 함께 묶은 요청.
    /// </summary>
    /// <remarks>
    /// 뒤로/앞으로 기록(HistoryManager)은 마지막 검색어를 문자열로 저장하므로,
    /// Encode/Decode로 옵션까지 한 문자열에 담아 기록한다. 옵션이 없는 예전 형식은 기본값(표제어·일치)으로 읽는다.
    /// </remarks>
    public sealed class SearchRequest
    {
        private const char Separator = '\u001F'; // 검색어에 쓰이지 않는 제어 문자

        public string Query { get; }
        public SearchMethod Method { get; }
        public SearchTarget Target { get; }

        public SearchRequest(string query, SearchMethod method = SearchMethod.Exact, SearchTarget target = SearchTarget.Headword)
        {
            Query = query ?? "";
            Target = target;
            // 뜻풀이는 문장 일부로 찾는 것이므로 항상 포함 검색
            Method = (target == SearchTarget.Definition) ? SearchMethod.Include : method;
        }

        /// <summary>기본 검색(표제어·일치)인지</summary>
        public bool IsDefault => Method == SearchMethod.Exact && Target == SearchTarget.Headword;

        public string ApiMethod
        {
            get
            {
                switch (Method)
                {
                    case SearchMethod.Include: return "include";
                    case SearchMethod.Start: return "start";
                    case SearchMethod.End: return "end";
                    default: return "exact";
                }
            }
        }

        public int ApiTarget => (int)Target;

        public SearchRequest WithMethod(SearchMethod method) => new SearchRequest(Query, method, Target);

        public string Encode() => $"{(int)Method}{Separator}{(int)Target}{Separator}{Query}";

        public static SearchRequest Decode(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return new SearchRequest("");

            string[] parts = encoded.Split(new[] { Separator }, 3);
            if (parts.Length == 3 && int.TryParse(parts[0], out int method) && int.TryParse(parts[1], out int target) &&
                Enum.IsDefined((SearchMethod)method) && Enum.IsDefined((SearchTarget)target))
                return new SearchRequest(parts[2], (SearchMethod)method, (SearchTarget)target);

            return new SearchRequest(encoded); // 예전 형식: 검색어만 있음
        }
    }
}
