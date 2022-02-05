using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.ApplicationModel.Resources;
using 표준국어대사전.Models;
using 표준국어대사전.Classes;
using 표준국어대사전.Controls;
using 표준국어대사전.Utils;

namespace 표준국어대사전.ViewModels
{
    internal class SearchViewModel : INotifyPropertyChanged
    {
        public bool IsWidthBigEnough;
        private HistoryManager History;
        private string lastSearchedQuery;
        Action SearchBoxFocus;

        public SearchViewModel(ref bool IsFirstPageOpen, Action searchBoxFocus)
        {
            this.IsWidthBigEnough = true;
            this.History = new HistoryManager();
            this.SearchBoxFocus = searchBoxFocus;
            this.IsMasterProgressBarVisible = Visibility.Collapsed;
            this.Query = "";
            this.IsSearchBoxEnabled = true;
            this.IsMoreButtonVisible = Visibility.Collapsed;
            this.IsErrorMessageVisible = Visibility.Collapsed;
            this.IsDetailProgressBarVisible = Visibility.Collapsed;
            this.IsDefinitionViewerVisible = Visibility.Collapsed;
            this.IsDetailGridVisible = Visibility.Visible;

            if (IsFirstPageOpen)
            {
                this.SearchResults = new ObservableCollection<SearchResultItem>(SearchResultStaticPage.GetHomeTab());
                this.Definitions = WordDetailStaticPage.GetHomepage(HandleHyperlinkClick, HandleRecentWordClick);
                this.IsDefinitionViewerVisible = Visibility.Visible;
                this.SearchResultSelectedIndex = 0;
                IsFirstPageOpen = false;
            }
            else
            {
                this.SearchResults = new ObservableCollection<SearchResultItem>();
                this.Definitions = new WordDetailItem(HandleHyperlinkClick);
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
        public Visibility IsMasterProgressBarVisible { get; private set; }
        public string Query { get; set; }
        public bool IsSearchBoxEnabled { get; private set; }

        public int SearchResultSelectedIndex { get; set; }
        public Visibility IsMoreButtonVisible { get; private set; }
        public Visibility IsErrorMessageVisible { get; private set; }
        public string ErrorMessageText { get; private set; }
        public Visibility IsDetailProgressBarVisible { get; private set; }
        public Visibility IsDefinitionViewerVisible { get; private set; }

        public Visibility IsDetailGridVisible { get; private set; }
        public bool IsTitleBarBackButtonEnabled
        {
            get { return IsTitleBarBackButtonEnabled; }
            set
            {
                var currentView = SystemNavigationManager.GetForCurrentView();
                currentView.AppViewBackButtonVisibility = (value) ? AppViewBackButtonVisibility.Visible
                                                                  : AppViewBackButtonVisibility.Collapsed;
            }
        }


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
            WordFinder wordFinder = new WordFinder(SearchResults, (Visibility visibility) => {
                IsMasterProgressBarVisible = visibility;
                RaisePropertyChanged("IsMasterProgressBarVisible");
            }, (string text) =>
            {
                ErrorMessageText = text;
                IsErrorMessageVisible = Visibility.Visible;
                RaisePropertyChanged("ErrorMessageText", "IsErrorMessageVisible");
            });
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

            WordFinder wordFinder = new WordFinder(SearchResults, (Visibility visibility) => {
                IsMasterProgressBarVisible = visibility;
                RaisePropertyChanged("IsMasterProgressBarVisible");
            }, (string text) =>
            {
                ErrorMessageText = text;
                IsErrorMessageVisible = Visibility.Visible;
                RaisePropertyChanged("ErrorMessageText", "IsErrorMessageVisible");
            });
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
                Definitions = WordDetailStaticPage.GetHomepage(HandleHyperlinkClick, HandleRecentWordClick);
                IsDetailGridVisible = Visibility.Visible;
                if (!IsWidthBigEnough)
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
            if (!IsWidthBigEnough)
                IsTitleBarBackButtonEnabled = true;
            RaisePropertyChanged("IsDefinitionViewerVisible", "IsDetailGridVisible", "IsTitleBarBackButtonEnabled");

            DefinitionParser definitionParser = new DefinitionParser((Visibility visibility) => {
                IsDetailProgressBarVisible = visibility;
                RaisePropertyChanged("IsDetailProgressBarVisible");
            }, HandleHyperlinkClick);
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
            IsDefinitionViewerVisible = (selectedIndex < 0) ? Visibility.Collapsed : Visibility.Visible;
            IsMoreButtonVisible = isMoreButtonVisible;
            RaisePropertyChanged("Query", "Definitions", "SearchResultSelectedIndex", "IsDefinitionViewerVisible",
                                 "IsMoreButtonVisible", "CanGoBack", "CanGoForward");
            
            // 작은 창일 때
            if (!IsWidthBigEnough)
            {
                IsDetailGridVisible = (selectedIndex < 0) ? Visibility.Collapsed : Visibility.Visible;
                IsTitleBarBackButtonEnabled = (selectedIndex < 0) ? false : true;
                RaisePropertyChanged("IsDetailGridVisible");
            }
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
            IsDefinitionViewerVisible = (selectedIndex < 0) ? Visibility.Collapsed : Visibility.Visible;
            IsMoreButtonVisible = isMoreButtonVisible;
            RaisePropertyChanged("Query", "Definitions", "SearchResultSelectedIndex", "IsDefinitionViewerVisible",
                                 "IsMoreButtonVisible", "CanGoBack", "CanGoForward");

            // 작은 창일 때
            if (!IsWidthBigEnough)
            {
                IsDetailGridVisible = (selectedIndex < 0) ? Visibility.Collapsed : Visibility.Visible;
                IsTitleBarBackButtonEnabled = (selectedIndex < 0) ? false : true;
                RaisePropertyChanged("IsDetailGridVisible");
            }
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
            Definitions = WordDetailStaticPage.GetHomepage(HandleHyperlinkClick, HandleRecentWordClick);
            IsDefinitionViewerVisible = Visibility.Visible;
            RaisePropertyChanged("Query", "SearchResultSelectedIndex", "Definitions", "IsDefinitionViewerVisible");
        }

        public void CloseDetailGrid(object sender, BackRequestedEventArgs e)
        {
            //뜻풀이 감추기
            IsDetailGridVisible = Visibility.Collapsed;
            RaisePropertyChanged("IsDetailGridVisible");

            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        public void NetStatusRefresh()
        {
            if (NetworkCheck() == true)
            {
                IsSearchBoxEnabled = true;
                IsErrorMessageVisible = Visibility.Collapsed;
                RaisePropertyChanged("IsSearchBoxEnabled", "IsErrorMessageVisible");
                SearchBoxFocus();
            }
        }

        public void SetDetailGridVisible()
        {
            IsDetailGridVisible = Visibility.Visible;
            RaisePropertyChanged("IsDetailGridVisible");
        }

        public void SetDetailGridCollapsed()
        {
            IsDetailGridVisible = Visibility.Collapsed;
            RaisePropertyChanged("IsDetailGridVisible");
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

        private void HandleHyperlinkClick(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            // TO-DO
            // '태허0' 뜻풀이의 '하늘' 링크처럼 어깨번호가 명확하지 않을 때는 검색 API로 검색 후
            // '하늘'에 해당하는 단어 중 선택할 수 있게 만들기.

            // TO-DO
            // ConWordDetail과 함수 두 개로 하는 일 분리하기

            // TO-DO
            // HyperViewer 아래 부분에 Margin 주기

            Hyperlink hyperlink = sender;
            if (hyperlink.FindName("DetailGrid") != null)
            {
                Grid DetailGrid = hyperlink.FindName("DetailGrid") as Grid;

                if (DetailGrid.FindName("HyperViewer") == null)
                {
                    ConWordDetail HyperViewer = new ConWordDetail();
                    HyperViewer.Name = "HyperViewer";
                    int sup_no;
                    if (2 < hyperlink.Inlines.Count)
                        int.TryParse(NumberConvertor.SupToNumber((hyperlink.Inlines[1] as Run).Text), out sup_no);
                    else
                        int.TryParse(Regex.Replace((hyperlink.Inlines[0] as Run).Text, "[^0-9.]", ""), out sup_no);
                    HyperViewer.Load_WordDetail(hyperlink.Inlines[hyperlink.Inlines.Count - 1].FontFamily.Source, sup_no);

                    DetailGrid.Children.Add(HyperViewer);
                }
            }
            else if (hyperlink.FindName("ConWordDetailGrid") != null)
            {
                // 현재 표시 중인 ConWordDetail 존재시
                Grid ConWordDetailGrid = hyperlink.FindName("ConWordDetailGrid") as Grid;
                ConWordDetail HyperViewer = ConWordDetailGrid.Parent as ConWordDetail;
                int sup_no;
                if (2 < hyperlink.Inlines.Count)
                    int.TryParse(NumberConvertor.SupToNumber((hyperlink.Inlines[1] as Run).Text), out sup_no);
                else
                    int.TryParse(Regex.Replace((hyperlink.Inlines[0] as Run).Text, "[^0-9.]", ""), out sup_no);
                HyperViewer.Load_WordDetail(hyperlink.Inlines[hyperlink.Inlines.Count - 1].FontFamily.Source, sup_no);
            }
        }

        private void HandleRecentWordClick(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            Hyperlink hyperlink = sender;

            // 검색어 설정
            Run run = hyperlink.Inlines[0] as Run;
            if (run != null)
                Query = run.Text;
            RaisePropertyChanged("Query");

            // 작은 창일 때 DetailGrid 숨기기
            if (!IsWidthBigEnough)
            {
                SearchResultSelectedIndex = -1;
                IsDetailGridVisible = Visibility.Collapsed;
                IsTitleBarBackButtonEnabled = false;
                RaisePropertyChanged("SearchResultSelectedIndex", "IsDetailGridVisible", "IsTitleBarBackButtonEnabled");
            }

            SearchBoxFocus();
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
