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
using 표준국어대사전.Views;
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

        // XAML ItemsSource 바인딩용 (BindableVector 설명 참고)
        private BindableVector searchResultsSource;
        public BindableVector SearchResultsSource
        {
            get
            {
                if (searchResultsSource == null || searchResultsSource.Source != SearchResults)
                    searchResultsSource = new BindableVector(SearchResults);
                return searchResultsSource;
            }
        }
        
        // 뜻 풀이
        public WordDetailItem Definitions { get; private set; }

        public bool CanGoBack { get { return History.CanGoBack; } }
        public bool CanGoForward { get { return History.CanGoForward; } }
        public Visibility IsMasterProgressBarVisible { get; private set; }
        public string Query { get; set; }
        public bool IsSearchBoxEnabled { get; private set; }

        public int SearchResultSelectedIndex { get; set; }
        public Visibility IsMoreButtonVisible { get; private set; }

        #region 검색 옵션
        // 검색 옵션 (앱을 다시 켜면 기본값: 표제어·일치)
        public SearchMethod SearchMethod { get; private set; } = SearchMethod.Exact;
        public SearchTarget SearchTarget { get; private set; } = SearchTarget.Headword;

        // 검색창 아래 상태 줄: 자동 포함 검색 안내가 우선, 없으면 기본값이 아닌 검색 옵션 요약
        private string fallbackNotice = null;
        public string SearchStatusText
        {
            get
            {
                if (!string.IsNullOrEmpty(fallbackNotice))
                    return fallbackNotice;

                SearchRequest options = new SearchRequest("", SearchMethod, SearchTarget);
                if (options.IsDefault)
                    return "";

                var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                string target = res.GetString(options.Target == SearchTarget.Definition ? "SearchTargetDefinition" : "SearchTargetHeadwordShort");
                string method = res.GetString("SearchMethod" + options.Method.ToString());
                return string.Format(res.GetString("SearchOptionSummaryFormat"), target, method);
            }
        }
        public Visibility IsSearchStatusVisible => string.IsNullOrEmpty(SearchStatusText) ? Visibility.Collapsed : Visibility.Visible;

        private void SetFallbackNotice(string notice)
        {
            fallbackNotice = notice;
            RaisePropertyChanged("SearchStatusText", "IsSearchStatusVisible");
        }

        /// <summary>검색 옵션을 바꾸고, 검색어가 있으면 새 옵션으로 다시 검색한다.</summary>
        public void SetSearchOptions(SearchMethod method, SearchTarget target)
        {
            if (method == SearchMethod && target == SearchTarget)
                return;

            SearchMethod = method;
            SearchTarget = target;
            fallbackNotice = null;
            RaisePropertyChanged("SearchStatusText", "IsSearchStatusVisible");

            if (!string.IsNullOrWhiteSpace(Query))
                SearchWords();
        }
        #endregion
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


        public async void SearchWords()
        {
            IsErrorMessageVisible = Visibility.Collapsed;
            RaisePropertyChanged("IsErrorMessageVisible");
            SetFallbackNotice(null);

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

            SearchRequest request = new SearchRequest(query, SearchMethod, SearchTarget);
            lastSearchedQuery = request.Encode();
            WordFinder wordFinder = new WordFinder(SearchResults, (Visibility visibility) => {
                if (visibility == Visibility.Collapsed)
                    LastLoadAddedItems = SearchResults.Count > countBeforeLoad;
                IsMasterProgressBarVisible = visibility;
                RaisePropertyChanged("IsMasterProgressBarVisible");
            }, (string text) =>
            {
                ErrorMessageText = text;
                IsErrorMessageVisible = Visibility.Visible;
                RaisePropertyChanged("ErrorMessageText", "IsErrorMessageVisible");
            });
            Action<Visibility> setMoreButtonVisibility = (visibility) => {
                IsMoreButtonVisible = visibility;
                RaisePropertyChanged("IsMoreButtonVisible");
            };

            RaisePropertyChanged("CanGoBack", "CanGoForward");
            //최근 검색어 기록
            RecentWordManager.Append(Query);

            // 기본 검색(표제어·일치)은 결과가 없으면 포함 검색으로 다시 찾으므로 '결과 없음'을 바로 표시하지 않는다
            countBeforeLoad = 0;
            int total = await wordFinder.GetSearchResults(1, 10, request, IsMoreButtonVisible, setMoreButtonVisibility,
                                                          reportNoResult: !request.IsDefault);

            if (total == 0 && request.IsDefault && lastSearchedQuery == request.Encode())
            {
                SearchRequest includeRequest = request.WithMethod(SearchMethod.Include);
                lastSearchedQuery = includeRequest.Encode();
                countBeforeLoad = 0;
                int includeTotal = await wordFinder.GetSearchResults(1, 10, includeRequest, IsMoreButtonVisible, setMoreButtonVisibility);

                if (includeTotal > 0 && lastSearchedQuery == includeRequest.Encode())
                {
                    var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                    SetFallbackNotice(string.Format(res.GetString("SearchFallbackNotice"), query));
                }
            }
        }

        // 스크롤 자동 추가 로딩
        private int countBeforeLoad;
        // 직전 검색·추가 로딩으로 항목이 늘었는지 (실패한 로딩을 자동으로 반복하지 않기 위해 사용)
        public bool LastLoadAddedItems { get; private set; }

        // 남은 결과가 있고 불러오는 중이 아닐 때만 다음 페이지를 불러온다
        public void TryLoadMore()
        {
            if (IsMoreButtonVisible == Visibility.Visible && IsMasterProgressBarVisible != Visibility.Visible && SearchResults.Count > 0)
                SearchMore();
        }

        public void SearchMore()
        {
            // 포함·뜻풀이 검색은 어깨번호가 0이므로, 지금까지 받은 항목 수로 다음 페이지를 계산한다
            int start = SearchResults.Count / 10 + 1;

            WordFinder wordFinder = new WordFinder(SearchResults, (Visibility visibility) => {
                if (visibility == Visibility.Collapsed)
                    LastLoadAddedItems = SearchResults.Count > countBeforeLoad;
                IsMasterProgressBarVisible = visibility;
                RaisePropertyChanged("IsMasterProgressBarVisible");
            }, (string text) =>
            {
                ErrorMessageText = text;
                IsErrorMessageVisible = Visibility.Visible;
                RaisePropertyChanged("ErrorMessageText", "IsErrorMessageVisible");
            });
            countBeforeLoad = SearchResults.Count;
            _ = wordFinder.GetSearchResults(start, 10, SearchRequest.Decode(lastSearchedQuery), IsMoreButtonVisible, (visibility) => {
                IsMoreButtonVisible = visibility;
                RaisePropertyChanged("IsMoreButtonVisible");
            });
        }

        // 뜻풀이 요청 순번: 빠르게 여러 단어를 고를 때 늦게 도착한 이전 응답이 최신 뜻풀이를 덮어쓰지 않도록 한다
        private int detailRequestId = 0;

        /// <param name="historySelectedIndex">뒤로 가기 기록에 남길 이전 선택 위치. 키보드로 이동할 때는
        /// 선택이 이미 바뀐 뒤라 이동 전 위치를 따로 넘긴다. 생략하면 현재 선택 위치를 쓴다.</param>
        public async void DisplayWordDetail(SearchResultItem clickedItem, int? historySelectedIndex = null)
        {
            int requestId = ++detailRequestId;

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
            History.RecordDefinition(Query, lastSearchedQuery, Definitions, historySelectedIndex ?? SearchResultSelectedIndex, false, IsMoreButtonVisible);
            //뜻풀이 감추기
            IsDefinitionViewerVisible = Visibility.Collapsed;

            IsDetailGridVisible = Visibility.Visible;
            if (!IsWidthBigEnough)
                IsTitleBarBackButtonEnabled = true;
            RaisePropertyChanged("IsDefinitionViewerVisible", "IsDetailGridVisible", "IsTitleBarBackButtonEnabled");

            DefinitionParser definitionParser = new DefinitionParser((Visibility visibility) => {
                if (requestId != detailRequestId)
                    return;
                IsDetailProgressBarVisible = visibility;
                RaisePropertyChanged("IsDetailProgressBarVisible");
            }, HandleHyperlinkClick);
            WordDetailItem definitionItem = await definitionParser.GetWordDetail(clickedItem.target_code.ToString(), clickedItem.word, clickedItem.sup_no);
            if (requestId != detailRequestId)
                return; // 그사이 다른 단어를 골랐으면 이 응답은 버린다

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
            SetFallbackNotice(null);

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
            SetFallbackNotice(null);

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
            SetFallbackNotice(null);
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
            Hyperlink hyperlink = sender;
            if (hyperlink.FindName("DetailGrid") != null)
            {
                Grid DetailGrid = hyperlink.FindName("DetailGrid") as Grid;

                if (DetailGrid.FindName("HyperViewer") == null)
                {
                    HyperViewer HyperViewer = new HyperViewer();
                    HyperViewer.Name = "HyperViewer";
                    int sup_no;
                    if (2 < hyperlink.Inlines.Count)
                        int.TryParse(NumberConvertor.SupToNumber((hyperlink.Inlines[1] as Run).Text), out sup_no);
                    else
                        int.TryParse(Regex.Replace((hyperlink.Inlines[0] as Run).Text, "[^0-9.]", ""), out sup_no);

                    string target_code = hyperlink.Inlines[hyperlink.Inlines.Count - 1].FontFamily.Source;
                    if (target_code == "0")
                        HyperViewer.SearchWords((hyperlink.Inlines[0] as Run).Text);
                    else
                        HyperViewer.DisplayWordDetail(target_code, sup_no);

                    DetailGrid.Children.Add(HyperViewer);
                }
            }
            else if (hyperlink.FindName("HyperViewerGrid") != null)
            {
                // 현재 표시 중인 HyperViewer 존재시
                Grid HyperViewerGrid = hyperlink.FindName("HyperViewerGrid") as Grid;
                HyperViewer HyperViewer = HyperViewerGrid.Parent as HyperViewer;
                int sup_no;
                if (2 < hyperlink.Inlines.Count)
                    int.TryParse(NumberConvertor.SupToNumber((hyperlink.Inlines[1] as Run).Text), out sup_no);
                else
                    int.TryParse(Regex.Replace((hyperlink.Inlines[0] as Run).Text, "[^0-9.]", ""), out sup_no);

                string target_code = hyperlink.Inlines[hyperlink.Inlines.Count - 1].FontFamily.Source;
                if (target_code == "0")
                    HyperViewer.SearchWords((hyperlink.Inlines[0] as Run).Text);
                else
                    HyperViewer.DisplayWordDetail(target_code, sup_no);
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
