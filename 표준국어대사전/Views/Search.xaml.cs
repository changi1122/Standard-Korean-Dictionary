using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Popups;
using System.Collections.ObjectModel;
using Windows.Networking.Connectivity;
using Windows.ApplicationModel.Resources;
using 표준국어대사전.Classes;
using 표준국어대사전.ViewModels;


namespace 표준국어대사전.Views
{
    public sealed partial class Search : Page
    {
        const int MASTERGRID_WIDTH = 310;
        const int MULTISEARCHGRID_WIDTH = 400;

        // 더 보기 버튼 표시 여부
        private Visibility isMoreButtonVisible;

        // 검색 결과 리스트뷰에 바인딩
        private ObservableCollection<SearchResultItem> SearchResults;

        // 단어 정의에 바인딩
        private ObservableCollection<WordDetailItem> Definitions;

        private bool IsWebViewOpen = false;
        private static bool IsHomepageVisible = true;
        private static bool IsFirstPageOpen = true;

        public Visibility IsDefinitionViewerVisible
        {
            get { return (Definitions[0].target_code != null) ? Visibility.Visible : Visibility.Collapsed; }
        }

        private HistoryManager History;

        private SearchViewModel ViewModel;

        public Search()
        {
            this.ViewModel = new SearchViewModel(ref IsFirstPageOpen);

            this.InitializeComponent();

            isMoreButtonVisible = Visibility.Collapsed;

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

            // 뒤로가기 버튼 이벤트 핸들러 추가
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.BackRequested += CloseDetailGrid;
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
            BtnMore.Visibility = isMoreButtonVisible;

            DefinitionViewer.Visibility = IsDefinitionViewerVisible;
            var currentView = SystemNavigationManager.GetForCurrentView();
            if (currentView.AppViewBackButtonVisibility == AppViewBackButtonVisibility.Visible
                && IsDefinitionViewerVisible == Visibility.Collapsed)
            {
                Definitions[0] = new WordDetailItem();
                
                DetailGrid.Visibility = Visibility.Collapsed;
                currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
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

        /// <summary>
        /// 단어 검색시 실행
        /// </summary>
        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (SearchBox.Text == "")
            {
                var res = ResourceLoader.GetForCurrentView();
                var messageDialog = new MessageDialog(res.GetString("DA_NoSearchText"));
                await messageDialog.ShowAsync();
                return;
            }

            ViewModel.SearchWords();
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SearchBox_QuerySubmitted(SearchBox, new AutoSuggestBoxQuerySubmittedEventArgs());
        }

        /// <summary>
        /// 검색 결과 리스트뷰의 항목을 클릭시 실행
        /// </summary>
        private void ListviewSearchResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = e.ClickedItem as SearchResultItem;

            ViewModel.DisplayWordDetail(clickedItem);
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

        private void BasicGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //반응형
            if (BasicGrid.ActualWidth >= 686)
            {
                MasterGrid.Margin = new Thickness(10, 50, 0, 10);
                MasterGrid.Width = MASTERGRID_WIDTH;
                MasterGrid.HorizontalAlignment = HorizontalAlignment.Left;
                DetailGrid.Margin = new Thickness(MASTERGRID_WIDTH + 20, 50, 10, 0);
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
                MasterGrid.Margin = new Thickness(10, 50, 10, 10);
                MasterGrid.ClearValue(WidthProperty);
                MasterGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                DetailGrid.Margin = new Thickness(10, 50, 10, 0);
                DetailGrid.Visibility = Visibility.Collapsed;
                if (BasicGrid.FindName("MultiSearchGrid") != null)
                {
                    Grid g = (Grid)BasicGrid.FindName("MultiSearchGrid");
                    g.HorizontalAlignment = HorizontalAlignment.Stretch;
                    g.ClearValue(WidthProperty);
                }
            }

            //뒤로가기 키 지우기
            HideBackButton();
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
            MultiSearchFrame.Navigate(typeof(Views.Search));
            MultiSearchGrid.Children.Add(MultiSearchFrame);

            BasicGrid.Children.Add(MultiSearchGrid);
        }

        private void BtnMultiSearchClose_Click(object sender, RoutedEventArgs e)
        {
            BasicGrid.Children.Remove((UIElement)this.FindName("MultiSearchGrid"));
        }

        private void ShowBackButton()
        {
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void HideBackButton()
        {
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void CloseDetailGrid(object sender, BackRequestedEventArgs e)
        {

            //뜻풀이 감추기
            Definitions[0] = new WordDetailItem();
            UpdateControls();

            DetailGrid.Visibility = Visibility.Collapsed;
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
    }
}