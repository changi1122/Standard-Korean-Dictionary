﻿using System;
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
    class SearchClass
    {
        string API_KEY;
        const string WORD_SEARCH_URL = "https://stdict.korean.go.kr/api/search.do?&key={0}&type_search=search&method={1}&part=all&start={2}&num={3}&q={4}";

        ListView ListviewSearchResult;
        ObservableCollection<SearchResultItem> SearchResults;
        ProgressBar MasterProgressBar;
        TextBlock TextBlockErrorMessage;

        public SearchClass(ListView listView, ObservableCollection<SearchResultItem> searchResultItems, ProgressBar pBar, TextBlock textBlock)
        {
            //생성자
            ListviewSearchResult = listView;
            SearchResults = searchResultItems;
            MasterProgressBar = pBar;
            TextBlockErrorMessage = textBlock;

            //API 키 처리
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            API_KEY = (string)localSettings.Values["#APIKey"];
        }

        public async void GetSearchResults(int start, int num, string Search_Text)
        {
            MasterProgressBar.Visibility = Visibility.Visible;

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

            //string temp = string.Format(WORD_SEARCH_URL, API_KEY, method, start, num, advanced, target, type1, type2, pos, cat, multimedia, letter_s, letter_e, Search_Text);
            string temp = string.Format(WORD_SEARCH_URL, API_KEY, method, start, num, Search_Text);


            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(temp);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

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
                var contentDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = error_message,
                    CloseButtonText = res.GetString("ContentDialogText2")
                };
                if (error_code == "020" || error_code == "021")
                    contentDialog.PrimaryButtonText = res.GetString("ContentDialogText1");

                ContentDialogResult result = await contentDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    if (error_code == "020")
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("https://costudio1122.blogspot.com/p/2.html"));
                    if (error_code == "021")
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("https://costudio1122.blogspot.com/p/api.html"));
                }

                MasterProgressBar.Visibility = Visibility.Collapsed;
                return;
            }

            int total = (int)xDoc.Descendants("total").ElementAt(0);
            if (total == 0)
            {
                var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                TextBlockErrorMessage.Text = res.GetString("ErrorMessageNoResult");
                TextBlockErrorMessage.Visibility = Visibility.Visible;
                MasterProgressBar.Visibility = Visibility.Collapsed;
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
                if (wordlist[i].sup_no == 0)
                    wordlist[i].display_sup_no = "";
                SearchResults.Add(new ObservableCollection<SearchResultItem>(wordlist)[i]);
            }

            if (start * 10 < total)
            {
                SearchResults.Add(new SearchResultItem { target_code = -321, word = "[더 보기]", sup_no = start, display_sup_no = "", definition = Search_Text });
            }

            MasterProgressBar.Visibility = Visibility.Collapsed;
        }

    }
}
