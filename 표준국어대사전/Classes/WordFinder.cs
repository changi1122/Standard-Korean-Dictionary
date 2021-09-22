using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace 표준국어대사전.Classes
{
    class WordFinder
    {
        string API_KEY;
        const string WORD_SEARCH_URL = "https://stdict.korean.go.kr/api/search.do?&key={0}&type_search=search&method={1}&part=all&start={2}&num={3}&q={4}";

        ObservableCollection<SearchResultItem> SearchResults;
        ProgressBar MasterProgressBar;
        TextBlock TextBlockErrorMessage;

        public WordFinder(ObservableCollection<SearchResultItem> searchResultItems, ProgressBar pBar, TextBlock textBlock)
        {
            //생성자
            SearchResults = searchResultItems;
            MasterProgressBar = pBar;
            TextBlockErrorMessage = textBlock;

            //API 키 처리
            API_KEY = StorageManager.GetSetting<string>(StorageManager.APIKey);
        }

        /// <summary>
        /// 단어 검색 결과를 받아오고, SearchResults 리스트에 넣는다.
        /// </summary>
        /// <returns>남은 검색 결과 존재 여부(더 보기 버튼 표시 여부)</returns>
        public async void GetSearchResults(int start, int num, string searchText, Visibility isMoreButtonVisible, Action<Visibility> setMoreButtonVisibility)
        {
            setMoreButtonVisibility(Visibility.Collapsed);
            MasterProgressBar.Visibility = Visibility.Visible;

            string responseBody = await DownloadSearchResultsAsync(start, num, searchText);
            if (responseBody == null) //실패 여부 확인
            {
                string error_code = "404";
                string error_message = $"error_code : {error_code}" + Environment.NewLine + "message : Network Problem";
                ShowErrorMessage(error_code, error_message, null);
                MasterProgressBar.Visibility = Visibility.Collapsed;
                setMoreButtonVisibility(isMoreButtonVisible);
                return;
            }

            ParseAndShowSearchResults(responseBody, start, num, searchText, setMoreButtonVisibility);

            MasterProgressBar.Visibility = Visibility.Collapsed;
        }

        private async Task<string> DownloadSearchResultsAsync(int start, int num, string searchText)
        {
            #region VAR
            //Search_Text
            string method = "exact";
            //start;
            //num
            /*string advanced = "n";
            int target = 1;
            string type1 = "all";
            string type2 = "all";
            int pos = 0;
            int cat = 0;
            int multimedia = 0;
            int letter_s = 1;
            int letter_e = 1;*/
            #endregion

            string url = string.Format(WORD_SEARCH_URL, API_KEY, method, start, num, searchText);

            HttpClient client = new HttpClient();

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch
            {
                //GetAsync 실패
                return null;
            }
        }

        private void ParseAndShowSearchResults(string responseBody, int start, int num, string searchText, Action<Visibility> setMoreButtonVisibility)
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

                return;
            }

            int total = (int)xDoc.Descendants("total").ElementAt(0);
            if (total == 0)
            {
                var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                TextBlockErrorMessage.Text = res.GetString("ErrorMessageNoResult");
                TextBlockErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            var words = from SearchResultItem in xDoc.Descendants("item")
                        select new SearchResultItem
                        {
                            target_code = (int)SearchResultItem.Element("target_code"),
                            word = (string)SearchResultItem.Element("word"),
                            sup_no = (int)SearchResultItem.Element("sup_no"),
                            display_sup_no = ((int)SearchResultItem.Element("sup_no")).ToString(),
                            definition = (string)SearchResultItem.Element("sense").Element("definition")
                        };

            List<SearchResultItem> wordlist = words.ToList();
            for (int i = 0; i < wordlist.Count(); ++i)
            {
                wordlist[i].display_sup_no = (wordlist[i].sup_no != 0) ? ToSup(wordlist[i].sup_no) : "";
                SearchResults.Add(new ObservableCollection<SearchResultItem>(wordlist)[i]);
            }

            if (start * 10 < total)
            {
                setMoreButtonVisibility(Visibility.Visible);
            }
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
