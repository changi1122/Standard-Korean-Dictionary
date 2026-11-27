using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using 표준국어대사전.Models;

namespace 표준국어대사전.Classes
{
    class WordFinder
    {
        string API_KEY;
        // advanced=y여야 method·target 옵션이 적용되고 관용구·속담도 함께 검색된다 (오픈 API 안내 기준)
        const string WORD_SEARCH_URL = "https://kdict.korean.go.kr/standard/api/search.do?key={0}&advanced=y&method={1}&target={2}&start={3}&num={4}&q={5}";

        ObservableCollection<SearchResultItem> SearchResults;
        Action<Visibility> SetProgressBar;
        Action<string> SetErrorMessageText;

        public WordFinder(ObservableCollection<SearchResultItem> searchResultItems, Action<Visibility> setProgressBar, Action<string> setErrorMessageText)
        {
            //생성자
            this.SearchResults = searchResultItems;
            this.SetProgressBar = setProgressBar;
            this.SetErrorMessageText = setErrorMessageText;

            //API 키 처리
            API_KEY = StorageManager.GetSetting<string>(StorageManager.APIKey);
        }

        /// <summary>
        /// 단어 검색 결과를 받아오고, SearchResults 리스트에 넣는다.
        /// </summary>
        /// <param name="start">페이지 번호 (1부터)</param>
        /// <param name="reportNoResult">결과가 없을 때 '검색 결과 없음'을 표시할지 (자동 포함 검색 전에는 끈다)</param>
        /// <returns>전체 결과 수. 네트워크·API 오류면 -1</returns>
        public async Task<int> GetSearchResults(int start, int num, SearchRequest request, Visibility isMoreButtonVisible,
                                                Action<Visibility> setMoreButtonVisibility, bool reportNoResult = true)
        {
            setMoreButtonVisibility(Visibility.Collapsed);
            SetProgressBar(Visibility.Visible);

            string responseBody = await DownloadSearchResultsAsync(start, num, request);
            if (responseBody == null) //실패 여부 확인
            {
                string error_code = "404";
                string error_message = $"error_code : {error_code}" + Environment.NewLine + "message : Network Problem";
                ShowErrorMessage(error_code, error_message, null);
                SetProgressBar(Visibility.Collapsed);
                setMoreButtonVisibility(isMoreButtonVisible);
                return -1;
            }

            int total = ParseAndShowSearchResults(responseBody, start, num, reportNoResult, setMoreButtonVisibility);

            SetProgressBar(Visibility.Collapsed);
            return total;
        }

        private async Task<string> DownloadSearchResultsAsync(int start, int num, SearchRequest request)
        {
            // 사전 표기대로 어미·접사를 붙임표와 함께 입력해도(예: -ㅂ니다, 제-) 검색되도록
            // API는 붙임표 없는 검색어만 찾으므로 앞뒤 붙임표를 제거한다.
            string searchText = request.Query;
            string query = searchText?.Trim().Trim('-').Trim() ?? "";
            if (query.Length == 0)
                query = searchText ?? "";

            // 뜻풀이 검색은 띄어쓰기·기호가 들어가므로 검색어를 URL 인코딩한다
            string url = string.Format(WORD_SEARCH_URL, API_KEY, request.ApiMethod, request.ApiTarget, start, num,
                                       Uri.EscapeDataString(query));

            // 최근 10분 안에 같은 요청을 했으면 저장된 응답을 그대로 쓴다
            if (ResponseCache.TryGet(url, out string cachedBody))
                return cachedBody;

            HttpClient client = new HttpClient();

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                ResponseCache.Add(url, responseBody);
                return responseBody;
            }
            catch
            {
                //GetAsync 실패
                return null;
            }
        }

