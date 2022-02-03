﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Resources;
using 표준국어대사전.Classes;


namespace 표준국어대사전.ViewModels
{
    internal class SearchViewModel : INotifyPropertyChanged
    {
        private HistoryManager History;

        private string lastSearchedQuery;

        public SearchViewModel(ref bool IsFirstPageOpen)
        {
            this.History = new HistoryManager();
            this.Query = "";
            this.IsSearchBoxEnabled = true;
            this.IsMoreButtonVisible = Visibility.Collapsed;
            this.IsErrorMessageVisible = Visibility.Collapsed;
            this.IsDefinitionViewerVisible = Visibility.Collapsed;
            this.IsDetailGridVisible = Visibility.Visible;

            if (IsFirstPageOpen)
            {
                this.SearchResults = new ObservableCollection<SearchResultItem>(SearchResultStaticPage.GetHomeTab());
                this.Definitions = WordDetailStaticPage.GetHomepage();
                this.IsDefinitionViewerVisible = Visibility.Visible;
                this.SearchResultSelectedIndex = 0;
                IsFirstPageOpen = false;
            }
            else
            {
                this.SearchResults = new ObservableCollection<SearchResultItem>();
                this.Definitions = new WordDetailItem();
                this.SearchResultSelectedIndex = -1;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // 검색 결과
        public ObservableCollection<SearchResultItem> SearchResults { get; private set; }
        
        // 뜻 풀이
        public WordDetailItem Definitions { get; private set; }

        public bool CanGoBack { get { return History.CanGoBack; } }
        public bool CanGoForward { get { return History.CanGoForward; } }
        public string Query { get; set; }
        public int SearchResultSelectedIndex { get; set; }
        public Visibility IsMoreButtonVisible { get; private set; }
        public bool IsSearchBoxEnabled { get; private set; }
        public Visibility IsErrorMessageVisible { get; private set; }
        public string ErrorMessageText { get; private set; }
        public Visibility IsDefinitionViewerVisible { get; private set; }

        // TO-DO
        // BasicGrid.ActualWidth < 686 일때만 사용
        public Visibility IsDetailGridVisible { get; private set; }
        public bool IsTitleBarBackButtonEnabled { get; private set; }

        public void SearchWords()
        {
            IsErrorMessageVisible = Visibility.Collapsed;
            RaisePropertyChanged("IsErrorMessageVisible");

            if (NetworkCheck() == false)
                return;

            // 되돌리기 위한 기록
            History.RecordAll(Query, lastSearchedQuery, SearchResults, Definitions, SearchResultSelectedIndex, false, IsMoreButtonVisible);

            // 검색어 숫자 지우기
            string query = Regex.Replace(Query, @"/[1-9]/g", "");
                
            // 검색 결과 지우기
            SearchResults.Clear();
            // 뜻풀이 감추기
            IsDefinitionViewerVisible = Visibility.Collapsed;
            RaisePropertyChanged("SearchResultSelectedIndex", "IsDefinitionViewerVisible");

            lastSearchedQuery = query;
            // TO-DO
            WordFinder wordFinder = new WordFinder(SearchResults, new Windows.UI.Xaml.Controls.ProgressBar(), new Windows.UI.Xaml.Controls.TextBlock());
            wordFinder.GetSearchResults(1, 10, query, IsMoreButtonVisible, (visibility) => {
                IsMoreButtonVisible = visibility;
                RaisePropertyChanged("IsMoreButtonVisible");
            });

            RaisePropertyChanged("CanGoBack", "CanGoForward");
            //최근 검색어 기록
            RecentWordManager.Append(Query);
        }

        public void SearchMore()
        {
            int start = SearchResults.Last().sup_no / 10 + 1;

            // TO-DO
            WordFinder wordFinder = new WordFinder(SearchResults, new Windows.UI.Xaml.Controls.ProgressBar(), new Windows.UI.Xaml.Controls.TextBlock());
            wordFinder.GetSearchResults(start, 10, lastSearchedQuery, IsMoreButtonVisible, (visibility) => {
                IsMoreButtonVisible = visibility;
                RaisePropertyChanged("IsMoreButtonVisible");
            });
        }

        public async void DisplayWordDetail(SearchResultItem clickedItem)
        {
            if (NetworkCheck() == false)
                return;

            if (clickedItem.target_code == -200)
            {
                //시작 누를 시 동작
                Definitions = WordDetailStaticPage.GetHomepage();
                IsDetailGridVisible = Visibility.Visible;
                IsTitleBarBackButtonEnabled = true;
                RaisePropertyChanged("Definitions", "IsDetailGridVisible", "IsTitleBarBackButtonEnabled");
                return;
            }

            //일반 단어 클릭시 동작

            //되돌리기 위한 기록
            History.RecordDefinition(Query, lastSearchedQuery, Definitions, SearchResultSelectedIndex, false, IsMoreButtonVisible);
            //뜻풀이 감추기
            IsDefinitionViewerVisible = Visibility.Collapsed;

            IsDetailGridVisible = Visibility.Visible;
            IsTitleBarBackButtonEnabled = true;
            RaisePropertyChanged("IsDefinitionViewerVisible", "IsDetailGridVisible", "IsTitleBarBackButtonEnabled");

            // TO-DO
            DefinitionParser definitionParser = new DefinitionParser(new Windows.UI.Xaml.Controls.ProgressBar());
            WordDetailItem definitionItem = await definitionParser.GetWordDetail(clickedItem.target_code.ToString(), clickedItem.word, clickedItem.sup_no);
            if (definitionItem != null)
            {
                Definitions = definitionItem;
            }
            IsDefinitionViewerVisible = Visibility.Visible;

            RaisePropertyChanged("Definitions", "IsDefinitionViewerVisible", "CanGoBack", "CanGoForward");
        }

        public void Undo()
        {
            string query = Query;
            var searchResults = SearchResults;
            WordDetailItem definition = Definitions;
            int selectedIndex = SearchResultSelectedIndex;
            Visibility isMoreButtonVisible = IsMoreButtonVisible;
            History.Undo(ref query, ref lastSearchedQuery, ref searchResults, ref definition, ref selectedIndex, ref isMoreButtonVisible);

            Query = query;
            SearchResults = searchResults;
            if (definition != null)
                Definitions = definition;
            SearchResultSelectedIndex = selectedIndex;
            if (selectedIndex < 0)
                IsDefinitionViewerVisible = Visibility.Collapsed;
            else
                IsDefinitionViewerVisible = Visibility.Visible;
            IsMoreButtonVisible = isMoreButtonVisible;
            RaisePropertyChanged("Query", "Definitions", "SearchResultSelectedIndex", "IsDefinitionViewerVisible",
                                 "IsMoreButtonVisible", "CanGoBack", "CanGoForward");
        }

        public void Redo()
        {
            string query = Query;
            var searchResults = SearchResults;
            WordDetailItem definition = Definitions;
            int selectedIndex = SearchResultSelectedIndex;
            Visibility isMoreButtonVisible = IsMoreButtonVisible;
            History.Redo(ref query, ref lastSearchedQuery, ref searchResults, ref definition, ref selectedIndex, ref isMoreButtonVisible);

            Query = query;
            SearchResults = searchResults;
            if (definition != null)
                Definitions = definition;
            SearchResultSelectedIndex = selectedIndex;
            if (selectedIndex < 0)
                IsDefinitionViewerVisible = Visibility.Collapsed;
            else
                IsDefinitionViewerVisible = Visibility.Visible;
            IsMoreButtonVisible = isMoreButtonVisible;
            RaisePropertyChanged("Query", "Definitions", "SearchResultSelectedIndex", "IsDefinitionViewerVisible",
                                 "IsMoreButtonVisible", "CanGoBack", "CanGoForward");
        }

        public void GoHome()
        {
            // 되돌리기 위한 기록
            History.RecordAll(Query, lastSearchedQuery, SearchResults, Definitions, SearchResultSelectedIndex, false, IsMoreButtonVisible);
            IsDefinitionViewerVisible = Visibility.Collapsed;
            RaisePropertyChanged("IsDefinitionViewerVisible", "CanGoBack", "CanGoForward");

            // 검색어 지우기
            Query = "";
            // 검색 결과 지우기
            SearchResults.Clear();

            List<SearchResultItem> homeTab = SearchResultStaticPage.GetHomeTab();
            foreach (SearchResultItem item in homeTab)
            {
                SearchResults.Add(item);
            }
            SearchResultSelectedIndex = 0;
            Definitions = WordDetailStaticPage.GetHomepage();
            IsDefinitionViewerVisible = Visibility.Visible;
            RaisePropertyChanged("Query", "SearchResultSelectedIndex", "Definitions", "IsDefinitionViewerVisible");
        }

        public void NetStatusRefresh()
        {
            if (NetworkCheck() == true)
            {
                IsSearchBoxEnabled = true;
                IsErrorMessageVisible = Visibility.Collapsed;
                RaisePropertyChanged("IsSearchBoxEnabled", "IsErrorMessageVisible");
            }
        }


        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        protected void RaisePropertyChanged(params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
                RaisePropertyChanged(names[i]);
        }

        private static bool IsInternetConnected()
        {
            Windows.Networking.Connectivity.ConnectionProfile connections = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            bool internet = (connections != null) &&
                (connections.GetNetworkConnectivityLevel() == Windows.Networking.Connectivity.NetworkConnectivityLevel.InternetAccess);
            return internet;
        }
        private bool NetworkCheck()
        {
            if (IsInternetConnected() == true)
            {
                return true;
            }
            else
            {
                IsSearchBoxEnabled = false;

                //검색 결과 Listview 지우기
                SearchResults.Clear();
                //뜻풀이 감추기
                IsDefinitionViewerVisible = Visibility.Collapsed;

                var res = ResourceLoader.GetForCurrentView();
                ErrorMessageText = res.GetString("ErrorMessageNoInternet");
                IsErrorMessageVisible = Visibility.Visible;

                RaisePropertyChanged("IsSearchBoxEnabled", "IsDefinitionViewerVisible",
                                     "Definitions", "ErrorMessageText", "IsErrorMessageVisible");
                return false;
            }
        }
    }
}
