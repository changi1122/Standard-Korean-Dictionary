using 표준국어대사전.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using Windows.Management.Deployment;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.Connectivity;
using System.Net.Http;
using System.Xml.Linq;
using Windows.ApplicationModel.Resources;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace 표준국어대사전.Pages
{
    public sealed partial class DicAppSearch : Page
    {
        const int MASTERGRID_WIDTH = 320;
        const int MULTISEARCHGRID_WIDTH = 400;

        //검색 결과 리스트뷰에 바인딩
        private ObservableCollection<SearchResultItem> SearchResults;
        
        //단어 정의에 바인딩
        private ObservableCollection<WordDetailItem> Definitions;

        private bool IsWebViewOpen = false;
        private static bool IsHomepageVisible = true;

        public Visibility IsDefinitionViewerVisible
        {
            get { return (Definitions[0].target_code != null) ? Visibility.Visible : Visibility.Collapsed; }
        }

        private HistoryManager History;


        public DicAppSearch()
        {
            this.InitializeComponent();

            History = new HistoryManager();

            if (IsHomepageVisible)
            {
                SearchResults = new ObservableCollection<SearchResultItem>(SearchResultStaticPage.GetHomeTab());
                Definitions = new ObservableCollection<WordDetailItem>();
                Definitions.Add(WordDetailStaticPage.GetHomepage());
                ListviewSearchResult.SelectedIndex = 0;
                IsHomepageVisible = false;
            }
            else
            {
                SearchResults = new ObservableCollection<SearchResultItem>();
                Definitions = new ObservableCollection<WordDetailItem>();
                Definitions.Add(new WordDetailItem());
            }

            UpdateControls();

            NetworkCheck();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// 바인딩과 유사하게 필요할 때마다 엘리먼트 속성을 관리
        /// </summary>
        private void UpdateControls()
        {
            BtnBack.IsEnabled = History.CanGoBack;
            BtnForward.IsEnabled = History.CanGoForward;

            DefinitionViewer.Visibility = IsDefinitionViewerVisible;
            if (BtnCloseDetail.Visibility == Visibility.Visible && IsDefinitionViewerVisible == Visibility.Collapsed)
            {
                Definitions[0] = new WordDetailItem();
                
                DetailGrid.Visibility = Visibility.Collapsed;
                BtnCloseDetail.Visibility = Visibility.Collapsed;
            }
        }

        private static bool IsInternetConnected()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = (connections != null) &&
                (connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
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
                SearchBox.IsEnabled = false;

                //검색 결과 Listview 지우기
                SearchResults.Clear();
                //뜻풀이 감추기
                Definitions[0] = new WordDetailItem();
                UpdateControls();
                var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                TextBlockErrorMessage.Text = res.GetString("ErrorMessageNoInternet");
                TextBlockErrorMessage.Visibility = Visibility.Visible;
                BtnNetStatusRefresh.Visibility = Visibility.Visible;
                return false;
            }
        }

        private void BtnNetStatusRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkCheck() == true)
            {
                SearchBox.IsEnabled = true;

                TextBlockErrorMessage.Visibility = Visibility.Collapsed;
                BtnNetStatusRefresh.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 검색 결과 리스트뷰의 항목을 클릭시 실행
        /// </summary>
        private async void ListviewSearchResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (NetworkCheck() == false)
                return;

            var clickedItem = (SearchResultItem)e.ClickedItem;

            if (clickedItem.target_code == -321)
            {
                //더보기 누를 시 동작
                WordFinder wordFinder = new WordFinder(SearchResults, MasterProgressBar, TextBlockErrorMessage);
                wordFinder.GetSearchResults(clickedItem.sup_no + 1, 10, clickedItem.definition);
                SearchResults.Remove(clickedItem);
                return;
            }
            else if (clickedItem.target_code == -200)
            {
                //시작 누를 시 동작
                Definitions[0] = WordDetailStaticPage.GetHomepage();

                if (BasicGrid.ActualWidth < 686)
                {
                    DetailGrid.Visibility = Visibility.Visible;
                    BtnCloseDetail.Visibility = Visibility.Visible;
                }
                UpdateControls();
                return;
            }

            //일반 단어 클릭시 동작

            //되돌리기 위한 기록
            History.RecordDefinition(SearchBox.Text, Definitions[0], ListviewSearchResult.SelectedIndex);
            //뜻풀이 감추기
            Definitions[0] = new WordDetailItem();
            UpdateControls();

            if (BasicGrid.ActualWidth < 686)
            {
                DetailGrid.Visibility = Visibility.Visible;
                BtnCloseDetail.Visibility = Visibility.Visible;
            }

            DefinitionParser definitionParser = new DefinitionParser(DetailProgressBar);
            WordDetailItem definitionItem = await definitionParser.GetWordDetail(clickedItem.target_code.ToString(), clickedItem.word, clickedItem.sup_no);
            if (definitionItem != null)
            {
                Definitions[0] = definitionItem;
            }

            //뜻풀이 보이기
            UpdateControls();
        }

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            TextBlockErrorMessage.Visibility = Visibility.Collapsed; //검색 결과 없음. 표시 지우기

            if (NetworkCheck() == false)
                return;

            //검색어 숫자 지우기
            string searchText = SearchBox.Text;
            while (searchText.Length >= 1 && char.IsNumber(searchText[searchText.Length - 1]))
            {
                searchText = searchText.Substring(0, searchText.Length - 1);
            }

            if (searchText == "")
            {
                var res = ResourceLoader.GetForCurrentView();
                var messageDialog = new MessageDialog(res.GetString("DA_NoSearchText"));
                await messageDialog.ShowAsync();
                return;
            }

            //되돌리기 위한 기록
            History.RecordAll(SearchBox.Text, SearchResults, Definitions[0], ListviewSearchResult.SelectedIndex);
            Definitions[0] = new WordDetailItem();
            //검색 결과 Listview 지우기
            SearchResults.Clear();
            //뜻풀이 감추기
            UpdateControls();

            WordFinder wordFinder = new WordFinder(SearchResults, MasterProgressBar, TextBlockErrorMessage);
            wordFinder.GetSearchResults(1, 10, searchText);

            //최근 검색 기록
            RecentWordManager.Append(searchText);
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SearchBox_QuerySubmitted(SearchBox, new AutoSuggestBoxQuerySubmittedEventArgs());
            }
        }

        private void BtnSubSearchClose_Click(object sender, RoutedEventArgs e)
        {
            BasicGrid.Children.Remove((UIElement)FindName("SubGrid"));
            IsWebViewOpen = false;
        }

        private void WebViewOpen(Uri uri)
        {
            if (IsWebViewOpen == true)
            {
                Grid g = (Grid)BasicGrid.FindName("SubGrid");
                WebView w = (WebView)g.FindName("SubSearch");
                w.Navigate(uri);
            }
            else
            {
                var SubGrid = new Grid
                {
                    Name = "SubGrid",
                    Margin = new Thickness(0, 40, 0, 0),
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = new SolidColorBrush(Windows.UI.Colors.White)
                };

                var ColorBar = new Windows.UI.Xaml.Shapes.Rectangle
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Height = 40,
                    Fill = (SolidColorBrush)Resources["BarColor"]
                };
                SubGrid.Children.Add(ColorBar);

                var CloseBtn = new Button
                {
                    Name = "BtnSubSearchClose",
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Height = 40,
                    Width = 40,
                    FontSize = 20,
                    Content = "",
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Background = null,
                    Foreground = new SolidColorBrush(Windows.UI.Colors.Black)
                };
                CloseBtn.Click += BtnSubSearchClose_Click;
                SubGrid.Children.Add(CloseBtn);

                var SubSearch = new WebView
                {
                    Name = "SubSearch",
                    Margin = new Thickness(1, 40, 1, 1),
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Source = uri
                };
                SubGrid.Children.Add(SubSearch);

                BasicGrid.Children.Add(SubGrid);
                IsWebViewOpen = true;
            }
        }

        private void BtnInform_Click(object sender, RoutedEventArgs e)
        {
            WebViewOpen(new Uri("https://stdict.korean.go.kr/help/popup/entry.do"));
        }

        private async void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://costudio1122.blogspot.com/p/blog-page_76.html"));
        }

        private void BtnMemo_Click(object sender, RoutedEventArgs e)
        {
            Controls.ConMemo conMemo = MasterGrid.FindName("Memo") as Controls.ConMemo;

            if (conMemo != null)
            {
                MasterGrid.Children.Remove(conMemo);
            }
            else
            {
                Controls.ConMemo memo = new Controls.ConMemo();
                memo.Name = "Memo";
                memo.VerticalAlignment = VerticalAlignment.Bottom;

                MasterGrid.Children.Add(memo);
            }
        }

        private void BasicGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //반응형
            if (BasicGrid.ActualWidth >= 686)
            {
                MasterGrid.Margin = new Thickness(0, 40, 0, 0);
                MasterGrid.Width = MASTERGRID_WIDTH;
                MasterGrid.HorizontalAlignment = HorizontalAlignment.Left;
                DetailGrid.Margin = new Thickness(MASTERGRID_WIDTH, 40, 0, 0);
                DetailGrid.Visibility = Visibility.Visible;
                if (BasicGrid.FindName("MultiSearchGrid") != null)
                {
                    Grid g = (Grid)BasicGrid.FindName("MultiSearchGrid");
                    g.HorizontalAlignment = HorizontalAlignment.Right;
                    g.Width = MULTISEARCHGRID_WIDTH;
                }
            }

            else if (BasicGrid.ActualWidth < 686)
            {
                MasterGrid.Margin = new Thickness(0, 40, 0, 0);
                MasterGrid.ClearValue(WidthProperty);
                MasterGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                DetailGrid.Margin = new Thickness(0, 40, 0, 0);
                DetailGrid.Visibility = Visibility.Collapsed;
                if (BasicGrid.FindName("MultiSearchGrid") != null)
                {
                    Grid g = (Grid)BasicGrid.FindName("MultiSearchGrid");
                    g.HorizontalAlignment = HorizontalAlignment.Stretch;
                    g.ClearValue(WidthProperty);
                }
            }

            //뒤로가기 키 지우기
            BtnCloseDetail.Visibility = Visibility.Collapsed;
        }

        private void BtnCloseDetail_Click(object sender, RoutedEventArgs e)
        {
            //뜻풀이 감추기
            Definitions[0] = new WordDetailItem();
            UpdateControls();

            DetailGrid.Visibility = Visibility.Collapsed;
            BtnCloseDetail.Visibility = Visibility.Collapsed;
        }

        private void BtnMultiSearch_Click(object sender, RoutedEventArgs e)
        {
            if (BasicGrid.FindName("MultiSearchGrid") != null)
            {
                BtnMultiSearchClose_Click(sender, e);
                return;
            }

            var MultiSearchGrid = new Grid
            {
                Name = "MultiSearchGrid",
                Margin = new Thickness(0, 40, 0, 0),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = (SolidColorBrush)Resources["BarColor"],
                Width = MULTISEARCHGRID_WIDTH
            };

            if (BasicGrid.ActualWidth < 686)
            {
                MultiSearchGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                MultiSearchGrid.ClearValue(WidthProperty);
            }

            var CloseBtn = new Button
            {
                Name = "BtnMultiSearchClose",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Height = 40,
                Width = 40,
                FontSize = 20,
                Content = "",
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Background = null,
                Foreground = new SolidColorBrush(Windows.UI.Colors.Black)
            };
            CloseBtn.Click += BtnMultiSearchClose_Click;
            MultiSearchGrid.Children.Add(CloseBtn);

            var MultiSearchFrame = new Frame
            {
                Name = "MultiSearchFrame",
                Margin = new Thickness(1, 40, 1, 1),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            MultiSearchFrame.Navigate(typeof(Pages.DicAppSearch));
            MultiSearchGrid.Children.Add(MultiSearchFrame);

            BasicGrid.Children.Add(MultiSearchGrid);
        }

        private void BtnMultiSearchClose_Click(object sender, RoutedEventArgs e)
        {
            BasicGrid.Children.Remove((UIElement)this.FindName("MultiSearchGrid"));
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text;
            WordDetailItem definition = Definitions[0];
            int selectedIndex = ListviewSearchResult.SelectedIndex;
            History.Undo(ref searchText, ref SearchResults, ref definition, ref selectedIndex);

            SearchBox.Text = searchText;
            if (definition != null)
                Definitions[0] = definition;
            ListviewSearchResult.SelectedIndex = selectedIndex;
            UpdateControls();
        }

        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text;
            WordDetailItem definition = Definitions[0];
            int selectedIndex = ListviewSearchResult.SelectedIndex;
            History.Redo(ref searchText, ref SearchResults, ref definition, ref selectedIndex);

            SearchBox.Text = searchText;
            if (definition != null)
                Definitions[0] = definition;
            ListviewSearchResult.SelectedIndex = selectedIndex;
            UpdateControls();
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            //되돌리기 위한 기록
            History.RecordAll(SearchBox.Text, SearchResults, Definitions[0], ListviewSearchResult.SelectedIndex);
            Definitions[0] = new WordDetailItem();

            //검색어 지우기
            SearchBox.Text = "";
            //검색 결과 Listview 지우기
            SearchResults.Clear();

            List<SearchResultItem> homeTab = SearchResultStaticPage.GetHomeTab();
            foreach(SearchResultItem item in homeTab)
            {
                SearchResults.Add(item);
            }
            ListviewSearchResult.SelectedIndex = 0;
            Definitions[0] = WordDetailStaticPage.GetHomepage();
            UpdateControls();
        }
    }
}