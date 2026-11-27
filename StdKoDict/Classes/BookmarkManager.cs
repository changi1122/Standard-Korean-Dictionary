using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace 표준국어대사전.Classes
{
    /// <summary>단어장에 저장한 단어 하나</summary>
    public sealed class BookmarkItem
    {
        public string TargetCode;   // 단어 코드 (상세 조회용)
        public string Word;         // 표제어 (예: 사과-나무)
        public int SupNo;           // 어깨번호, 없으면 0
        public string Pos;          // 품사
        public string Origin;       // 원어
        public string Definition;   // 대표 뜻풀이 (목록 툴팁용)
        public DateTime AddedAt;
    }

    /// <summary>
    /// 단어장 저장소. 앱 데이터 폴더의 텍스트 파일에 한 줄에 한 단어씩 탭으로 구분해 저장한다.
    /// </summary>
    /// <remarks>
    /// - LocalSettings는 값 하나에 8KB 제한이 있어 단어가 많아지면 저장에 실패하므로 파일을 쓴다.
    /// - JSON 대신 텍스트 형식이라 Native AOT에서 직렬화 설정이 필요 없다.
    /// - 설정 초기화(LocalSettings 비우기)와는 별개로 유지된다.
    /// </remarks>
    public static class BookmarkManager
    {
        private const string FileName = "bookmarks.tsv";
        private static List<BookmarkItem> items;

        private static string FilePath =>
            Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, FileName);

        /// <summary>저장한 단어 목록 (최근에 추가한 순)</summary>
        public static IReadOnlyList<BookmarkItem> GetAll()
        {
            EnsureLoaded();
            return items.OrderByDescending(item => item.AddedAt).ToList();
        }

        public static bool Contains(string targetCode)
        {
            EnsureLoaded();
            return !string.IsNullOrEmpty(targetCode) && items.Any(item => item.TargetCode == targetCode);
        }

        public static void Add(BookmarkItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.TargetCode))
                return;

            EnsureLoaded();
            items.RemoveAll(existing => existing.TargetCode == item.TargetCode);
            item.AddedAt = DateTime.Now;
            items.Add(item);
            Save();
        }

        public static void Remove(string targetCode)
        {
            EnsureLoaded();
            if (items.RemoveAll(item => item.TargetCode == targetCode) > 0)
                Save();
        }

        private static void EnsureLoaded()
        {
            if (items != null)
                return;

            items = new List<BookmarkItem>();
            try
            {
                if (!File.Exists(FilePath))
                    return;

                foreach (string line in File.ReadAllLines(FilePath, Encoding.UTF8))
                {
                    string[] fields = line.Split('\t');
                    if (fields.Length < 7 || string.IsNullOrEmpty(fields[0]))
                        continue;

                    int.TryParse(fields[2], out int supNo);
                    long.TryParse(fields[6], out long ticks);
                    items.Add(new BookmarkItem
                    {
                        TargetCode = fields[0],
                        Word = fields[1],
                        SupNo = supNo,
                        Pos = fields[3],
                        Origin = fields[4],
                        Definition = fields[5],
                        AddedAt = new DateTime(ticks)
                    });
                }
            }
            catch
            {
                // 파일이 손상되었으면 읽은 데까지만 사용
            }
        }

        private static void Save()
        {
            try
            {
                IEnumerable<string> lines = items.Select(item => string.Join("\t",
                    Clean(item.TargetCode), Clean(item.Word), item.SupNo.ToString(), Clean(item.Pos),
                    Clean(item.Origin), Clean(item.Definition), item.AddedAt.Ticks.ToString()));

                // 임시 파일에 쓴 뒤 바꿔치기해서, 쓰는 도중 앱이 꺼져도 기존 단어장이 깨지지 않게 한다
                string tempPath = FilePath + ".tmp";
                File.WriteAllLines(tempPath, lines, new UTF8Encoding(false));
                if (File.Exists(FilePath))
                    File.Replace(tempPath, FilePath, null);
                else
                    File.Move(tempPath, FilePath);
            }
            catch
            {
                // 저장 실패는 다음 저장 때 다시 시도된다
            }
        }

        // 구분자(탭)와 줄바꿈이 값에 섞이지 않도록
        private static string Clean(string value)
        {
            return (value ?? "").Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
        }
    }
}
