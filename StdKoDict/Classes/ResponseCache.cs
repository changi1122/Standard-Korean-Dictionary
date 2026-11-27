using System;
using System.Collections.Generic;

namespace 표준국어대사전.Classes
{
    /// <summary>
    /// 사전 API 응답(XML 문자열)을 잠시 메모리에 보관해 같은 요청을 다시 보내지 않도록 한다.
    /// </summary>
    /// <remarks>
    /// - 요청 URL 전체를 키로 쓴다. URL에 API 키·검색어·페이지가 모두 들어 있어 설정에서 키를 바꿔도 섞이지 않는다.
    /// - 파싱 결과(WordDetailItem)는 화면 요소와 상태를 담고 있어 재사용하면 안 되므로, 원본 XML 문자열만 저장한다.
    /// - 네트워크 실패(null)와 API 오류 응답은 저장하지 않는다. 문제가 해결된 뒤에도 오류가 계속 보이지 않게 하기 위함.
    /// - 앱을 끄면 사라지는 메모리 캐시다.
    /// </remarks>
    internal static class ResponseCache
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);
        private const int Capacity = 100;

        private sealed class Entry
        {
            public string Body;
            public DateTime StoredAt;
            public LinkedListNode<string> Node;
        }

        private static readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>();
        private static readonly LinkedList<string> order = new LinkedList<string>(); // 앞쪽이 가장 오래된 항목
        private static readonly object gate = new object();

        public static bool TryGet(string url, out string body)
        {
            lock (gate)
            {
                if (entries.TryGetValue(url, out Entry entry))
                {
                    if (DateTime.UtcNow - entry.StoredAt < Lifetime)
                    {
                        body = entry.Body;
                        return true;
                    }
                    Remove(url, entry); // 만료
                }
            }
            body = null;
            return false;
        }

        public static void Add(string url, string body)
        {
            if (string.IsNullOrEmpty(body) || IsErrorResponse(body))
                return;

            lock (gate)
            {
                if (entries.TryGetValue(url, out Entry existing))
                    Remove(url, existing);

                entries[url] = new Entry { Body = body, StoredAt = DateTime.UtcNow, Node = order.AddLast(url) };

                // 최대 개수를 넘으면 가장 오래 저장된 것부터 지운다
                while (entries.Count > Capacity)
                {
                    string oldest = order.First.Value;
                    Remove(oldest, entries[oldest]);
                }
            }
        }

        private static void Remove(string url, Entry entry)
        {
            order.Remove(entry.Node);
            entries.Remove(url);
        }

        // API 오류 응답: <error><error_code>…</error_code><message>…</message></error>
        private static bool IsErrorResponse(string body)
        {
            return body.Contains("<error_code>");
        }
    }
}