        // 반환값: 전체 결과 수, API 오류면 -1
        private int ParseAndShowSearchResults(string responseBody, int start, int num, bool reportNoResult, Action<Visibility> setMoreButtonVisibility)
        {
            XDocument xDoc = XDocument.Parse(responseBody);

            //검색 결과 Listview 지우기
            if (start == 1)
                SearchResults.Clear();

            //에러코드
            if (xDoc.Element("error") != null)
            {
                string error_code = (string)xDoc.Element("error").Descendants("error_code").ElementAt(0);
                string error_message = "error_code : " + error_code + Environment.NewLine + "message : " + (string)xDoc.Element("error").Descendants("message").ElementAt(0);

                var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                ShowErrorMessage(error_code, error_message, res.GetString("ContentDialogText1"));

                return -1;
            }

            int total = (int)xDoc.Descendants("total").ElementAt(0);
            if (total == 0)
            {
                if (reportNoResult)
                {
                    var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                    SetErrorMessageText(res.GetString("ErrorMessageNoResult"));
                }
                return 0;
            }

            var words = from SearchResultItem in xDoc.Descendants("item")
                        select new SearchResultItem
                        {
                            target_code = (int)SearchResultItem.Element("target_code"),
                            word = (string)SearchResultItem.Element("word"),
                            sup_no = (int)SearchResultItem.Element("sup_no"),
                            display_sup_no = ((int)SearchResultItem.Element("sup_no")).ToString(),
                            definition = (string)SearchResultItem.Element("sense").Element("definition"),
                            origin = ((string)SearchResultItem.Element("origin"))?.Trim(),
                            pos = ((string)SearchResultItem.Element("pos"))?.Trim()
                        };

            List<SearchResultItem> wordlist = words.ToList();
            for (int i = 0; i < wordlist.Count(); ++i)
            {
                wordlist[i].display_sup_no = (wordlist[i].sup_no != 0) ? ToSup(wordlist[i].sup_no) : "";
                SearchResults.Add(new ObservableCollection<SearchResultItem>(wordlist)[i]);
            }

            if (start * num < total)
            {
                setMoreButtonVisibility(Visibility.Visible);
            }
            return total;
        }

        private async void ShowErrorMessage(string error_code, string error_message, string PrimaryButtonText)
        {
            #region URL_LIST
            const string URLCODE020 = "https://costudio1122.blogspot.com/p/2.html";
            const string URLCODE021 = "https://costudio1122.blogspot.com/p/api.html";
            #endregion

            var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            var contentDialog = new ContentDialog
            {
                Title = "Error",
                Content = error_message,
                CloseButtonText = res.GetString("ContentDialogText2")
            };
            if (PrimaryButtonText != null)
                contentDialog.PrimaryButtonText = PrimaryButtonText;

            ContentDialogResult result = await contentDialog.ShowAsync();

            //도움말 클릭시 웹페이지 열기
            if (PrimaryButtonText != null && result == ContentDialogResult.Primary)
            {
                if (error_code == "020")
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(URLCODE020));
                else if (error_code == "021")
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(URLCODE021));
            }
        }

        // 위 첨자로 변환
        private string ToSup(int number)
        {
            StringBuilder numString = new StringBuilder(number.ToString());
            for (int i = 0; i < numString.Length; i++)
            {
                switch (numString[i])
                {
                    case '1':
                        numString[i] = '¹';
                        break;
                    case '2':
                        numString[i] = '²';
                        break;
                    case '3':
                        numString[i] = '³';
                        break;
                    case '4':
                        numString[i] = '⁴';
                        break;
                    case '5':
                        numString[i] = '⁵';
                        break;
                    case '6':
                        numString[i] = '⁶';
                        break;
                    case '7':
                        numString[i] = '⁷';
                        break;
                    case '8':
                        numString[i] = '⁸';
                        break;
                    case '9':
                        numString[i] = '⁹';
                        break;
                    case '0':
                        numString[i] = '⁰';
                        break;
                    default:
                        break;
                }
            }
            return numString.ToString();
        }

    }
}
